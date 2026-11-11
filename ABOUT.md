# About 肌忆五笔 · PassiveWubi

## 项目是什么

肌忆五笔是一个常驻桌面的五笔学习工具。它把五笔字根表显示在屏幕置顶的半透明键盘上，让字根、键名和一级简码持续出现在日常工作的视线边缘。

项目英文名为 **PassiveWubi**，中文名为 **肌忆五笔**。项目已迁移到 **Flutter + Dart**，使用 MIT License 开源。

## 功能

- 五笔 86 版、98 版和新世纪版。
- 置顶字根表键盘，不显示助记歌。
- 主界面与置顶键盘使用独立窗口，可以同时显示；键盘显隐由主界面统一控制。
- 键盘透明度、缩放、拖拽定位、深浅主题。
- 系统托盘和后台驻留。
- 速度测试和五笔口诀、字根、拆字、词组练习。
- 练习题库随五笔版本切换。
- 关于、更新日志、日志目录、GitHub 更新检查。
- Windows 用户启动项和 Ubuntu XDG 自启动。
- 设置、日志和运行数据只写入程序目录的 `PassiveWubiData/` 子目录，便于便携使用和完整清理。

## 架构

Flutter 主工程位于仓库根目录，`lib/core` 保存 Dart 数据模型、设置、日志、更新、练习会话和窗口通信逻辑，`lib/ui` 保存主界面、置顶键盘和练习界面。桌面端通过 `desktop_multi_window` 为主界面和键盘分别创建 Flutter 引擎，键盘窗口独立置顶，主窗口不置顶。

```text
lib/
├─ core/     Dart 设置、题库、日志、更新、练习
└─ ui/       Flutter 主界面、置顶键盘、练习页面
assets/     五笔题库和应用图标
linux/      Flutter Linux runner 与 deb 打包
windows/    Flutter Windows runner
test/       Dart/Flutter 测试
```

## 平台支持

- Windows 10 / 11 x64。
- Ubuntu 22.04 / 24.04 x64。
- Windows 提供 ZIP 发布包；Ubuntu 提供 `.deb` 发布包，并承担开发测试。
- Ubuntu 的置顶和透明效果依赖桌面合成器；GNOME 托盘可能需要 AppIndicator 扩展，应用内设置按钮和托盘入口均可使用。
- 其他 Linux 发行版尚未列入官方支持范围。

## Ubuntu 安装

Ubuntu `.deb` 会作为 GitHub Release 附件发布：

```bash
sudo dpkg -i PassiveWubi-Flutter-v2.1.4-linux-amd64.deb
passivewubi
```

程序在自身目录的 `PassiveWubiData/` 中写入 `settings.json` 和 `logs/`。便携运行时请将程序放在当前用户可写目录中。

## 项目地址

- GitHub：<https://github.com/CY68GI/PassiveWubi>
- Issues：<https://github.com/CY68GI/PassiveWubi/issues>
- Releases：<https://github.com/CY68GI/PassiveWubi/releases>

## 许可证

MIT License。

## 致谢

- 感谢王永民先生发明五笔字型输入法；项目使用公开字根资料整理版本键位数据。
- 感谢 DeepSeek V4 Flash 人工智能在代码实现和问题分析中的协助。
- 感谢 ChatGPT Codex 工具在开发、测试和发布流程中的协助。
