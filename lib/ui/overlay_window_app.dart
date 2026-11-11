// ignore_for_file: deprecated_member_use

import 'dart:async';
import 'dart:io';

import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:window_manager/window_manager.dart';

import '../core/app_settings.dart';
import '../core/runtime.dart';
import '../core/window_bridge.dart';
import 'overlay_screen.dart';

class OverlayWindowApp extends StatefulWidget {
  const OverlayWindowApp({super.key, required this.runtime});

  final AppRuntime runtime;

  @override
  State<OverlayWindowApp> createState() => _OverlayWindowAppState();
}

class _OverlayWindowAppState extends State<OverlayWindowApp>
    with WindowListener {
  Timer? _positionDebounce;
  bool _nativeReady = false;

  AppRuntime get runtime => widget.runtime;

  @override
  void initState() {
    super.initState();
    windowManager.addListener(this);
    unawaited(windowSettingsChannel.setMethodCallHandler(_handleSettingsCall));
    WidgetsBinding.instance.addPostFrameCallback((_) {
      unawaited(_initializeWindow());
    });
  }

  @override
  void dispose() {
    _positionDebounce?.cancel();
    windowManager.removeListener(this);
    unawaited(windowSettingsChannel.setMethodCallHandler(null));
    runtime.dispose();
    super.dispose();
  }

  Future<void> _initializeWindow() async {
    try {
      await windowManager.setPreventClose(true);
      final scale = runtime.settings.keyboardScale;
      await windowManager.waitUntilReadyToShow(
        WindowOptions(
          size: Size(1240 * scale, 520 * scale),
          center: false,
          alwaysOnTop: true,
          backgroundColor: Colors.transparent,
          skipTaskbar: true,
          title: '肌忆五笔 · 置顶键盘',
          titleBarStyle: TitleBarStyle.hidden,
          windowButtonVisibility: false,
        ),
        () async {
          await windowManager.setAsFrameless();
          if (!Platform.isLinux) {
            await windowManager.setHasShadow(false);
          }
          await windowManager.setAlwaysOnTop(true);
          await windowManager.setSkipTaskbar(true);
          await windowManager.setBackgroundColor(Colors.transparent);
          await windowManager.setOpacity(1);
          await _applyPlacement(runtime.settings);
          _nativeReady = true;
          await windowCommandsChannel.invokeMethod('overlayReady');
        },
      );
    } catch (error) {
      await runtime.log.error('Unable to initialize keyboard window.', error);
    }
  }

  Future<dynamic> _handleSettingsCall(MethodCall call) async {
    if (call.method != 'applySettings' || call.arguments is! Map) {
      return null;
    }
    final settings = AppSettings.fromJson(
      Map<String, dynamic>.from(call.arguments as Map),
    );
    runtime.applyExternalSettings(settings);
    if (_nativeReady) {
      await _applyPlacement(settings);
    }
    return true;
  }

  Future<void> _applyPlacement(AppSettings settings) async {
    await windowManager.setSize(
      Size(1240 * settings.keyboardScale, 520 * settings.keyboardScale),
    );
    if (settings.keyboardLeft >= 0 && settings.keyboardTop >= 0) {
      await windowManager.setPosition(
        Offset(settings.keyboardLeft, settings.keyboardTop),
      );
    } else {
      await windowManager.setAlignment(Alignment.bottomCenter);
    }
  }

  Future<void> _sendCommand(String method) async {
    try {
      await windowCommandsChannel.invokeMethod(method);
    } catch (error) {
      await runtime.log.error('Keyboard window command failed: $method', error);
    }
  }

  @override
  void onWindowClose() {
    unawaited(_sendCommand('hideOverlay'));
  }

  @override
  void onWindowMoved() {
    _positionDebounce?.cancel();
    _positionDebounce = Timer(const Duration(milliseconds: 350), () async {
      if (!_nativeReady) return;
      try {
        final position = await windowManager.getPosition();
        await windowCommandsChannel.invokeMethod('overlayMoved', {
          'left': position.dx,
          'top': position.dy,
        });
      } catch (error) {
        await runtime.log.error('Unable to persist keyboard position.', error);
      }
    });
  }

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: '肌忆五笔 · 置顶键盘',
      debugShowCheckedModeBanner: false,
      color: Colors.transparent,
      theme: ThemeData(
        useMaterial3: true,
        brightness: Brightness.dark,
        scaffoldBackgroundColor: Colors.transparent,
      ),
      home: OverlayScreen(
        runtime: runtime,
        onOpenMain: () => _sendCommand('showMain'),
        onHide: () => _sendCommand('hideOverlay'),
      ),
    );
  }
}
