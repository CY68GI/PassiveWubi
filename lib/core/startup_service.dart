import 'dart:io';

import 'package:path/path.dart' as p;

class StartupService {
  static Future<bool> isEnabled() async {
    if (Platform.isWindows) {
      final result = await Process.run(
        'reg',
        ['query', r'HKCU\Software\Microsoft\Windows\CurrentVersion\Run', '/v', 'PassiveWubi'],
        runInShell: true,
      );
      return result.exitCode == 0 && result.stdout.toString().contains('PassiveWubi');
    }
    if (Platform.isLinux) {
      return File(_linuxAutostartFile).exists();
    }
    return false;
  }

  static Future<void> setEnabled(bool enabled) async {
    try {
      if (Platform.isWindows) {
        final exe = Platform.resolvedExecutable.replaceAll('"', '\\"');
        if (enabled) {
          await Process.run(
            'reg',
            [
              'add',
              r'HKCU\Software\Microsoft\Windows\CurrentVersion\Run',
              '/v',
              'PassiveWubi',
              '/t',
              'REG_SZ',
              '/d',
              '"$exe"',
              '/f',
            ],
            runInShell: true,
          );
        } else {
          await Process.run(
            'reg',
            [
              'delete',
              r'HKCU\Software\Microsoft\Windows\CurrentVersion\Run',
              '/v',
              'PassiveWubi',
              '/f',
            ],
            runInShell: true,
          );
        }
      } else if (Platform.isLinux) {
        final file = File(_linuxAutostartFile);
        if (!enabled) {
          if (await file.exists()) await file.delete();
          return;
        }
        await file.parent.create(recursive: true);
        await file.writeAsString(
          '[Desktop Entry]\n'
          'Type=Application\n'
          'Name=肌忆五笔\n'
          'Comment=PassiveWubi 五笔置顶学习键盘\n'
          'Exec="${Platform.resolvedExecutable}"\n'
          'Terminal=false\n'
          'X-GNOME-Autostart-enabled=true\n',
          flush: true,
        );
      }
    } catch (_) {}
  }

  static String get _linuxAutostartFile => p.join(
        Platform.environment['XDG_CONFIG_HOME'] ??
            p.join(Platform.environment['HOME'] ?? '', '.config'),
        'autostart',
        'passivewubi.desktop',
      );
}
