import 'package:flutter/foundation.dart';

import 'app_settings.dart';
import 'practice_data.dart';
import 'storage.dart';

class AppRuntime extends ChangeNotifier {
  AppRuntime({
    required this.storage,
    required this.catalog,
    required this.settings,
    required this.log,
  });

  final AppStorage storage;
  final PracticeCatalog catalog;
  final AppLogger log;
  AppSettings settings;

  EditionData get editionData => catalog.forEdition(settings.edition.name);

  Future<void> save() => storage.saveSettings(settings);

  Future<void> update(void Function(AppSettings value) mutate) async {
    mutate(settings);
    settings.normalize();
    await save();
    notifyListeners();
  }

  Future<void> setEdition(WubiEdition edition) async {
    settings.edition = edition;
    await save();
    notifyListeners();
  }

  Future<void> toggleTheme() async {
    settings.theme = settings.theme == AppThemePreference.dark
        ? AppThemePreference.light
        : AppThemePreference.dark;
    await save();
    notifyListeners();
  }
}
