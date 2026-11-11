import 'practice_data.dart';

const keyNames = <String, String>{
  'Q': '金',
  'W': '人',
  'E': '月',
  'R': '白',
  'T': '禾',
  'Y': '言',
  'U': '立',
  'I': '水',
  'O': '火',
  'P': '之',
  'A': '工',
  'S': '木',
  'D': '大',
  'F': '土',
  'G': '王',
  'H': '目',
  'J': '日',
  'K': '口',
  'L': '田',
  'Z': '学习键',
  'X': '纟',
  'C': '又',
  'V': '女',
  'B': '子',
  'N': '已',
  'M': '山',
};

const simpleCodes = <String, String>{
  'Q': '我',
  'W': '人',
  'E': '有',
  'R': '的',
  'T': '和',
  'Y': '主',
  'U': '产',
  'I': '不',
  'O': '为',
  'P': '这',
  'A': '工',
  'S': '要',
  'D': '在',
  'F': '地',
  'G': '一',
  'H': '上',
  'J': '是',
  'K': '中',
  'L': '国',
  'Z': '',
  'X': '经',
  'C': '以',
  'V': '发',
  'B': '了',
  'N': '民',
  'M': '同',
};

const keyboardRows = <List<String>>[
  ['Q', 'W', 'E', 'R', 'T', 'Y', 'U', 'I', 'O', 'P'],
  ['A', 'S', 'D', 'F', 'G', 'H', 'J', 'K', 'L'],
  ['Z', 'X', 'C', 'V', 'B', 'N', 'M'],
];

class KeyboardKey {
  const KeyboardKey({
    required this.key,
    required this.keyName,
    required this.simpleCode,
    required this.roots,
  });

  final String key;
  final String keyName;
  final String simpleCode;
  final String roots;
}

List<KeyboardKey> buildKeyboardKeys(EditionData edition) {
  final rootsByKey = <String, List<String>>{};
  for (final root in edition.roots) {
    final key = root.code.toUpperCase();
    if (key.isEmpty) continue;
    rootsByKey.putIfAbsent(key, () => []).add(root.prompt);
  }
  return [
    for (final row in keyboardRows)
      for (final key in row)
        KeyboardKey(
          key: key,
          keyName: keyNames[key] ?? key,
          simpleCode: simpleCodes[key] ?? '',
          roots: rootsByKey[key]?.join(' ') ?? keyNames[key] ?? '',
        ),
  ];
}
