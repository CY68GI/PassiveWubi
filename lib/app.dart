// The 0.6 window/tray packages keep the stable 0.5 bridge while native APIs
// are still evolving; isolate that compatibility layer here.
// ignore_for_file: deprecated_member_use

import 'dart:async';
import 'dart:io';

import 'package:desktop_multi_window/desktop_multi_window.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:tray_manager/legacy.dart';
import 'package:window_manager/window_manager.dart';

import 'core/app_settings.dart';
import 'core/practice_data.dart';
import 'core/runtime.dart';
import 'core/single_instance.dart';
import 'core/storage.dart';
import 'core/update_service.dart';
import 'core/window_bridge.dart';
import 'ui/main_shell.dart';

class PassiveWubiApp extends StatefulWidget {
  const PassiveWubiApp({
    super.key,
    required this.storage,
    required this.settings,
    required this.catalog,
    required this.log,
  });

  final AppStorage storage;
  final AppSettings settings;
  final PracticeCatalog catalog;
  final AppLogger log;

  @override
  State<PassiveWubiApp> createState() => _PassiveWubiAppState();
}

class _PassiveWubiAppState extends State<PassiveWubiApp>
    with WindowListener, TrayListener {
  late final AppRuntime runtime;
  late final _TrayController _tray;
  WindowController? _overlayController;
  Completer<void> _overlayReady = Completer<void>()..complete();
  Timer? _settingsSyncDebounce;
  bool _overlayVisible = false;
  bool _initialized = false;
  bool _exiting = false;
  StreamSubscription<void>? _activationSubscription;

  @override
  void initState() {
    super.initState();
    runtime = AppRuntime(
      storage: widget.storage,
      catalog: widget.catalog,
      settings: widget.settings,
      log: widget.log,
    );
    runtime.addListener(_onRuntimeChanged);
    _tray = _TrayController(this);
    unawaited(windowCommandsChannel.setMethodCallHandler(_handleWindowCommand));
    _activationSubscription = SingleInstance.activations.listen(
      (_) => unawaited(showMain()),
    );
    WidgetsBinding.instance.addPostFrameCallback((_) => _initializeDesktop());
  }

  @override
  void dispose() {
    runtime.removeListener(_onRuntimeChanged);
    _activationSubscription?.cancel();
    _settingsSyncDebounce?.cancel();
    windowManager.removeListener(this);
    trayManager.removeListener(this);
    unawaited(windowCommandsChannel.setMethodCallHandler(null));
    _tray.dispose();
    runtime.dispose();
    super.dispose();
  }

  Future<void> _initializeDesktop() async {
    try {
      await windowManager.setPreventClose(true);
      windowManager.addListener(this);
      await windowManager.waitUntilReadyToShow(
        WindowOptions(
          size: const Size(1120, 760),
          minimumSize: const Size(900, 640),
          center: true,
          title: '肌忆五笔 PassiveWubi',
          titleBarStyle: TitleBarStyle.normal,
          backgroundColor: _mainBackgroundColor,
        ),
        () async {
          await windowManager.show();
          await windowManager.focus();
        },
      );
      await _tray.initialize();
      await widget.log.info('Desktop window initialized.');
      _initialized = true;
      if (widget.settings.showKeyboardOnStartup) {
        await _showOverlay();
      } else {
        setState(() {});
      }
      if (!widget.settings.hasCompletedFirstRun) {
        widget.settings.hasCompletedFirstRun = true;
        await runtime.save();
      }
      unawaited(_checkUpdatesIfEnabled());
    } catch (error) {
      await widget.log.error('Unable to initialize desktop shell.', error);
      setState(() {});
    }
  }

  void _onRuntimeChanged() {
    if (!mounted) return;
    setState(() {});
    _settingsSyncDebounce?.cancel();
    _settingsSyncDebounce = Timer(
      const Duration(milliseconds: 60),
      () => unawaited(_sendOverlaySettings()),
    );
  }

  Color get _mainBackgroundColor =>
      widget.settings.theme == AppThemePreference.dark
      ? const Color(0xFF0B111B)
      : const Color(0xFFF2F4F8);

  Future<void> showMain() async {
    if (_exiting) return;
    if (!_initialized) return;
    await windowManager.setTitleBarStyle(TitleBarStyle.normal);
    await windowManager.setAlwaysOnTop(false);
    await windowManager.setSkipTaskbar(false);
    await windowManager.setOpacity(1);
    await windowManager.setBackgroundColor(_mainBackgroundColor);
    await windowManager.setSize(const Size(1120, 760));
    await windowManager.center();
    await windowManager.show();
    await windowManager.focus();
  }

  Future<WindowController> _ensureOverlayController() async {
    final existing = _overlayController;
    if (existing != null) return existing;

    for (final controller in await WindowController.getAll()) {
      if (windowKindFromArguments(controller.arguments) ==
          AppWindowKind.keyboard) {
        _overlayController = controller;
        return controller;
      }
    }

    _overlayReady = Completer<void>();
    final controller = await WindowController.create(
      const WindowConfiguration(
        hiddenAtLaunch: true,
        arguments: keyboardWindowArguments,
      ),
    );
    _overlayController = controller;
    return controller;
  }

  Future<void> _showOverlay() async {
    if (_exiting || !_initialized) return;
    try {
      final controller = await _ensureOverlayController();
      await _overlayReady.future.timeout(const Duration(seconds: 12));
      await _sendOverlaySettings();
      await controller.show();
      if (mounted) setState(() => _overlayVisible = true);
      await widget.log.info('Keyboard window shown.');
    } catch (error) {
      await widget.log.error('Unable to show keyboard window.', error);
    }
  }

  Future<void> _hideOverlay() async {
    final controller = _overlayController;
    if (controller != null) {
      try {
        await controller.hide();
        await widget.log.info('Keyboard window hidden.');
      } catch (error) {
        await widget.log.error('Unable to hide keyboard window.', error);
      }
    }
    if (mounted) setState(() => _overlayVisible = false);
  }

  Future<void> _toggleOverlay() async {
    if (_overlayVisible) {
      await _hideOverlay();
    } else {
      await _showOverlay();
    }
  }

  Future<void> _sendOverlaySettings() async {
    if (_overlayController == null) return;
    try {
      await windowSettingsChannel.invokeMethod(
        'applySettings',
        widget.settings.toJson(),
      );
    } catch (_) {
      // The keyboard engine can still be starting; it loads settings itself.
    }
  }

  Future<dynamic> _handleWindowCommand(MethodCall call) async {
    switch (call.method) {
      case 'overlayReady':
        if (!_overlayReady.isCompleted) _overlayReady.complete();
      case 'showMain':
        await showMain();
      case 'hideOverlay':
        await _hideOverlay();
      case 'overlayMoved':
        final args = call.arguments;
        if (args is Map) {
          final left = args['left'];
          final top = args['top'];
          if (left is num && top is num) {
            widget.settings.keyboardLeft = left.toDouble();
            widget.settings.keyboardTop = top.toDouble();
            await runtime.save();
          }
        }
    }
    return true;
  }

  Future<void> hideToTray() async {
    if (_initialized) await windowManager.hide();
  }

  Future<void> exitApplication() async {
    if (_exiting) return;
    _exiting = true;
    await runtime.save();
    await widget.log.info('Application exit requested.');
    await _hideOverlay();
    await _tray.dispose();
    try {
      await windowManager.destroy();
    } catch (_) {
      exit(0);
    }
  }

  @override
  void onWindowClose() {
    unawaited(hideToTray());
  }

  @override
  void onTrayIconMouseUp() {
    unawaited(showMain());
  }

  @override
  void onTrayMenuItemClick(MenuItem menuItem) {
    switch (menuItem.key) {
      case 'show':
        unawaited(showMain());
      case 'keyboard':
        unawaited(_toggleOverlay());
      case 'exit':
        unawaited(exitApplication());
    }
  }

  Future<void> _checkUpdatesIfEnabled() async {
    if (!widget.settings.autoCheckUpdates) return;
    await Future<void>.delayed(const Duration(seconds: 4));
    final result = await UpdateService.check();
    await widget.log.info(
      'Update check: current=${result.current}, latest=${result.latest}, success=${result.success}',
    );
  }

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: '肌忆五笔 PassiveWubi',
      debugShowCheckedModeBanner: false,
      themeMode: widget.settings.theme.themeMode,
      themeAnimationDuration: Platform.isLinux
          ? Duration.zero
          : const Duration(milliseconds: 180),
      localizationsDelegates: GlobalMaterialLocalizations.delegates,
      supportedLocales: const [Locale('zh', 'CN'), Locale('en', 'US')],
      locale: const Locale('zh', 'CN'),
      theme: _theme(false),
      darkTheme: _theme(true),
      home: MainShell(
        runtime: runtime,
        overlayVisible: _overlayVisible,
        onToggleOverlay: _toggleOverlay,
        onExit: exitApplication,
      ),
    );
  }

  ThemeData _theme(bool dark) {
    final scheme = ColorScheme.fromSeed(
      seedColor: const Color(0xFFF57C1F),
      brightness: dark ? Brightness.dark : Brightness.light,
      surface: dark ? const Color(0xFF131B29) : Colors.white,
    );
    return ThemeData(
      useMaterial3: true,
      colorScheme: scheme,
      scaffoldBackgroundColor: dark
          ? const Color(0xFF0B111B)
          : const Color(0xFFF2F4F8),
      fontFamily: 'Noto Sans CJK SC',
      cardTheme: CardThemeData(
        color: dark ? const Color(0xFF172233) : Colors.white,
        elevation: 0,
        margin: EdgeInsets.zero,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(14),
          side: BorderSide(color: scheme.outlineVariant.withValues(alpha: 0.7)),
        ),
      ),
      inputDecorationTheme: InputDecorationTheme(
        filled: true,
        fillColor: dark ? const Color(0xFF111A28) : const Color(0xFFF7F8FA),
        border: OutlineInputBorder(
          borderRadius: BorderRadius.circular(9),
          borderSide: BorderSide(color: scheme.outlineVariant),
        ),
      ),
    );
  }
}

class _TrayController {
  _TrayController(this.host);
  final _PassiveWubiAppState host;

  Future<void> initialize() async {
    try {
      await trayManager.setIcon('assets/icons/passive_wubi.png');
      await trayManager.setToolTip('肌忆五笔 PassiveWubi');
      await trayManager.setContextMenu(
        Menu(
          items: [
            MenuItem(key: 'show', label: '打开肌忆五笔'),
            MenuItem(key: 'keyboard', label: '显示/隐藏置顶键盘'),
            MenuItem.separator(),
            MenuItem(key: 'exit', label: '退出肌忆五笔'),
          ],
        ),
      );
      trayManager.addListener(host);
    } catch (error) {
      await host.widget.log.error('Tray initialization failed.', error);
    }
  }

  Future<void> dispose() async {
    try {
      trayManager.removeListener(host);
      await trayManager.destroy();
    } catch (_) {}
  }
}
