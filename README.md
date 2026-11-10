# 肌忆五笔 · PassiveWubi

肌忆五笔是一个常驻桌面的五笔键位学习工具。它把当前选择的五笔版本以半透明键盘固定在屏幕上方，让字根和一级简码持续出现在工作视线中；默认每 5 秒轻微点亮一个键位，在低打扰的状态下积累视觉记忆。

![应用标识](src/PassiveWubi/Assets/PassiveWubi.svg)

## 功能

- 支持五笔 86 版、98 版和新世纪版。
- 置顶显示完整 26 键学习键盘，25 个五笔键展示键名、一级简码和实际字根表，Z 键展示学习键提示。
- 可拖动、可整体缩放，可调整 25%–100% 透明度。
- 可选“潜移默化模式”，每 5 秒轻提示一个键位。
- 主界面用于版本、外观、启动和更新设置。
- 支持统一暗色、浅色主题即时切换。
- 关闭主界面不会退出程序；可通过系统托盘重新打开主界面、显示/隐藏键盘或彻底退出。
- 关于页包含软件信息、日志入口、GitHub 地址和更新检查。
- 启动时自动检查 GitHub Release，也可手动检查。
- Windows 支持随 Windows 启动，Ubuntu 支持通过 XDG 登录桌面自启动。
- 日志按日期保存在程序目录的 `logs` 文件夹。

## 系统要求

- Windows 10 / 11 x64：WPF 客户端。
- Ubuntu 22.04 / 24.04 x64：Avalonia Linux 客户端。
- Release 包均为自包含程序，不需要另外安装 .NET。
- 其他 Linux 发行版暂未列入官方支持范围。

## 使用

### Windows

1. 从 [Releases](https://github.com/CY68GI/PassiveWubi/releases) 下载最新的 `PassiveWubi-v*-win-x64.zip`。
2. 解压到普通用户可写的目录，例如 `D:\Apps\PassiveWubi`。
3. 运行 `PassiveWubi.exe`。
4. 在主界面选择五笔版本和键盘透明度、大小。
5. 拖动置顶键盘顶部移动位置，拖动右下角调整大小。
6. 关闭主界面后，从系统托盘双击图标重新打开；退出软件请使用托盘菜单。

> 请勿只把 exe 放进受保护的 `Program Files` 目录。设置和日志需要写入程序所在目录，便携版应放在用户可写目录。

### Ubuntu Linux

1. 下载 `PassiveWubi-Linux-x64-v*.tar.gz`。
2. 解压后进入目录。
3. 可选执行 `bash install-ubuntu.sh` 安装到用户目录；也可以直接 `chmod +x PassiveWubi.Linux` 后运行。
4. 已安装时可在应用菜单搜索“肌忆五笔”，或在终端运行 `passivewubi`。

Ubuntu 的置顶与透明效果依赖桌面合成器。GNOME 托盘图标可能需要 AppIndicator 扩展；主窗口和键盘内部均提供设置与隐藏入口。

## 数据与隐私

程序不上传键位记录或运行日志。更新检查只请求 GitHub 公开的 Release 信息；日志仅保存在本机程序目录。

## 五笔字根数据

置顶键盘显示实际字根字符，不显示助记歌。86 版逐键字根依据公开字根资料整理；98 版与新世纪版按各自字根键位表整理键名、一级简码和字根集合。若发现某个版本的字根归类与本机输入法不一致，请在 Issue 中注明版本、键位和字根字符。

## 源码与目录

```text
PassiveWubi/
├─ src/PassiveWubi.Core/   跨平台五笔版本、键位和字根数据
├─ src/PassiveWubi/        Windows C# / WPF 源码
├─ src/PassiveWubi.Linux/  Ubuntu/Linux C# / Avalonia 源码
├─ packaging/linux/        Ubuntu 安装脚本
├─ scripts/                Windows/Linux 发布与图标生成脚本
├─ .github/workflows/      GitHub Actions 发布流程
├─ dist/                   编译产物（已忽略，不进入源码提交）
├─ PassiveWubi.sln
├─ ABOUT.md
├─ CHANGELOG.md
└─ LICENSE
```

源码目录中不包含编译后的可运行程序。项目通过根目录 `Directory.Build.props` 把编译中间文件和输出定向到 `dist`，发布脚本也把便携包放到 `dist`。

## 本地构建

需要 .NET 10 SDK。Windows 客户端还需要 Windows Desktop 目标包。

```powershell
dotnet build .\PassiveWubi.sln -c Release
dotnet run --project .\src\PassiveWubi\PassiveWubi.csproj
```

生成 Windows 64 位单文件便携包：

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\build-release.ps1 -Version 1.1.0
```

输出：

```text
dist/PassiveWubi-v1.1.0-win-x64/
dist/PassiveWubi-v1.1.0-win-x64.zip
dist/PassiveWubi-v1.1.0-win-x64.zip.sha256
```

生成 Ubuntu Linux x64 自包含包：

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\build-linux-release.ps1 -Version 1.1.0
```

Linux 包输出为 `dist/PassiveWubi-Linux-x64-v1.1.0.tar.gz` 及同名 `.sha256` 文件。

## 发布

推送形如 `v1.1.0` 的标签会触发 `.github/workflows/release.yml`，同时构建 Windows ZIP 与 Linux TAR.GZ，并创建包含四个附件的 GitHub Release。

## 致谢

五笔输入法由王永民先生发明。项目使用公开字根表资料整理版本键位与字根字符；如发现键位内容需要校正，欢迎提交 Issue 或 Pull Request。

## 许可证

[MIT License](LICENSE)
