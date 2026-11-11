import 'dart:convert';

import 'package:flutter/material.dart';

enum WubiEdition {
  v86('86 版'),
  v98('98 版'),
  newCentury('新世纪版');

  const WubiEdition(this.label);
  final String label;
}

enum AppThemePreference {
  dark,
  light;

  ThemeMode get themeMode => this == dark ? ThemeMode.dark : ThemeMode.light;
}

class AppSettings {
  AppSettings();

  WubiEdition edition = WubiEdition.v86;
  AppThemePreference theme = AppThemePreference.dark;
  double keyboardOpacity = 0.90;
  double keyboardScale = 1.0;
  bool showSimpleCodes = true;
  bool showRootTable = true;
  bool passiveHighlight = true;
  bool showKeyboardOnStartup = true;
  bool autoCheckUpdates = true;
  bool launchAtStartup = false;
  bool hasCompletedFirstRun = false;
  double keyboardLeft = -1;
  double keyboardTop = -1;

  void normalize() {
    keyboardOpacity = keyboardOpacity.clamp(0.25, 1.0);
    keyboardScale = keyboardScale.clamp(0.60, 1.45);
  }

  Map<String, dynamic> toJson() => <String, dynamic>{
    'edition': edition.name,
    'theme': theme.name,
    'keyboardOpacity': keyboardOpacity,
    'keyboardScale': keyboardScale,
    'showSimpleCodes': showSimpleCodes,
    'showRootTable': showRootTable,
    'passiveHighlight': passiveHighlight,
    'showKeyboardOnStartup': showKeyboardOnStartup,
    'autoCheckUpdates': autoCheckUpdates,
    'launchAtStartup': launchAtStartup,
    'hasCompletedFirstRun': hasCompletedFirstRun,
    'keyboardLeft': keyboardLeft,
    'keyboardTop': keyboardTop,
  };

  factory AppSettings.fromJson(Map<String, dynamic> json) {
    final result = AppSettings();
    result.edition = _editionFrom(json['edition']);
    result.theme = _themeFrom(json['theme']);
    result.keyboardOpacity =
        (json['keyboardOpacity'] as num?)?.toDouble() ?? 0.90;
    result.keyboardScale = (json['keyboardScale'] as num?)?.toDouble() ?? 1.0;
    result.showSimpleCodes = json['showSimpleCodes'] as bool? ?? true;
    result.showRootTable = json['showRootTable'] as bool? ?? true;
    result.passiveHighlight = json['passiveHighlight'] as bool? ?? true;
    result.showKeyboardOnStartup =
        json['showKeyboardOnStartup'] as bool? ?? true;
    result.autoCheckUpdates = json['autoCheckUpdates'] as bool? ?? true;
    result.launchAtStartup = json['launchAtStartup'] as bool? ?? false;
    result.hasCompletedFirstRun =
        json['hasCompletedFirstRun'] as bool? ?? false;
    result.keyboardLeft = (json['keyboardLeft'] as num?)?.toDouble() ?? -1;
    result.keyboardTop = (json['keyboardTop'] as num?)?.toDouble() ?? -1;
    result.normalize();
    return result;
  }

  static Map<String, dynamic> decode(String source) =>
      jsonDecode(source) as Map<String, dynamic>;

  static WubiEdition _editionFrom(dynamic value) {
    final name = value?.toString();
    return WubiEdition.values.firstWhere(
      (item) => item.name == name || item.label == name,
      orElse: () => WubiEdition.v86,
    );
  }

  static AppThemePreference _themeFrom(dynamic value) {
    final name = value?.toString();
    return AppThemePreference.values.firstWhere(
      (item) => item.name == name,
      orElse: () => AppThemePreference.dark,
    );
  }
}
