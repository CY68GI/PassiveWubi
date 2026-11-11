import 'dart:async';
import 'dart:io';

import 'package:flutter/material.dart';
import 'package:url_launcher/url_launcher.dart';

import '../core/app_settings.dart';
import '../core/runtime.dart';
import '../core/startup_service.dart';
import '../core/update_service.dart';
import 'practice_screen.dart';

enum MainSection { settings, practice, about, changelog }

class MainShell extends StatefulWidget {
  const MainShell({
    super.key,
    required this.runtime,
    required this.overlayVisible,
    required this.onToggleOverlay,
    required this.onExit,
  });

  final AppRuntime runtime;
  final bool overlayVisible;
  final Future<void> Function() onToggleOverlay;
  final Future<void> Function() onExit;

  @override
  State<MainShell> createState() => _MainShellState();
}

class _MainShellState extends State<MainShell> {
  MainSection _section = MainSection.settings;
  bool _startupEnabled = false;
  bool _checkingUpdate = false;
  String _updateStatus = '当前版本 v${UpdateService.currentVersion}';

  AppRuntime get runtime => widget.runtime;

  @override
  void initState() {
    super.initState();
    unawaited(_loadStartupState());
  }

  Future<void> _loadStartupState() async {
    final enabled = await StartupService.isEnabled();
    if (mounted) setState(() => _startupEnabled = enabled);
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final isDark = theme.brightness == Brightness.dark;
    return Scaffold(
      body: Row(
        children: [
          Container(
            width: 226,
            color: isDark ? const Color(0xFF101722) : Colors.white,
            child: Column(
              children: [
                Padding(
                  padding: const EdgeInsets.fromLTRB(24, 28, 20, 34),
                  child: Row(
                    children: [
                      _LogoMark(size: 46),
                      const SizedBox(width: 12),
                      Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            '肌忆五笔',
                            style: theme.textTheme.titleLarge?.copyWith(
                              fontWeight: FontWeight.w700,
                            ),
                          ),
                          Text(
                            'PassiveWubi',
                            style: TextStyle(
                              color: theme.colorScheme.onSurface.withValues(
                                alpha: .52,
                              ),
                              fontSize: 12,
                            ),
                          ),
                        ],
                      ),
                    ],
                  ),
                ),
                _NavItem(
                  icon: Icons.keyboard_alt_outlined,
                  label: '键盘与学习',
                  selected: _section == MainSection.settings,
                  onTap: () => setState(() => _section = MainSection.settings),
                ),
                _NavItem(
                  icon: Icons.school_outlined,
                  label: '打字练习',
                  selected: _section == MainSection.practice,
                  onTap: () => setState(() => _section = MainSection.practice),
                ),
                _NavItem(
                  icon: Icons.info_outline,
                  label: '关于',
                  selected: _section == MainSection.about,
                  onTap: () => setState(() => _section = MainSection.about),
                ),
                _NavItem(
                  icon: Icons.history_outlined,
                  label: '更新日志',
                  selected: _section == MainSection.changelog,
                  onTap: () => setState(() => _section = MainSection.changelog),
                ),
                const Spacer(),
                Padding(
                  padding: const EdgeInsets.all(18),
                  child: Container(
                    padding: const EdgeInsets.all(14),
                    decoration: BoxDecoration(
                      color: isDark
                          ? const Color(0xFF192334)
                          : const Color(0xFFF5F7FA),
                      borderRadius: BorderRadius.circular(12),
                    ),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          '后台常驻',
                          style: theme.textTheme.labelLarge?.copyWith(
                            fontWeight: FontWeight.w600,
                          ),
                        ),
                        const SizedBox(height: 5),
                        Text(
                          '关闭主界面后，程序仍在托盘运行。',
                          style: theme.textTheme.bodySmall?.copyWith(
                            color: theme.colorScheme.onSurface.withValues(
                              alpha: .58,
                            ),
                            height: 1.45,
                          ),
                        ),
                        const SizedBox(height: 12),
                        Text(
                          'v${UpdateService.currentVersion} · Flutter',
                          style: theme.textTheme.labelSmall?.copyWith(
                            color: theme.colorScheme.onSurface.withValues(
                              alpha: .48,
                            ),
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
              ],
            ),
          ),
          Expanded(
            child: Column(
              children: [
                _Header(
                  section: _section,
                  overlayVisible: widget.overlayVisible,
                  onThemeToggle: runtime.toggleTheme,
                  onToggleOverlay: widget.onToggleOverlay,
                ),
                Expanded(
                  child: switch (_section) {
                    MainSection.settings => _settingsPage(context),
                    MainSection.practice => PracticeScreen(runtime: runtime),
                    MainSection.about => _aboutPage(context),
                    MainSection.changelog => _changelogPage(context),
                  },
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _settingsPage(BuildContext context) {
    final theme = Theme.of(context);
    final settings = runtime.settings;
    return SingleChildScrollView(
      padding: const EdgeInsets.fromLTRB(32, 6, 32, 30),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Container(
            padding: const EdgeInsets.all(26),
            decoration: BoxDecoration(
              borderRadius: BorderRadius.circular(16),
              gradient: const LinearGradient(
                colors: [Color(0xFF172235), Color(0xFF293A52)],
                begin: Alignment.topLeft,
                end: Alignment.bottomRight,
              ),
            ),
            child: Row(
              children: [
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      const Text(
                        '让视线先学会，让手指随后记住',
                        style: TextStyle(
                          color: Colors.white,
                          fontSize: 22,
                          fontWeight: FontWeight.w700,
                        ),
                      ),
                      const SizedBox(height: 10),
                      Text(
                        '置顶键盘直接显示当前五笔版本的实际字根表，不显示助记歌。',
                        style: TextStyle(
                          color: Colors.white.withValues(alpha: .66),
                          fontSize: 13,
                        ),
                      ),
                      const SizedBox(height: 20),
                      Wrap(
                        spacing: 10,
                        children: [
                          FilledButton(
                            onPressed: widget.onToggleOverlay,
                            child: Text(
                              widget.overlayVisible ? '隐藏置顶键盘' : '显示置顶键盘',
                            ),
                          ),
                          OutlinedButton(
                            onPressed: () => setState(() {}),
                            style: OutlinedButton.styleFrom(
                              foregroundColor: Colors.white,
                              side: BorderSide(
                                color: Colors.white.withValues(alpha: .3),
                              ),
                            ),
                            child: const Text('刷新界面'),
                          ),
                        ],
                      ),
                    ],
                  ),
                ),
                const SizedBox(width: 20),
                Container(
                  width: 138,
                  height: 138,
                  decoration: BoxDecoration(
                    shape: BoxShape.circle,
                    color: Colors.white.withValues(alpha: .08),
                    border: Border.all(
                      color: Colors.white.withValues(alpha: .14),
                    ),
                  ),
                  child: Column(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      Text(
                        '当前版本',
                        style: TextStyle(
                          color: Colors.white.withValues(alpha: .54),
                          fontSize: 11,
                        ),
                      ),
                      const SizedBox(height: 5),
                      Text(
                        settings.edition.label,
                        style: const TextStyle(
                          color: Colors.white,
                          fontSize: 26,
                          fontWeight: FontWeight.w700,
                        ),
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),
          const SizedBox(height: 20),
          Text(
            '五笔版本',
            style: theme.textTheme.titleMedium?.copyWith(
              fontWeight: FontWeight.w700,
            ),
          ),
          const SizedBox(height: 10),
          Wrap(
            spacing: 10,
            runSpacing: 10,
            children: [
              for (final edition in WubiEdition.values)
                ChoiceChip(
                  label: Text(edition.label),
                  selected: settings.edition == edition,
                  onSelected: (_) => runtime.setEdition(edition),
                ),
            ],
          ),
          const SizedBox(height: 20),
          Card(
            child: Padding(
              padding: const EdgeInsets.all(20),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    '置顶键盘外观',
                    style: theme.textTheme.titleMedium?.copyWith(
                      fontWeight: FontWeight.w700,
                    ),
                  ),
                  const SizedBox(height: 5),
                  Text(
                    '键盘大字与字根小字会按比例显示，可在置顶模式拖动窗口。',
                    style: theme.textTheme.bodySmall?.copyWith(
                      color: theme.colorScheme.onSurface.withValues(alpha: .58),
                    ),
                  ),
                  const SizedBox(height: 16),
                  _SliderRow(
                    label: '透明度',
                    value: settings.keyboardOpacity,
                    min: .25,
                    max: 1,
                    valueLabel: '${(settings.keyboardOpacity * 100).round()}%',
                    onChanged: (value) =>
                        runtime.update((s) => s.keyboardOpacity = value),
                  ),
                  const SizedBox(height: 10),
                  _SliderRow(
                    label: '整体大小',
                    value: settings.keyboardScale,
                    min: .60,
                    max: 1.45,
                    valueLabel: '${(settings.keyboardScale * 100).round()}%',
                    onChanged: (value) =>
                        runtime.update((s) => s.keyboardScale = value),
                  ),
                  const SizedBox(height: 10),
                  SwitchListTile(
                    value: settings.showSimpleCodes,
                    onChanged: (value) =>
                        runtime.update((s) => s.showSimpleCodes = value),
                    title: const Text('显示一级简码'),
                    contentPadding: EdgeInsets.zero,
                  ),
                  SwitchListTile(
                    value: settings.showRootTable,
                    onChanged: (value) =>
                        runtime.update((s) => s.showRootTable = value),
                    title: const Text('显示字根表'),
                    contentPadding: EdgeInsets.zero,
                  ),
                  SwitchListTile(
                    value: settings.passiveHighlight,
                    onChanged: (value) =>
                        runtime.update((s) => s.passiveHighlight = value),
                    title: const Text('每 5 秒轻提示一个键位（潜移默化模式）'),
                    contentPadding: EdgeInsets.zero,
                  ),
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
                  Text(
                    '启动与更新',
                    style: theme.textTheme.titleMedium?.copyWith(
                      fontWeight: FontWeight.w700,
                    ),
                  ),
                  SwitchListTile(
                    value: settings.showKeyboardOnStartup,
                    onChanged: (value) =>
                        runtime.update((s) => s.showKeyboardOnStartup = value),
                    title: const Text('启动软件时显示置顶键盘'),
                    contentPadding: EdgeInsets.zero,
                  ),
                  SwitchListTile(
                    value: settings.autoCheckUpdates,
                    onChanged: (value) =>
                        runtime.update((s) => s.autoCheckUpdates = value),
                    title: const Text('启动时自动检查更新'),
                    contentPadding: EdgeInsets.zero,
                  ),
                  SwitchListTile(
                    value: _startupEnabled,
                    onChanged: (value) async {
                      await StartupService.setEnabled(value);
                      if (mounted) setState(() => _startupEnabled = value);
                    },
                    title: const Text('登录系统后自动启动'),
                    subtitle: Text(
                      Platform.isLinux
                          ? 'Linux：~/.config/autostart/passivewubi.desktop'
                          : '使用当前用户启动项',
                      style: theme.textTheme.bodySmall?.copyWith(
                        color: theme.colorScheme.onSurface.withValues(
                          alpha: .52,
                        ),
                      ),
                    ),
                    contentPadding: EdgeInsets.zero,
                  ),
                ],
              ),
            ),
          ),
          const SizedBox(height: 16),
          Wrap(
            spacing: 8,
            runSpacing: 8,
            children: [
              OutlinedButton(
                onPressed: runtime.log.openToday,
                child: const Text('打开今日日志'),
              ),
              OutlinedButton(
                onPressed: runtime.log.openFolder,
                child: const Text('打开日志目录'),
              ),
              TextButton(onPressed: widget.onExit, child: const Text('完全退出')),
            ],
          ),
        ],
      ),
    );
  }

  Widget _aboutPage(BuildContext context) {
    final theme = Theme.of(context);
    return SingleChildScrollView(
      padding: const EdgeInsets.fromLTRB(32, 6, 32, 30),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Card(
            child: Padding(
              padding: const EdgeInsets.all(22),
              child: Row(
                children: [
                  _LogoMark(size: 64),
                  const SizedBox(width: 16),
                  const Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          '肌忆五笔',
                          style: TextStyle(
                            fontSize: 24,
                            fontWeight: FontWeight.w700,
                          ),
                        ),
                        SizedBox(height: 4),
                        Text(
                          'PassiveWubi · Flutter / Dart',
                          style: TextStyle(fontSize: 13),
                        ),
                      ],
                    ),
                  ),
                  Chip(label: Text('v${UpdateService.currentVersion}')),
                ],
              ),
            ),
          ),
          const SizedBox(height: 16),
          Card(
            child: Padding(
              padding: const EdgeInsets.all(20),
              child: Column(
                children: [
                  _InfoRow('中文名称', '肌忆五笔'),
                  _InfoRow('英文名称', 'PassiveWubi'),
                  _InfoRow('技术栈', 'Flutter / Dart'),
                  _InfoRow('平台', 'Windows 10/11 · Ubuntu Linux'),
                  _InfoRow('程序数据', '设置与日志均保存在程序目录 PassiveWubiData 文件夹'),
                  _InfoRow('开源许可', 'MIT License'),
                  const Divider(height: 28),
                  Align(
                    alignment: Alignment.centerLeft,
                    child: Text(
                      '通过 GitHub 获取源码、反馈问题和检查更新。',
                      style: theme.textTheme.bodySmall?.copyWith(
                        color: theme.colorScheme.onSurface.withValues(
                          alpha: .62,
                        ),
                      ),
                    ),
                  ),
                  const SizedBox(height: 12),
                  Wrap(
                    spacing: 8,
                    children: [
                      OutlinedButton(
                        onPressed: () => launchUrl(
                          Uri.parse('https://github.com/CY68GI/PassiveWubi'),
                          mode: LaunchMode.externalApplication,
                        ),
                        child: const Text('打开 GitHub 仓库'),
                      ),
                      OutlinedButton(
                        onPressed: runtime.log.openToday,
                        child: const Text('打开今日日志'),
                      ),
                    ],
                  ),
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
                  Text(
                    '特别致谢',
                    style: theme.textTheme.titleMedium?.copyWith(
                      fontWeight: FontWeight.w700,
                    ),
                  ),
                  const SizedBox(height: 12),
                  const Text(
                    '感谢王永民先生发明五笔字型输入法，为中文输入和本项目的'
                    '字根学习体系奠定基础。',
                    style: TextStyle(height: 1.65),
                  ),
                  const SizedBox(height: 8),
                  Text(
                    '感谢 DeepSeek V4 Flash 人工智能与 ChatGPT Codex 工具在'
                    '代码实现、测试和发布过程中的协助。',
                    style: theme.textTheme.bodyMedium?.copyWith(height: 1.65),
                  ),
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
                  Text(
                    '软件更新',
                    style: theme.textTheme.titleMedium?.copyWith(
                      fontWeight: FontWeight.w700,
                    ),
                  ),
                  const SizedBox(height: 6),
                  Text(
                    _updateStatus,
                    style: theme.textTheme.bodySmall?.copyWith(
                      color: theme.colorScheme.onSurface.withValues(alpha: .62),
                    ),
                  ),
                  const SizedBox(height: 12),
                  Wrap(
                    spacing: 8,
                    children: [
                      FilledButton(
                        onPressed: _checkingUpdate ? null : _checkUpdate,
                        child: Text(_checkingUpdate ? '正在检查…' : '检查更新'),
                      ),
                      OutlinedButton(
                        onPressed: () => launchUrl(
                          Uri.parse(
                            'https://github.com/CY68GI/PassiveWubi/releases/latest',
                          ),
                          mode: LaunchMode.externalApplication,
                        ),
                        child: const Text('打开发布页'),
                      ),
                    ],
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _changelogPage(BuildContext context) {
    final theme = Theme.of(context);
    return SingleChildScrollView(
      padding: const EdgeInsets.fromLTRB(32, 6, 32, 30),
      child: Card(
        child: Padding(
          padding: const EdgeInsets.all(22),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const Row(
                children: [
                  Chip(label: Text('v${UpdateService.currentVersion}')),
                  SizedBox(width: 10),
                  Text(
                    '双窗口、图标与桌面体验更新',
                    style: TextStyle(fontSize: 19, fontWeight: FontWeight.w700),
                  ),
                ],
              ),
              const SizedBox(height: 8),
              Text(
                '2026-11-11',
                style: theme.textTheme.bodySmall?.copyWith(
                  color: theme.colorScheme.onSurface.withValues(alpha: .55),
                ),
              ),
              const Divider(height: 28),
              const Text(
                '• 主界面与置顶键盘改为独立窗口，可同时显示并由主界面控制显隐\n'
                '• 重绘所有图标为橙色圆角矩形和白色“五”，移除悬浮键盘透明阴影\n'
                '• 修复 Ubuntu 界面闪烁，设置和日志统一保存到程序目录\n'
                '• Windows 构建 Windows 包，Ubuntu 构建 deb，Windows 统一发布\n'
                '• 增加对王永民先生、DeepSeek V4 Flash 和 ChatGPT Codex 的致谢\n'
                '• 修复无仓库上下文时 GitHub Release 无法创建和上传附件的问题\n'
                '• 修复 GitHub Release 附件路径，恢复 Windows 与 Ubuntu 发布包上传\n'
                '• Windows WPF 与 Linux Avalonia 客户端合并为单一 Flutter 工程\n'
                '• 使用 Dart 共享全部五笔字根、题库和答题判定\n'
                '• 保留置顶键盘、深浅主题、托盘、日志、更新检查和开机启动\n'
                '• 新增速度测试、拆字练习和词组练习\n'
                '• 提供 Windows 目录发布与 Ubuntu .deb 打包流程',
                style: TextStyle(height: 1.9),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Future<void> _checkUpdate() async {
    setState(() => _checkingUpdate = true);
    final result = await UpdateService.check();
    if (!mounted) return;
    setState(() {
      _checkingUpdate = false;
      _updateStatus = result.success
          ? result.hasUpdate
                ? '发现新版本 ${result.latest}，当前版本 ${result.current}。'
                : '当前已是最新版本 ${result.current}。'
          : result.error ?? '更新检查失败。';
    });
  }
}

class _Header extends StatelessWidget {
  const _Header({
    required this.section,
    required this.overlayVisible,
    required this.onThemeToggle,
    required this.onToggleOverlay,
  });

  final MainSection section;
  final bool overlayVisible;
  final Future<void> Function() onThemeToggle;
  final Future<void> Function() onToggleOverlay;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final (title, subtitle) = switch (section) {
      MainSection.settings => ('键盘与学习', '选择五笔版本，让字根在视线边缘慢慢变成肌肉记忆。'),
      MainSection.practice => ('打字练习', '速度、字根、拆字与词组练习，题库随五笔版本变化。'),
      MainSection.about => ('关于', '版本信息、运行日志、开源地址与更新检查。'),
      MainSection.changelog => ('更新日志', '查看每次版本更新带来的变化。'),
    };
    return SizedBox(
      height: 84,
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 32),
        child: Row(
          children: [
            Expanded(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    title,
                    style: theme.textTheme.headlineSmall?.copyWith(
                      fontWeight: FontWeight.w700,
                    ),
                  ),
                  const SizedBox(height: 5),
                  Text(
                    subtitle,
                    style: theme.textTheme.bodySmall?.copyWith(
                      color: theme.colorScheme.onSurface.withValues(alpha: .58),
                    ),
                  ),
                ],
              ),
            ),
            OutlinedButton.icon(
              onPressed: onThemeToggle,
              icon: Icon(
                theme.brightness == Brightness.dark
                    ? Icons.light_mode_outlined
                    : Icons.dark_mode_outlined,
              ),
              label: Text(theme.brightness == Brightness.dark ? '浅色' : '深色'),
            ),
            const SizedBox(width: 8),
            FilledButton.icon(
              onPressed: onToggleOverlay,
              icon: Icon(
                overlayVisible
                    ? Icons.keyboard_hide_outlined
                    : Icons.keyboard_alt_outlined,
              ),
              label: Text(overlayVisible ? '隐藏键盘' : '显示键盘'),
            ),
          ],
        ),
      ),
    );
  }
}

class _NavItem extends StatelessWidget {
  const _NavItem({
    required this.icon,
    required this.label,
    required this.selected,
    required this.onTap,
  });

  final IconData icon;
  final String label;
  final bool selected;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 3),
      child: Material(
        color: selected
            ? theme.colorScheme.primary.withValues(alpha: .14)
            : Colors.transparent,
        borderRadius: BorderRadius.circular(10),
        child: InkWell(
          onTap: onTap,
          borderRadius: BorderRadius.circular(10),
          child: Padding(
            padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 12),
            child: Row(
              children: [
                Icon(
                  icon,
                  size: 19,
                  color: selected
                      ? theme.colorScheme.primary
                      : theme.colorScheme.onSurface.withValues(alpha: .58),
                ),
                const SizedBox(width: 12),
                Text(
                  label,
                  style: TextStyle(
                    fontWeight: selected ? FontWeight.w600 : FontWeight.w400,
                    color: selected
                        ? theme.colorScheme.onSurface
                        : theme.colorScheme.onSurface.withValues(alpha: .66),
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

class _LogoMark extends StatelessWidget {
  const _LogoMark({required this.size});
  final double size;

  @override
  Widget build(BuildContext context) => Container(
    width: size,
    height: size,
    decoration: BoxDecoration(
      color: const Color(0xFFF57C1F),
      borderRadius: BorderRadius.circular(size * .28),
    ),
    alignment: Alignment.center,
    child: Text(
      '五',
      style: TextStyle(
        color: Colors.white,
        fontSize: size * .52,
        fontWeight: FontWeight.w700,
      ),
    ),
  );
}

class _SliderRow extends StatelessWidget {
  const _SliderRow({
    required this.label,
    required this.value,
    required this.min,
    required this.max,
    required this.valueLabel,
    required this.onChanged,
  });

  final String label;
  final double value;
  final double min;
  final double max;
  final String valueLabel;
  final ValueChanged<double> onChanged;

  @override
  Widget build(BuildContext context) => Row(
    children: [
      SizedBox(width: 90, child: Text(label)),
      Expanded(
        child: Slider(value: value, min: min, max: max, onChanged: onChanged),
      ),
      SizedBox(
        width: 54,
        child: Text(
          valueLabel,
          textAlign: TextAlign.right,
          style: Theme.of(context).textTheme.bodySmall,
        ),
      ),
    ],
  );
}

class _InfoRow extends StatelessWidget {
  const _InfoRow(this.label, this.value);
  final String label;
  final String value;

  @override
  Widget build(BuildContext context) => Padding(
    padding: const EdgeInsets.symmetric(vertical: 6),
    child: Row(
      children: [
        SizedBox(
          width: 90,
          child: Text(
            label,
            style: Theme.of(context).textTheme.bodySmall?.copyWith(
              color: Theme.of(context).colorScheme.onSurface
                  .withValues(alpha: .58),
            ),
          ),
        ),
        Expanded(child: Text(value)),
      ],
    ),
  );
}
