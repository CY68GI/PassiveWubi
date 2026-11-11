import 'dart:convert';
import 'dart:math';

import 'package:flutter/services.dart';

enum CharacterExercise {
  formingRoot('成字字根'),
  keyName('键名字'),
  common('常用字'),
  level1('一级简码'),
  level2('二级简码'),
  level3('三级简码'),
  identification('识别码'),
  difficult('难拆字');

  const CharacterExercise(this.label);
  final String label;
}

enum PhraseExercise {
  two('二字词组'),
  three('三字词组'),
  four('四字词组'),
  mixed('混合词组'),
  mixedShuffled('混合乱序');

  const PhraseExercise(this.label);
  final String label;
}

class PracticeItem {
  const PracticeItem({
    required this.prompt,
    required this.code,
    this.category = '',
  });

  final String prompt;
  final String code;
  final String category;
}

class MnemonicItem {
  const MnemonicItem({required this.key, required this.text});
  final String key;
  final String text;
}

class EditionData {
  final List<PracticeItem> keyNames;
  final List<PracticeItem> roots;
  final List<PracticeItem> formingRoots;
  final List<PracticeItem> common;
  final List<PracticeItem> level1;
  final List<PracticeItem> level2;
  final List<PracticeItem> level3;
  final List<PracticeItem> identification;
  final List<PracticeItem> difficult;
  final List<MnemonicItem> mnemonics;
  final List<PracticeItem> words2;
  final List<PracticeItem> words3;
  final List<PracticeItem> words4;

  const EditionData({
    required this.keyNames,
    required this.roots,
    required this.formingRoots,
    required this.common,
    required this.level1,
    required this.level2,
    required this.level3,
    required this.identification,
    required this.difficult,
    required this.mnemonics,
    required this.words2,
    required this.words3,
    required this.words4,
  });

  List<PracticeItem> characterItems(CharacterExercise exercise) => switch (exercise) {
        CharacterExercise.formingRoot => formingRoots,
        CharacterExercise.keyName => keyNames,
        CharacterExercise.common => common,
        CharacterExercise.level1 => level1,
        CharacterExercise.level2 => level2,
        CharacterExercise.level3 => level3,
        CharacterExercise.identification => identification,
        CharacterExercise.difficult => difficult,
      };

  List<PracticeItem> phraseItems(PhraseExercise exercise) => switch (exercise) {
        PhraseExercise.two => words2,
        PhraseExercise.three => words3,
        PhraseExercise.four => words4,
        PhraseExercise.mixed => [...words2, ...words3, ...words4],
        PhraseExercise.mixedShuffled => [...words2, ...words3, ...words4]
          ..shuffle(Random()),
      };
}

class PracticeCatalog {
  PracticeCatalog._(this.editions);

  final Map<String, EditionData> editions;

  EditionData forEdition(String edition) {
    final key = switch (edition) {
      'v86' || 'V86' => 'V86',
      'v98' || 'V98' => 'V98',
      'newCentury' || 'NewCentury' => 'NewCentury',
      _ => edition,
    };
    return editions[key] ?? editions['V86']!;
  }

  static Future<PracticeCatalog> load() async {
    final source = await rootBundle.loadString('assets/data/practice-data.json');
    final json = jsonDecode(source) as Map<String, dynamic>;
    final editions = <String, EditionData>{};
    for (final entry in json.entries) {
      final value = entry.value as Map<String, dynamic>;
      editions[entry.key] = EditionData(
        keyNames: _items(value['keyNames']),
        roots: _items(value['rootChars']),
        formingRoots: _items(value['formingRoots']),
        common: _items(value['commonChars']),
        level1: _items(value['level1']),
        level2: _items(value['level2']),
        level3: _items(value['level3']),
        identification: _items(value['identification']),
        difficult: _items(value['difficult']),
        mnemonics: _mnemonics(value['mnemonics']),
        words2: _items(value['words2']),
        words3: _items(value['words3']),
        words4: _items(value['words4']),
      );
    }
    return PracticeCatalog._(editions);
  }

  static List<PracticeItem> _items(dynamic value) =>
      (value as List<dynamic>? ?? const [])
          .map((item) => PracticeItem(
                prompt: (item as Map<String, dynamic>)['prompt']?.toString() ?? '',
                code: item['code']?.toString() ?? '',
                category: item['category']?.toString() ?? '',
              ))
          .where((item) => item.prompt.isNotEmpty && item.code.isNotEmpty)
          .toList();

  static List<MnemonicItem> _mnemonics(dynamic value) =>
      (value as List<dynamic>? ?? const [])
          .map((item) => MnemonicItem(
                key: (item as Map<String, dynamic>)['key']?.toString() ?? '',
                text: item['text']?.toString() ?? '',
              ))
          .where((item) => item.key.isNotEmpty && item.text.isNotEmpty)
          .toList();
}

class SpeedTestText {
  const SpeedTestText(this.title, this.text);
  final String title;
  final String text;
}

const speedTestTexts = <SpeedTestText>[
  SpeedTestText(
    '日常办公',
    '清晨打开电脑，先整理今天要完成的事情。把重要的工作写在前面，再逐项安排时间。遇到问题时不急着反复尝试，先确认目标和路径，再记录结果。稳定的节奏比一时的速度更重要，认真检查也能减少返工。',
  ),
  SpeedTestText(
    '五笔学习',
    '学习五笔需要熟悉字根在键盘上的位置，也要理解汉字的拆分顺序。先从键名字和基本字根开始，再练习一级简码、二级简码和三级简码。每天用一点时间反复输入常用字，逐渐把看见字形到按下按键的过程变成自然的反应。',
  ),
  SpeedTestText(
    '山水短文',
    '沿着山路慢慢向前，树木在风里轻轻摇动。溪水从石头旁边流过，远处的云层映着柔和的光。走过一段坡地，视野忽然开阔，田野和村庄安静地铺在眼前。停下来听一会风声，再继续赶路，脚步反而更加轻松。',
  ),
  SpeedTestText(
    '科技短文',
    '软件设计既要解决眼前的问题，也要考虑以后维护和扩展的成本。清晰的数据结构可以减少重复代码，稳定的接口能让不同平台保持一致。测试不是为了证明程序没有错误，而是尽早发现风险，让修改和发布都更有把握。',
  ),
];

class SpeedResult {
  const SpeedResult(this.correct, this.typed, this.accuracy, this.cpm);
  final int correct;
  final int typed;
  final double accuracy;
  final double cpm;
}

SpeedResult calculateSpeed(String target, String typed, Duration elapsed) {
  final length = min(target.length, typed.length);
  var correct = 0;
  for (var i = 0; i < length; i++) {
    if (target[i] == typed[i]) correct++;
  }
  final minutes = max(elapsed.inMilliseconds / 60000, 1 / 60);
  return SpeedResult(
    correct,
    typed.length,
    typed.isEmpty ? 0 : correct * 100 / typed.length,
    correct / minutes,
  );
}

String normalizeWubi(String value) => value.trim().toLowerCase();
