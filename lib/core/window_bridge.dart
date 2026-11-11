import 'dart:convert';

import 'package:desktop_multi_window/desktop_multi_window.dart';

enum AppWindowKind { main, keyboard }

const keyboardWindowArguments = '{"kind":"keyboard"}';

const windowSettingsChannel = WindowMethodChannel(
  'passivewubi/window-settings',
  mode: ChannelMode.unidirectional,
);

const windowCommandsChannel = WindowMethodChannel(
  'passivewubi/window-commands',
  mode: ChannelMode.unidirectional,
);

AppWindowKind windowKindFromArguments(String arguments) {
  if (arguments.isEmpty) return AppWindowKind.main;
  try {
    final json = jsonDecode(arguments);
    if (json is Map<String, dynamic> && json['kind'] == 'keyboard') {
      return AppWindowKind.keyboard;
    }
  } catch (_) {}
  return AppWindowKind.main;
}
