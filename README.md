# 肌忆五笔 · PassiveWubi

肌忆五笔是一个常驻桌面的五笔学习工具。它以 Flutter 构建，使用 Dart 编写，主界面和置顶字根键盘使用两个独立窗口，可以同时显示。

## 核心功能

- 五笔 86 版、98 版和新世纪版。
- 独立的屏幕置顶 26 键字根表键盘，显示键名、一级简码和实际字根。
- 主界面不置顶；键盘窗口的显示和隐藏由主界面或托盘统一控制。
- 透明度、整体缩放、拖拽定位、深浅主题。
- 每 5 秒轻提示一个键位的潜移默化模式。
- 主界面关闭后继续在系统托盘运行。
- 速度测试：内置文章或自定义文字、1–50 分钟计时、速度和正确率统计。
- 键位练习：五笔口诀、字根、拆字和词组。
- 拆字题库：成字字根、键名字、常用字、一级/二级/三级简码、识别码、难拆字。
- 词题库：二字、三字、四字、混合词组、混合乱序。
- 练习题目跟随当前五笔版本变化。
- 关于、更新日志、运行日志、GitHub 更新检查和开机启动。
- 设置、日志和其他运行数据只保存在程序目录的 `PassiveWubiData/` 文件夹。

## 平台

- Windows 10 / 11 x64：提供 Windows ZIP 发布包。
- Ubuntu 22.04 / 24.04 x64：提供 Ubuntu `.deb` 发布包，并用于开发与兼容性测试。
- 其他 Linux 发行版暂未列入官方支持范围。

## 开发环境

需要 Flutter `3.47.9` 或兼容的 stable 版本，Dart `3.13.7` 或兼容版本。

```bash
flutter pub get
flutter test
flutter analyze
```

Windows 构建：

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\build_flutter_windows.ps1 -Version 2.1.3
```

Ubuntu/Linux 构建：

```bash
flutter build linux --release
bash packaging/linux/build_deb.sh 2.1.3
```

Ubuntu 构建需要 GTK 3、X11、Xi、Clang、CMake、Ninja 和 pkg-config。项目 CI 使用 Ubuntu 24.04。

GitHub Actions 使用 Windows runner 构建 Windows ZIP，使用 Ubuntu runner 构建并测试 `.deb`；最终由 Windows 发布任务统一上传全部 Release 附件。

## 目录

```text
lib/                    Flutter/Dart 客户端
assets/data/            三版本五笔练习题库
assets/icons/           应用和托盘图标
linux/                  Flutter Linux runner
windows/                Flutter Windows runner
packaging/linux/        Ubuntu .deb 构建脚本
test/                   Dart/Flutter 测试
.github/workflows/      Windows/Linux CI 与 Release
```

Flutter 编译产物位于 `build/`，发布产物位于 `dist/`，两者都不提交到源码。

## 关于

项目详细说明见 [ABOUT.md](ABOUT.md)，版本变化见 [CHANGELOG.md](CHANGELOG.md)。

## 致谢

- 感谢王永民先生发明五笔字型输入法，为中文输入和本项目的字根学习体系奠定基础。
- 感谢 DeepSeek V4 Flash 人工智能在代码实现和问题分析中的协助。
- 感谢 ChatGPT Codex 工具在开发、测试和发布流程中的协助。

## 许可证

[MIT License](LICENSE)
