import 'dart:math';

import 'practice_data.dart';

class PracticeFeedback {
  const PracticeFeedback(this.correct, this.answer);
  final bool correct;
  final String answer;
}

class PracticeSession {
  PracticeSession(Iterable<PracticeItem> items, {bool shuffle = false})
    : items = items
          .where((item) => item.prompt.isNotEmpty && item.code.isNotEmpty)
          .toList() {
    if (shuffle) this.items.shuffle(Random());
  }

  final List<PracticeItem> items;
  int index = 0;
  int correct = 0;
  int attempts = 0;
  DateTime started = DateTime.now();

  PracticeItem? get current =>
      items.isEmpty ? null : items[index.clamp(0, items.length - 1)];
  int get position => items.isEmpty ? 0 : index.clamp(0, items.length - 1) + 1;
  int get total => items.length;
  double get accuracy => attempts == 0 ? 0 : correct * 100 / attempts;

  PracticeFeedback submit(String answer) {
    attempts++;
    final expected = normalizeWubi(current?.code ?? '');
    final isCorrect = normalizeWubi(answer) == expected;
    if (isCorrect) correct++;
    return PracticeFeedback(isCorrect, current?.code ?? '');
  }

  bool next() {
    if (items.isEmpty) return false;
    index++;
    if (index < items.length) return true;
    index = 0;
    items.shuffle(Random());
    return false;
  }

  void restart() {
    index = 0;
    correct = 0;
    attempts = 0;
    started = DateTime.now();
    items.shuffle(Random());
  }

  static String keyDescription(String code) {
    final labels = <String, String>{
      'g': '王',
      'f': '土',
      'd': '大',
      's': '木',
      'a': '工',
      'h': '目',
      'j': '日',
      'k': '口',
      'l': '田',
      'm': '山',
      't': '禾',
      'r': '白',
      'e': '月',
      'w': '人',
      'q': '金',
      'y': '言',
      'u': '立',
      'i': '水',
      'o': '火',
      'p': '之',
      'n': '已',
      'b': '子',
      'v': '女',
      'c': '又',
      'x': '纟',
      'z': '学习键',
    };
    return code
        .toLowerCase()
        .split('')
        .map((key) => '${key.toUpperCase()} ${labels[key] ?? key}')
        .join(' · ');
  }
}
