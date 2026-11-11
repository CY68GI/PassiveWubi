// ignore_for_file: deprecated_member_use

import 'dart:async';

import 'package:flutter/material.dart';
import 'package:window_manager/legacy.dart';

import '../core/keyboard_data.dart';
import '../core/runtime.dart';

class OverlayScreen extends StatefulWidget {
  const OverlayScreen({
    super.key,
    required this.runtime,
    required this.onOpenMain,
    required this.onHide,
  });

  final AppRuntime runtime;
  final Future<void> Function() onOpenMain;
  final Future<void> Function() onHide;

  @override
  State<OverlayScreen> createState() => _OverlayScreenState();
}

class _OverlayScreenState extends State<OverlayScreen> {
  Timer? _timer;
  int _highlight = -1;

  AppRuntime get runtime => widget.runtime;

  @override
  void initState() {
    super.initState();
    _restartTimer();
  }

  @override
  void didUpdateWidget(covariant OverlayScreen oldWidget) {
    super.didUpdateWidget(oldWidget);
    _restartTimer();
  }

  @override
  void dispose() {
    _timer?.cancel();
    super.dispose();
  }

  void _restartTimer() {
    _timer?.cancel();
    if (!runtime.settings.passiveHighlight) return;
    _timer = Timer.periodic(const Duration(seconds: 5), (_) {
      if (!mounted) return;
      final count = buildKeyboardKeys(runtime.editionData).length;
      setState(() => _highlight = count == 0 ? -1 : (_highlight + 1) % count);
    });
  }

  @override
  Widget build(BuildContext context) {
    final keys = buildKeyboardKeys(runtime.editionData);
    final settings = runtime.settings;
    return Material(
      color: Colors.transparent,
      child: FittedBox(
        fit: BoxFit.contain,
        child: SizedBox(
          width: 1240,
          height: 520,
          child: Container(
            margin: const EdgeInsets.all(8),
            padding: const EdgeInsets.fromLTRB(20, 10, 20, 8),
            decoration: BoxDecoration(
              color: const Color(0xF2131B2A),
              borderRadius: BorderRadius.circular(17),
              border: Border.all(color: Colors.white.withValues(alpha: .16)),
              boxShadow: const [
                BoxShadow(
                  color: Color(0x66000000),
                  blurRadius: 24,
                  offset: Offset(0, 8),
                ),
              ],
            ),
            child: Column(
              children: [
                GestureDetector(
                  behavior: HitTestBehavior.opaque,
                  onPanStart: (_) => windowManager.startDragging(),
                  child: SizedBox(
                    height: 52,
                    child: Row(
                      children: [
                        Container(
                          width: 35,
                          height: 35,
                          decoration: BoxDecoration(
                            color: const Color(0xFFE9654F),
                            borderRadius: BorderRadius.circular(10),
                          ),
                          alignment: Alignment.center,
                          child: const Text(
                            '五',
                            style: TextStyle(
                              color: Colors.white,
                              fontSize: 19,
                              fontWeight: FontWeight.w700,
                            ),
                          ),
                        ),
                        const SizedBox(width: 10),
                        const Text(
                          '肌忆五笔',
                          style: TextStyle(
                            color: Colors.white,
                            fontSize: 16,
                            fontWeight: FontWeight.w600,
                          ),
                        ),
                        const SizedBox(width: 10),
                        Container(
                          padding: const EdgeInsets.symmetric(
                            horizontal: 9,
                            vertical: 4,
                          ),
                          decoration: BoxDecoration(
                            color: Colors.white.withValues(alpha: .10),
                            borderRadius: BorderRadius.circular(10),
                          ),
                          child: Text(
                            settings.edition.label,
                            style: const TextStyle(
                              color: Color(0xFFF2B6AA),
                              fontSize: 10.5,
                              fontWeight: FontWeight.w600,
                            ),
                          ),
                        ),
                        const Spacer(),
                        Text(
                          '横 竖 撇 捺 折 · 字根表',
                          style: TextStyle(
                            color: Colors.white.withValues(alpha: .48),
                            fontSize: 11,
                          ),
                        ),
                        const Spacer(),
                        _OverlayButton(label: '设置', onTap: widget.onOpenMain),
                        const SizedBox(width: 8),
                        _OverlayButton(label: '隐藏', onTap: widget.onHide),
                      ],
                    ),
                  ),
                ),
                Expanded(
                  child: Column(
                    children: [
                      for (var row = 0; row < keyboardRows.length; row++)
                        Expanded(
                          child: Center(
                            child: Row(
                              mainAxisSize: MainAxisSize.min,
                              children: [
                                for (
                                  var col = 0;
                                  col < keyboardRows[row].length;
                                  col++
                                )
                                  _KeyTile(
                                    keyData: keys.firstWhere(
                                      (item) =>
                                          item.key == keyboardRows[row][col],
                                    ),
                                    highlighted:
                                        _highlight ==
                                        keys.indexWhere(
                                          (item) =>
                                              item.key ==
                                              keyboardRows[row][col],
                                        ),
                                    showSimpleCode: settings.showSimpleCodes,
                                    showRootTable: settings.showRootTable,
                                  ),
                              ],
                            ),
                          ),
                        ),
                    ],
                  ),
                ),
                SizedBox(
                  height: 30,
                  child: Row(
                    children: [
                      Text(
                        '拖动顶部移动 · 在设置中调整大小和透明度 · 关闭主界面不会退出',
                        style: TextStyle(
                          color: Colors.white.withValues(alpha: .38),
                          fontSize: 10.5,
                        ),
                      ),
                      const Spacer(),
                      Text(
                        settings.passiveHighlight
                            ? '潜移默化模式 · 每 5 秒轻提示'
                            : '静态字根模式',
                        style: TextStyle(
                          color: Colors.white.withValues(alpha: .42),
                          fontSize: 10,
                        ),
                      ),
                      const SizedBox(width: 8),
                      Container(
                        width: 7,
                        height: 7,
                        decoration: const BoxDecoration(
                          color: Color(0xFF6BC49A),
                          shape: BoxShape.circle,
                        ),
                      ),
                      const SizedBox(width: 5),
                      Text(
                        '置顶',
                        style: TextStyle(
                          color: Colors.white.withValues(alpha: .42),
                          fontSize: 10,
                        ),
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

class _OverlayButton extends StatelessWidget {
  const _OverlayButton({required this.label, required this.onTap});
  final String label;
  final Future<void> Function() onTap;

  @override
  Widget build(BuildContext context) => TextButton(
    onPressed: onTap,
    style: TextButton.styleFrom(
      foregroundColor: Colors.white.withValues(alpha: .78),
      backgroundColor: Colors.white.withValues(alpha: .08),
      padding: const EdgeInsets.symmetric(horizontal: 13, vertical: 7),
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(7)),
    ),
    child: Text(label, style: const TextStyle(fontSize: 12)),
  );
}

class _KeyTile extends StatelessWidget {
  const _KeyTile({
    required this.keyData,
    required this.highlighted,
    required this.showSimpleCode,
    required this.showRootTable,
  });

  final KeyboardKey keyData;
  final bool highlighted;
  final bool showSimpleCode;
  final bool showRootTable;

  @override
  Widget build(BuildContext context) {
    final accent = _keyColor(keyData.key);
    return Container(
      width: 108,
      height: 124,
      margin: const EdgeInsets.all(4),
      decoration: BoxDecoration(
        color: highlighted ? const Color(0xFF28374D) : const Color(0xFF172233),
        borderRadius: BorderRadius.circular(10),
        border: Border.all(
          color: highlighted ? accent : Colors.white.withValues(alpha: .15),
        ),
      ),
      child: Column(
        children: [
          Container(
            height: 3,
            decoration: BoxDecoration(
              color: accent,
              borderRadius: const BorderRadius.vertical(
                top: Radius.circular(10),
              ),
            ),
          ),
          Expanded(
            child: Padding(
              padding: const EdgeInsets.fromLTRB(10, 6, 10, 7),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  Row(
                    children: [
                      Text(
                        keyData.key,
                        style: TextStyle(
                          color: accent,
                          fontFamily: 'monospace',
                          fontSize: 15,
                          fontWeight: FontWeight.w700,
                        ),
                      ),
                      const Spacer(),
                      Text(
                        _zoneName(keyData.key),
                        style: TextStyle(
                          color: Colors.white.withValues(alpha: .40),
                          fontSize: 8.5,
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 5),
                  Row(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        keyData.keyName,
                        style: const TextStyle(
                          color: Colors.white,
                          fontSize: 21,
                          fontWeight: FontWeight.w600,
                        ),
                      ),
                      const Spacer(),
                      if (showSimpleCode && keyData.simpleCode.isNotEmpty)
                        Container(
                          padding: const EdgeInsets.symmetric(
                            horizontal: 5,
                            vertical: 2,
                          ),
                          decoration: BoxDecoration(
                            color: Colors.white.withValues(alpha: .09),
                            borderRadius: BorderRadius.circular(5),
                          ),
                          child: Text(
                            keyData.simpleCode,
                            style: TextStyle(
                              color: accent,
                              fontSize: 10.5,
                              fontWeight: FontWeight.w600,
                            ),
                          ),
                        ),
                    ],
                  ),
                  if (showRootTable)
                    Expanded(
                      child: Center(
                        child: Text(
                          keyData.roots,
                          textAlign: TextAlign.center,
                          maxLines: 5,
                          overflow: TextOverflow.fade,
                          style: TextStyle(
                            color: Colors.white.withValues(alpha: .72),
                            fontSize: 11.2,
                            height: 1.23,
                          ),
                        ),
                      ),
                    ),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }

  static Color _keyColor(String key) {
    if ('QWERT'.contains(key)) return const Color(0xFFEF725E);
    if ('YUIOP'.contains(key)) return const Color(0xFFE7AE54);
    if ('ASDFG'.contains(key)) return const Color(0xFF6BC49A);
    if ('HJKL'.contains(key)) return const Color(0xFF65AFE1);
    return const Color(0xFFAD8AE5);
  }

  static String _zoneName(String key) {
    if ('QWERT'.contains(key)) return '撇区';
    if ('YUIOP'.contains(key)) return '捺区';
    if ('ASDFG'.contains(key)) return '横区';
    if ('HJKL'.contains(key)) return '竖区';
    return '折区';
  }
}
