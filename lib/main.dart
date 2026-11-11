// ignore_for_file: deprecated_member_use

import 'dart:io';

import 'package:desktop_multi_window/desktop_multi_window.dart';
import 'package:flutter/material.dart';
import 'package:window_manager/window_manager.dart';

import 'app.dart';
import 'core/practice_data.dart';
import 'core/runtime.dart';
import 'core/single_instance.dart';
import 'core/storage.dart';
import 'core/update_service.dart';
import 'core/window_bridge.dart';
import 'ui/overlay_window_app.dart';

Future<void> main(List<String> args) async {
  WidgetsFlutterBinding.ensureInitialized();
  await windowManager.ensureInitialized();

  final window = await WindowController.fromCurrentEngine();
  final windowKind = windowKindFromArguments(window.arguments);
  if (windowKind == AppWindowKind.keyboard) {
    await _runKeyboardWindow();
    return;
  }

  if (!await SingleInstance.acquire()) {
    exit(0);
  }

  final storage = await AppStorage.create();
  final settings = await storage.loadSettings();
  final catalog = await PracticeCatalog.load();
  final log = AppLogger(storage);
  await log.info(
    'PassiveWubi Flutter ${UpdateService.currentVersion} main window starting. '
    'Program directory: ${storage.rootDirectory.path}.',
  );

  runApp(
    PassiveWubiApp(
      storage: storage,
      settings: settings,
      catalog: catalog,
      log: log,
    ),
  );
}

Future<void> _runKeyboardWindow() async {
  final storage = await AppStorage.create();
  final settings = await storage.loadSettings();
  final catalog = await PracticeCatalog.load();
  final log = AppLogger(storage);
  final runtime = AppRuntime(
    storage: storage,
    catalog: catalog,
    settings: settings,
    log: log,
  );
  await log.info(
    'PassiveWubi Flutter ${UpdateService.currentVersion} keyboard window starting. '
    'Program directory: ${storage.rootDirectory.path}.',
  );
  runApp(OverlayWindowApp(runtime: runtime));
}
