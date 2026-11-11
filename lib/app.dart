// The 0.6 window/tray packages keep the stable 0.5 bridge while native APIs
// are still evolving; isolate that compatibility layer here.
// ignore_for_file: deprecated_member_use

import 'dart:async';
import 'dart:io';

import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:tray_manager/legacy.dart';
import 'package:window_manager/legacy.dart';

import 'core/app_settings.dart';
import 'core/practice_data.dart';
import 'core/runtime.dart';
import 'core/single_instance.dart';
import 'core/storage.dart';
import 'core/update_service.dart';
import 'ui/main_shell.dart';
import 'ui/overlay_screen.dart';

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
  bool _overlayMode = false;
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
    _activationSubscription =
        SingleInstance.activations.listen((_) => unawaited(showMain()));
    WidgetsBinding.instance.addPostFrameCallback((_) => _initializeDesktop());
  }

  @override
  void dispose() {
    runtime.removeListener(_onRuntimeChanged);
    _activationSubscription?.cancel();
    windowManager.removeListener(this);
    trayManager.removeListener(this);
    _tray.dispose();
    runtime.dispose();
    super.dispose();
  }

  Future<void> _initializeDesktop() async {
    try {
      await windowManager.setPreventClose(true);
      windowManager.addListener(this);
      await windowManager.waitUntilReadyToShow(
        const WindowOptions(
          size: Size(1120, 760),
          minimumSize: Size(900, 640),
          center: true,
          title: '肌忆五笔 PassiveWubi',
          titleBarStyle: TitleBarStyle.normal,
          backgroundColor: Color(0xFF0B111B),
        ),
        () async {
          await windowManager.show();
          await windowManager.focus();
        },
      );
      await _tray.initialize();
      await widget.log.info('Desktop window initialized.');
      _initialized = true;
      if (widget.settings.hasCompletedFirstRun &&
          widget.settings.showKeyboardOnStartup) {
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
    if (_overlayMode) {
      unawaited(_applyOverlayWindowSettings());
    }
  }

  Future<void> _applyOverlayWindowSettings() async {
    if (!_initialized) return;
    final scale = widget.settings.keyboardScale;
    await windowManager.setSize(Size(1240 * scale, 520 * scale));
    await windowManager.setOpacity(widget.settings.keyboardOpacity);
  }

  Future<void> showMain() async {
    if (_exiting) return;
    setState(() => _overlayMode = false);
    if (!_initialized) return;
    await windowManager.setTitleBarStyle(TitleBarStyle.normal);
    await windowManager.setAlwaysOnTop(false);
    await windowManager.setSkipTaskbar(false);
    await windowManager.setOpacity(1);
    await windowManager.setBackgroundColor(
      widget.settings.theme == AppThemePreference.dark
          ? const Color(0xFF0B111B)
          : const Color(0xFFF2F4F8),
    );
    await windowManager.setSize(const Size(1120, 760));
    await windowManager.center();
    await windowManager.show();
    await windowManager.focus();
  }

  Future<void> _showOverlay() async {
    if (_exiting) return;
    setState(() => _overlayMode = true);
    if (!_initialized) return;
    await windowManager.setAsFrameless();
    await windowManager.setAlwaysOnTop(true);
    await windowManager.setSkipTaskbar(true);
    await windowManager.setBackgroundColor(Colors.transparent);
    await windowManager.setOpacity(widget.settings.keyboardOpacity);
    await windowManager.setSize(
      Size(1240 * widget.settings.keyboardScale,
          520 * widget.settings.keyboardScale),
    );
    if (widget.settings.keyboardLeft >= 0 &&
        widget.settings.keyboardTop >= 0) {
      await windowManager.setPosition(Offset(
        widget.settings.keyboardLeft,
        widget.settings.keyboardTop,
      ));
    } else {
      await windowManager.setAlignment(Alignment.bottomCenter);
    }
    await windowManager.show();
  }

  Future<void> hideToTray() async {
    if (_initialized) await windowManager.hide();
  }

  Future<void> exitApplication() async {
    if (_exiting) return;
    _exiting = true;
    await runtime.save();
    await widget.log.info('Application exit requested.');
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
  void onWindowMoved() {
    unawaited(_rememberOverlayPosition());
  }

  Future<void> _rememberOverlayPosition() async {
    if (!_overlayMode || !_initialized) return;
    final position = await windowManager.getPosition();
    runtime.settings.keyboardLeft = position.dx;
    runtime.settings.keyboardTop = position.dy;
    await runtime.save();
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
        unawaited(_overlayMode ? showMain() : _showOverlay());
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
      themeAnimationDuration: const Duration(milliseconds: 220),
      localizationsDelegates: GlobalMaterialLocalizations.delegates,
      supportedLocales: const [
        Locale('zh', 'CN'),
        Locale('en', 'US'),
      ],
      locale: const Locale('zh', 'CN'),
      theme: _theme(false),
      darkTheme: _theme(true),
      builder: (context, child) => _overlayMode
          ? OverlayScreen(
              runtime: runtime,
              onOpenMain: showMain,
              onHide: hideToTray,
            )
          : (child ?? const SizedBox.shrink()),
      home: MainShell(
        runtime: runtime,
        onShowOverlay: _showOverlay,
        onExit: exitApplication,
      ),
    );
  }

  ThemeData _theme(bool dark) {
    final scheme = ColorScheme.fromSeed(
      seedColor: const Color(0xFFE9654F),
      brightness: dark ? Brightness.dark : Brightness.light,
      surface: dark ? const Color(0xFF131B29) : Colors.white,
    );
    return ThemeData(
      useMaterial3: true,
      colorScheme: scheme,
      scaffoldBackgroundColor:
          dark ? const Color(0xFF0B111B) : const Color(0xFFF2F4F8),
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
