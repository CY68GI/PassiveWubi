import 'dart:convert';
import 'dart:io';

import 'package:path/path.dart' as p;

import 'app_settings.dart';

class AppStorage {
  AppStorage._(this.rootDirectory);

  final Directory rootDirectory;

  static Future<AppStorage> create() async {
    final base = File(Platform.resolvedExecutable).parent.path;
    try {
      final candidate = Directory(p.join(base));
      final probe = File(p.join(base, '.write-test-$pid'));
      await probe.writeAsString('ok', flush: true);
      await probe.delete();
      return AppStorage._(candidate);
    } catch (_) {
      final fallback = Platform.isWindows
          ? p.join(Platform.environment['APPDATA'] ?? base, 'PassiveWubi')
          : p.join(
              Platform.environment['XDG_STATE_HOME'] ??
                  p.join(Platform.environment['HOME'] ?? base, '.local', 'state'),
              'passivewubi',
            );
      final dir = Directory(fallback);
      await dir.create(recursive: true);
      return AppStorage._(dir);
    }
  }

  File get settingsFile => File(p.join(rootDirectory.path, 'settings.json'));

  Directory get logDirectory => Directory(p.join(rootDirectory.path, 'logs'));

  Future<AppSettings> loadSettings() async {
    try {
      if (!await settingsFile.exists()) return AppSettings();
      final json = jsonDecode(await settingsFile.readAsString()) as Map<String, dynamic>;
      return AppSettings.fromJson(json);
    } catch (_) {
      return AppSettings();
    }
  }

  Future<void> saveSettings(AppSettings settings) async {
    try {
      settings.normalize();
      await rootDirectory.create(recursive: true);
      await settingsFile.writeAsString(
        const JsonEncoder.withIndent('  ').convert(settings.toJson()),
        flush: true,
      );
    } catch (_) {}
  }
}

class AppLogger {
  AppLogger(this.storage);
  final AppStorage storage;

  String get todayFile =>
      p.join(storage.logDirectory.path, 'PassiveWubi-${_dateStamp()}.log');

  Future<void> info(String message) => _write('INFO', message);
  Future<void> error(String message, [Object? error]) =>
      _write('ERROR', error == null ? message : '$message\n$error');

  Future<void> openFolder() async {
    await storage.logDirectory.create(recursive: true);
    await _openPath(storage.logDirectory.path);
  }

  Future<void> openToday() async {
    await info('Today log opened.');
    await _openPath(todayFile);
  }

  Future<void> _write(String level, String message) async {
    try {
      await storage.logDirectory.create(recursive: true);
      await File(todayFile).writeAsString(
        '${DateTime.now().toIso8601String()} [$level] $message\n',
        mode: FileMode.append,
        flush: true,
      );
    } catch (_) {}
  }

  static String _dateStamp() =>
      '${DateTime.now().year.toString().padLeft(4, '0')}'
      '${DateTime.now().month.toString().padLeft(2, '0')}'
      '${DateTime.now().day.toString().padLeft(2, '0')}';

  static Future<void> _openPath(String path) async {
    if (Platform.isWindows) {
      await Process.start('explorer.exe', [path], runInShell: false);
    } else if (Platform.isLinux) {
      await Process.start('xdg-open', [path], runInShell: false);
    }
  }
}
