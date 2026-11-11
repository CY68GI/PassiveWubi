import 'package:flutter_test/flutter_test.dart';
import 'package:passive_wubi/core/app_settings.dart';
import 'package:passive_wubi/core/keyboard_data.dart';
import 'package:passive_wubi/core/practice_data.dart';
import 'package:passive_wubi/core/practice_session.dart';
import 'package:passive_wubi/core/window_bridge.dart';

void main() {
  TestWidgetsFlutterBinding.ensureInitialized();

  test('settings round-trip and normalize', () {
    final settings = AppSettings()
      ..edition = WubiEdition.v98
      ..theme = AppThemePreference.light
      ..keyboardOpacity = 1.8
      ..keyboardScale = 0.2;
    settings.normalize();
    final restored = AppSettings.fromJson(settings.toJson());
    expect(restored.edition, WubiEdition.v98);
    expect(restored.theme, AppThemePreference.light);
    expect(restored.keyboardOpacity, 1.0);
    expect(restored.keyboardScale, .60);
  });

  test('catalog contains all Wubi practice categories', () async {
    final catalog = await PracticeCatalog.load();
    for (final edition in WubiEdition.values) {
      final data = catalog.forEdition(edition.name);
      expect(data.keyNames, hasLength(25), reason: edition.name);
      expect(data.mnemonics, hasLength(25), reason: edition.name);
      expect(data.formingRoots.length, greaterThan(20), reason: edition.name);
      expect(data.common.length, greaterThanOrEqualTo(200), reason: edition.name);
      expect(data.level1, hasLength(25), reason: edition.name);
      expect(data.level2.length, greaterThan(50), reason: edition.name);
      expect(data.level3.length, greaterThan(50), reason: edition.name);
      expect(data.identification.length, greaterThanOrEqualTo(20), reason: edition.name);
      expect(data.difficult.length, greaterThanOrEqualTo(20), reason: edition.name);
      expect(data.words2.length, greaterThanOrEqualTo(40), reason: edition.name);
      expect(data.words3.length, greaterThanOrEqualTo(40), reason: edition.name);
      expect(data.words4.length, greaterThanOrEqualTo(40), reason: edition.name);
      for (final item in data.characterItems(CharacterExercise.formingRoot)) {
        expect(item.code, isNotEmpty);
      }
    }
  });

  test('keyboard data follows physical QWERTY rows', () async {
    final catalog = await PracticeCatalog.load();
    final keys = buildKeyboardKeys(catalog.forEdition(WubiEdition.v86.name));
    expect(keys, hasLength(26));
    expect(keys.first.key, 'Q');
    expect(keys[10].key, 'A');
    expect(keys.last.key, 'M');
  });

  test('practice answer accepts case and rejects incorrect codes', () {
    const item = PracticeItem(prompt: '的', code: 'rqyy');
    final session = PracticeSession([item]);
    expect(session.submit('RQYY').correct, isTrue);
    expect(session.submit('abcd').correct, isFalse);
    expect(PracticeSession.keyDescription('rqyy'), 'R 白 · Q 金 · Y 言 · Y 言');
  });

  test('speed calculator reports accuracy and cpm', () {
    final result = calculateSpeed(
      '肌忆五笔',
      '肌忆五笔',
      const Duration(minutes: 1),
    );
    expect(result.correct, 4);
    expect(result.accuracy, 100);
    expect(result.cpm, 4);
  });

  test('desktop window role follows launch arguments', () {
    expect(windowKindFromArguments(''), AppWindowKind.main);
    expect(
      windowKindFromArguments(keyboardWindowArguments),
      AppWindowKind.keyboard,
    );
    expect(windowKindFromArguments('invalid-json'), AppWindowKind.main);
  });
}
