// ignore_for_file: deprecated_member_use

import 'dart:io';

import 'package:flutter/material.dart';
import 'package:window_manager/legacy.dart';

import 'app.dart';
import 'core/practice_data.dart';
import 'core/single_instance.dart';
import 'core/storage.dart';

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();
  if (!await SingleInstance.acquire()) {
    exit(0);
  }

  final storage = await AppStorage.create();
  final settings = await storage.loadSettings();
  final catalog = await PracticeCatalog.load();
  final log = AppLogger(storage);
  await log.info('PassiveWubi Flutter 2.0.0 starting.');

  await windowManager.ensureInitialized();

  runApp(
    PassiveWubiApp(
      storage: storage,
      settings: settings,
      catalog: catalog,
      log: log,
    ),
  );
}
