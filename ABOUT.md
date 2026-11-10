# About 肌忆五笔 · PassiveWubi

## 项目是什么

肌忆五笔是一个常驻桌面的五笔学习工具。它把五笔字根表显示在屏幕置顶的半透明键盘上，让字根、键名和一级简码持续出现在日常工作的视线边缘。默认每 5 秒轻轻点亮一个键位，以低打扰的方式积累视觉和肌肉记忆。

项目英文名为 **PassiveWubi**，中文名为 **肌忆五笔**，源码采用 C# 编写，使用 MIT License 开源。

## 核心功能

- 支持五笔 86 版、98 版和新世纪版。
- 26 键置顶键盘，展示实际字根表，不显示助记歌。
- 键位显示键名、一级简码和该键字根字符集合。
- 可拖动、可缩放，支持 25%–100% 透明度。
- 潜移默化模式每 5 秒轻提示一个键位。
- 支持统一暗色与浅色主题。
- 内置速度测试：可选择或自定义测试文字，并设置 1–50 分钟计时。
- 内置五笔口诀、字根、拆字和词组练习，题目随 86/98/新世纪版本变化。
- 拆字覆盖成字字根、键名字、常用字、三级简码、识别码和难拆字。
- 词组覆盖二/三/四字、混合和混合乱序。
- 主窗口关闭后继续在后台运行，并从托盘恢复。
- 关于页、更新日志、运行日志和 GitHub 更新检查。
- 日志保存在程序可写目录；无法写入时使用系统用户状态目录。

## 平台支持

### Windows

- Windows 10 / 11 x64。
- WPF 与 Windows Forms 托盘组件。
- 自包含单文件发布，不要求用户另行安装 .NET。
- 使用当前用户注册表实现随 Windows 启动。

### Linux

- 首批支持 Ubuntu 22.04 / 24.04 x64。
- 使用 Avalonia 12 和 .NET 10。
- 自包含 `linux-x64` 发布。
- 支持置顶键盘、透明度、缩放、主题、托盘、日志和 XDG 自启动。
- 其他 Linux 发行版暂未列入官方支持范围，后续根据兼容性反馈再考虑。

Ubuntu 上建议使用默认 X11/XWayland 桌面会话。置顶与窗口透明依赖桌面合成器；GNOME 的托盘图标可能需要 AppIndicator 扩展，主窗口和键盘内均提供设置与隐藏入口。

## 架构

```text
src/
├─ PassiveWubi.Core/      跨平台五笔版本、键位和字根数据
├─ PassiveWubi/           Windows WPF 客户端
└─ PassiveWubi.Linux/     Ubuntu/Linux Avalonia 客户端
```

Windows 与 Linux 共用 `PassiveWubi.Core`，确保三个版本的键位和字根数据保持一致。窗口、托盘、自启动和平台路径逻辑分别位于各自客户端项目中。

## Linux 安装

从 Releases 下载 Linux x64 的 `.tar.gz`，解压后执行：

```bash
bash install-ubuntu.sh
```

安装脚本会把程序放入 `~/.local/share/passivewubi`，创建 `~/.local/bin/passivewubi` 启动命令和桌面菜单项。也可以不安装，直接执行：

```bash
chmod +x PassiveWubi.Linux
./PassiveWubi.Linux
```

建议把便携版解压到用户可写目录，例如 `~/Applications/PassiveWubi`。如果程序目录不可写，设置和日志会回退到 `$XDG_CONFIG_HOME` 与 `$XDG_STATE_HOME` 对应的目录。

## 数据与隐私

肌忆五笔不上传输入内容、键位记录或运行日志。更新检查只访问 GitHub 的公开 Release 信息。运行日志只写入本机。

## 项目地址

- GitHub：<https://github.com/CY68GI/PassiveWubi>
- Releases：<https://github.com/CY68GI/PassiveWubi/releases>
- Issues：<https://github.com/CY68GI/PassiveWubi/issues>

## 许可证

MIT License。五笔输入法由王永民先生发明；项目使用公开字根资料整理各版本键位数据。
