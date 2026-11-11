// ignore_for_file: deprecated_member_use

import 'dart:async';

import 'package:flutter/material.dart';

import '../core/practice_data.dart';
import '../core/practice_session.dart';
import '../core/runtime.dart';

enum PracticeMode { speed, key }

enum PracticeKind {
  mnemonic('五笔口诀练习'),
  root('五笔字根练习'),
  character('五笔拆字练习'),
  phrase('五笔词组练习');

  const PracticeKind(this.label);
  final String label;
}

class PracticeScreen extends StatefulWidget {
  const PracticeScreen({super.key, required this.runtime});
  final AppRuntime runtime;

  @override
  State<PracticeScreen> createState() => _PracticeScreenState();
}

class _PracticeScreenState extends State<PracticeScreen> {
  PracticeMode _mode = PracticeMode.speed;
  PracticeKind _kind = PracticeKind.character;
  CharacterExercise _characterExercise = CharacterExercise.common;
  PhraseExercise _phraseExercise = PhraseExercise.two;
  final _speedInput = TextEditingController();
  final _minutesInput = TextEditingController(text: '3');
  final _answerInput = TextEditingController();
  Timer? _speedTimer;
  Timer? _highlightTimer;
  String _speedTarget = '';
  DateTime? _speedStarted;
  Duration _remaining = const Duration(minutes: 3);
  bool _speedRunning = false;
  String _speedResult = '';
  PracticeSession? _session;
  bool _answered = false;
  String _feedback = '选择练习类型后开始答题。';
  bool _feedbackCorrect = true;
  int _highlight = -1;

  AppRuntime get runtime => widget.runtime;

  @override
  void initState() {
    super.initState();
    runtime.addListener(_onRuntime);
    _speedInput.addListener(_onSpeedInput);
    _startPractice();
  }

  @override
  void dispose() {
    runtime.removeListener(_onRuntime);
    _speedTimer?.cancel();
    _highlightTimer?.cancel();
    _speedInput.dispose();
    _minutesInput.dispose();
    _answerInput.dispose();
    _customTextController.dispose();
    super.dispose();
  }

  void _onRuntime() {
    if (mounted) {
      setState(_startPractice);
    }
  }

  void _startPractice() {
    _highlightTimer?.cancel();
    final edition = runtime.editionData;
    final items = switch (_kind) {
      PracticeKind.mnemonic => edition.mnemonics.map(
        (item) => PracticeItem(
          prompt: item.text,
          code: item.key.toLowerCase(),
          category: '口诀 · ${item.key} 键',
        ),
      ),
      PracticeKind.root => edition.roots,
      PracticeKind.character => edition.characterItems(_characterExercise),
      PracticeKind.phrase => edition.phraseItems(_phraseExercise),
    };
    _session = PracticeSession(
      items,
      shuffle:
          _kind == PracticeKind.phrase &&
          _phraseExercise == PhraseExercise.mixedShuffled,
    );
    _highlight = -1;
    _highlightTimer = Timer.periodic(const Duration(seconds: 5), (_) {
      if (!mounted || runtime.settings.passiveHighlight == false) return;
      setState(() {
        _highlight = _session == null || _session!.items.isEmpty
            ? -1
            : (_highlight + 1) % _session!.items.length;
      });
    });
    _answered = false;
    _feedback = '输入编码后按 Enter 或点击“提交”。';
    _feedbackCorrect = true;
    _answerInput.clear();
  }

  @override
  Widget build(BuildContext context) {
    return SingleChildScrollView(
      padding: const EdgeInsets.fromLTRB(32, 6, 32, 30),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          SegmentedButton<PracticeMode>(
            segments: const [
              ButtonSegment(
                value: PracticeMode.speed,
                icon: Icon(Icons.timer_outlined),
                label: Text('速度测试'),
              ),
              ButtonSegment(
                value: PracticeMode.key,
                icon: Icon(Icons.keyboard_outlined),
                label: Text('键位练习'),
              ),
            ],
            selected: {_mode},
            onSelectionChanged: (selection) =>
                setState(() => _mode = selection.first),
          ),
          const SizedBox(height: 18),
          if (_mode == PracticeMode.speed)
            _speedPage(context)
          else
            _keyPage(context),
        ],
      ),
    );
  }

  Widget _speedPage(BuildContext context) {
    final theme = Theme.of(context);
    return Column(
      children: [
        Card(
          child: Padding(
            padding: const EdgeInsets.all(20),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  '速度测试',
                  style: theme.textTheme.titleLarge?.copyWith(
                    fontWeight: FontWeight.w700,
                  ),
                ),
                const SizedBox(height: 5),
                Text(
                  '选择内置文章或自定义文字，设置 1–50 分钟后开始。',
                  style: theme.textTheme.bodySmall?.copyWith(
                    color: theme.colorScheme.onSurface.withValues(alpha: .58),
                  ),
                ),
                const SizedBox(height: 18),
                Row(
                  children: [
                    const SizedBox(width: 72, child: Text('测试文字')),
                    Expanded(
                      child: DropdownButtonFormField<int>(
                        value: _speedSelection,
                        isExpanded: true,
                        decoration: const InputDecoration(
                          labelText: '文章或自定义文本',
                          isDense: true,
                        ),
                        items: [
                          for (var i = 0; i < speedTestTexts.length; i++)
                            DropdownMenuItem(
                              value: i,
                              child: Text(speedTestTexts[i].title),
                            ),
                          const DropdownMenuItem(
                            value: -1,
                            child: Text('自定义文本'),
                          ),
                        ],
                        onChanged: (value) {
                          if (!_speedRunning) {
                            setState(() => _speedSelection = value ?? 0);
                          }
                        },
                      ),
                    ),
                    const SizedBox(width: 20),
                    SizedBox(
                      width: 76,
                      child: TextField(
                        controller: _minutesInput,
                        keyboardType: TextInputType.number,
                        enabled: !_speedRunning,
                        decoration: const InputDecoration(
                          labelText: '分钟',
                          isDense: true,
                        ),
                      ),
                    ),
                    const SizedBox(width: 12),
                    FilledButton(
                      onPressed: _speedRunning ? _finishSpeed : _startSpeed,
                      child: Text(_speedRunning ? '结束' : '开始测试'),
                    ),
                  ],
                ),
                if (_speedSelection == -1) ...[
                  const SizedBox(height: 12),
                  TextField(
                    controller: _customTextController,
                    minLines: 3,
                    maxLines: 5,
                    enabled: !_speedRunning,
                    decoration: const InputDecoration(
                      labelText: '自定义测试文字',
                      alignLabelWithHint: true,
                    ),
                  ),
                ],
              ],
            ),
          ),
        ),
        const SizedBox(height: 16),
        Card(
          child: Padding(
            padding: const EdgeInsets.all(20),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  children: [
                    Text(
                      '测试文本',
                      style: theme.textTheme.titleMedium?.copyWith(
                        fontWeight: FontWeight.w700,
                      ),
                    ),
                    const Spacer(),
                    Text(
                      _formatTime(_remaining),
                      style: TextStyle(
                        color: theme.colorScheme.primary,
                        fontSize: 20,
                        fontWeight: FontWeight.w700,
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 12),
                Container(
                  width: double.infinity,
                  constraints: const BoxConstraints(minHeight: 90),
                  padding: const EdgeInsets.all(16),
                  decoration: BoxDecoration(
                    color: theme.colorScheme.surfaceContainerHighest.withValues(
                      alpha: .45,
                    ),
                    borderRadius: BorderRadius.circular(10),
                  ),
                  child: Text(
                    _speedTarget.isEmpty ? '点击“开始测试”后显示测试文本。' : _speedTarget,
                    style: const TextStyle(fontSize: 17, height: 1.7),
                  ),
                ),
                const SizedBox(height: 12),
                TextField(
                  controller: _speedInput,
                  enabled: _speedRunning,
                  minLines: 5,
                  maxLines: 8,
                  decoration: const InputDecoration(
                    hintText: '开始后在这里输入文字',
                    alignLabelWithHint: true,
                  ),
                ),
                const SizedBox(height: 12),
                Text(
                  _speedResult.isEmpty
                      ? '速度 0 字/分钟   正确率 0%   已输入 0 字'
                      : _speedResult,
                  style: theme.textTheme.bodyMedium,
                ),
                const SizedBox(height: 12),
                OutlinedButton(
                  onPressed: _resetSpeed,
                  child: const Text('重新开始'),
                ),
              ],
            ),
          ),
        ),
      ],
    );
  }

  Widget _keyPage(BuildContext context) {
    final theme = Theme.of(context);
    final current = _session?.current;
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Row(
              children: [
                Expanded(
                  child: Text(
                    '${_kind.label}  ${_session?.position ?? 0} / ${_session?.total ?? 0}',
                    style: theme.textTheme.titleMedium?.copyWith(
                      fontWeight: FontWeight.w700,
                    ),
                  ),
                ),
                Text(
                  '正确率 ${_session?.accuracy.toStringAsFixed(0) ?? '0'}%',
                  style: theme.textTheme.bodySmall?.copyWith(
                    color: theme.colorScheme.onSurface.withValues(alpha: .62),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 14),
            Wrap(
              spacing: 12,
              runSpacing: 10,
              children: [
                SizedBox(
                  width: 180,
                  child: DropdownButtonFormField<PracticeKind>(
                    value: _kind,
                    decoration: const InputDecoration(labelText: '练习类型'),
                    items: [
                      for (final kind in PracticeKind.values)
                        DropdownMenuItem(value: kind, child: Text(kind.label)),
                    ],
                    onChanged: (value) {
                      if (value != null) {
                        setState(() {
                          _kind = value;
                          _startPractice();
                        });
                      }
                    },
                  ),
                ),
                if (_kind == PracticeKind.character)
                  SizedBox(
                    width: 180,
                    child: DropdownButtonFormField<CharacterExercise>(
                      value: _characterExercise,
                      decoration: const InputDecoration(labelText: '拆字分类'),
                      items: [
                        for (final item in CharacterExercise.values)
                          DropdownMenuItem(
                            value: item,
                            child: Text(item.label),
                          ),
                      ],
                      onChanged: (value) {
                        if (value != null) {
                          setState(() {
                            _characterExercise = value;
                            _startPractice();
                          });
                        }
                      },
                    ),
                  ),
                if (_kind == PracticeKind.phrase)
                  SizedBox(
                    width: 180,
                    child: DropdownButtonFormField<PhraseExercise>(
                      value: _phraseExercise,
                      decoration: const InputDecoration(labelText: '词组分类'),
                      items: [
                        for (final item in PhraseExercise.values)
                          DropdownMenuItem(
                            value: item,
                            child: Text(item.label),
                          ),
                      ],
                      onChanged: (value) {
                        if (value != null) {
                          setState(() {
                            _phraseExercise = value;
                            _startPractice();
                          });
                        }
                      },
                    ),
                  ),
                Align(
                  alignment: Alignment.centerRight,
                  child: Text(
                    '题库：${runtime.settings.edition.label}',
                    style: theme.textTheme.bodySmall?.copyWith(
                      color: theme.colorScheme.onSurface.withValues(alpha: .58),
                    ),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 18),
            Container(
              padding: const EdgeInsets.symmetric(vertical: 28, horizontal: 20),
              decoration: BoxDecoration(
                color: theme.colorScheme.primary.withValues(alpha: .06),
                borderRadius: BorderRadius.circular(12),
              ),
              child: Column(
                children: [
                  Text(
                    '题目',
                    style: theme.textTheme.labelSmall?.copyWith(
                      color: theme.colorScheme.onSurface.withValues(alpha: .52),
                    ),
                  ),
                  const SizedBox(height: 10),
                  Text(
                    current?.prompt ?? '当前分类暂无题目',
                    textAlign: TextAlign.center,
                    style: const TextStyle(
                      fontSize: 30,
                      fontWeight: FontWeight.w700,
                      height: 1.45,
                    ),
                  ),
                  const SizedBox(height: 8),
                  Text(
                    _hintText(),
                    style: theme.textTheme.bodySmall?.copyWith(
                      color: theme.colorScheme.onSurface.withValues(alpha: .58),
                    ),
                  ),
                ],
              ),
            ),
            const SizedBox(height: 16),
            Row(
              children: [
                Expanded(
                  child: TextField(
                    controller: _answerInput,
                    autofocus: true,
                    enabled: !_answered,
                    textInputAction: TextInputAction.done,
                    onSubmitted: (_) => _submitAnswer(),
                    decoration: const InputDecoration(
                      hintText: '输入五笔编码，按 Enter 提交',
                    ),
                  ),
                ),
                const SizedBox(width: 10),
                FilledButton(
                  onPressed: _answered ? null : _submitAnswer,
                  child: const Text('提交'),
                ),
                const SizedBox(width: 8),
                OutlinedButton(
                  onPressed: _nextQuestion,
                  child: const Text('下一题'),
                ),
              ],
            ),
            const SizedBox(height: 14),
            Container(
              width: double.infinity,
              padding: const EdgeInsets.all(14),
              decoration: BoxDecoration(
                color: theme.colorScheme.surfaceContainerHighest.withValues(
                  alpha: .32,
                ),
                borderRadius: BorderRadius.circular(10),
              ),
              child: Text(
                _feedback,
                style: TextStyle(
                  color: _feedbackCorrect
                      ? theme.colorScheme.onSurface
                      : theme.colorScheme.error,
                  height: 1.5,
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  int _speedSelection = 0;
  final _customTextController = TextEditingController();

  void _onSpeedInput() {
    if (!_speedRunning) return;
    final elapsed = DateTime.now().difference(_speedStarted!);
    final result = calculateSpeed(_speedTarget, _speedInput.text, elapsed);
    setState(() {
      _speedResult =
          '速度 ${result.cpm.toStringAsFixed(0)} 字/分钟   正确率 ${result.accuracy.toStringAsFixed(1)}%   已输入 ${result.typed} 字';
    });
    if (_speedInput.text.length >= _speedTarget.length) {
      _finishSpeed();
    }
  }

  void _startSpeed() {
    final custom = _speedSelection == -1;
    final target = custom
        ? _customTextController.text.trim()
        : speedTestTexts[_speedSelection].text;
    if (target.isEmpty) {
      setState(() => _speedResult = '请选择文章或输入自定义文字。');
      return;
    }
    final minutes = int.tryParse(_minutesInput.text)?.clamp(1, 50) ?? 3;
    _speedTarget = target;
    _speedInput.clear();
    _remaining = Duration(minutes: minutes);
    _speedStarted = DateTime.now();
    _speedRunning = true;
    _speedResult = '速度 0 字/分钟   正确率 0%   已输入 0 字';
    _speedTimer?.cancel();
    _speedTimer = Timer.periodic(const Duration(milliseconds: 250), (_) {
      if (!_speedRunning || _speedStarted == null) return;
      final elapsed = DateTime.now().difference(_speedStarted!);
      final remaining = Duration(minutes: minutes) - elapsed;
      if (remaining <= Duration.zero) {
        _finishSpeed();
      } else {
        setState(() => _remaining = remaining);
      }
    });
    setState(() {});
  }

  void _finishSpeed() {
    if (!_speedRunning) return;
    _speedRunning = false;
    _speedTimer?.cancel();
    final elapsed = DateTime.now().difference(_speedStarted ?? DateTime.now());
    final result = calculateSpeed(_speedTarget, _speedInput.text, elapsed);
    setState(() {
      _remaining = Duration.zero;
      _speedResult =
          '测试完成：速度 ${result.cpm.toStringAsFixed(0)} 字/分钟，正确率 ${result.accuracy.toStringAsFixed(1)}%，已输入 ${result.typed} 字';
    });
  }

  void _resetSpeed() {
    _speedRunning = false;
    _speedTimer?.cancel();
    _speedInput.clear();
    _speedTarget = '';
    _speedResult = '';
    _remaining = Duration(minutes: int.tryParse(_minutesInput.text) ?? 3);
    setState(() {});
  }

  String _formatTime(Duration duration) {
    final minutes = duration.inMinutes.clamp(0, 99);
    final seconds = duration.inSeconds.remainder(60).clamp(0, 59);
    return '${minutes.toString().padLeft(2, '0')}:${seconds.toString().padLeft(2, '0')}';
  }

  String _hintText() => switch (_kind) {
    PracticeKind.mnemonic => '判断题：这句口诀属于哪个键？输入键位字母。',
    PracticeKind.root => '判断题：这个字根位于哪个键？输入键位字母。',
    PracticeKind.phrase => '输入该词组的五笔编码。',
    PracticeKind.character => '输入该字的五笔编码。',
  };

  void _submitAnswer() {
    final answer = _answerInput.text.trim();
    if (_session == null || answer.isEmpty || _answered) return;
    final result = _session!.submit(answer);
    setState(() {
      _answered = true;
      _feedbackCorrect = result.correct;
      _feedback = result.correct
          ? '回答正确：${result.answer}\n编码对应键位：${PracticeSession.keyDescription(result.answer)}'
          : '回答错误。你输入的是“$answer”，正确编码是“${result.answer}”\n编码对应键位：${PracticeSession.keyDescription(result.answer)}';
    });
  }

  void _nextQuestion() {
    if (_session == null) return;
    _session!.next();
    _answered = false;
    _answerInput.clear();
    setState(() {
      _feedbackCorrect = true;
      _feedback = '输入编码后按 Enter 或点击“提交”。';
    });
  }
}
