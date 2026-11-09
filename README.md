# 肌忆五笔 · PassiveWubi

肌忆五笔是一个常驻桌面的五笔键位学习工具。它把当前选择的五笔版本以半透明键盘固定在屏幕上方，让字根和一级简码持续出现在工作视线中；默认每 5 秒轻微点亮一个键位，在低打扰的状态下积累视觉记忆。

![应用标识](src/PassiveWubi/Assets/PassiveWubi.svg)

## 功能

- 支持五笔 86 版、98 版和新世纪版。
- 置顶显示完整 26 键学习键盘，25 个五笔键展示一级简码和字根口诀，Z 键展示学习键提示。
- 可拖动、可整体缩放，可调整 25%–100% 透明度。
- 可选“潜移默化模式”，每 5 秒轻提示一个键位。
- 主界面用于版本、外观、启动和更新设置。
- 支持统一暗色、浅色主题即时切换。
- 关闭主界面不会退出程序；可通过系统托盘重新打开主界面、显示/隐藏键盘或彻底退出。
- 关于页包含软件信息、日志入口、GitHub 地址和更新检查。
- 启动时自动检查 GitHub Release，也可手动检查。
- 可选随 Windows 启动。
- 日志按日期保存在程序目录的 `logs` 文件夹。

## 系统要求

- Windows 10 或 Windows 11，64 位。
- Release 包为自包含单文件程序，不需要另外安装 .NET。

## 使用

1. 从 [Releases](https://github.com/CY68GI/PassiveWubi/releases) 下载最新的 `PassiveWubi-v*-win-x64.zip`。
2. 解压到普通用户可写的目录，例如 `D:\Apps\PassiveWubi`。
3. 运行 `PassiveWubi.exe`。
4. 在主界面选择五笔版本和键盘透明度、大小。
5. 拖动置顶键盘顶部移动位置，拖动右下角调整大小。
6. 关闭主界面后，从系统托盘双击图标重新打开；退出软件请使用托盘菜单。

> 请勿只把 exe 放进受保护的 `Program Files` 目录。设置和日志需要写入程序所在目录，便携版应放在用户可写目录。

## 数据与隐私

程序不上传键位记录或运行日志。更新检查只请求 GitHub 公开的 Release 信息；日志仅保存在本机程序目录。

## 源码与目录

```text
PassiveWubi/
├─ src/PassiveWubi/        C# / WPF 源码与图标资源
├─ scripts/                本地发布与图标生成脚本
├─ .github/workflows/      GitHub Actions 发布流程
├─ dist/                   编译产物（已忽略，不进入源码提交）
├─ PassiveWubi.sln
├─ CHANGELOG.md
└─ LICENSE
```

源码目录中不包含编译后的可运行程序。项目通过根目录 `Directory.Build.props` 把编译中间文件和输出定向到 `dist`，发布脚本也把便携包放到 `dist`。

## 本地构建

需要 .NET 10 SDK 和 Windows Desktop 运行时/目标包。

```powershell
dotnet build .\PassiveWubi.sln -c Release
dotnet run --project .\src\PassiveWubi\PassiveWubi.csproj
```

生成 Windows 64 位单文件便携包：

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\build-release.ps1 -Version 1.0.1
```

输出：

```text
dist/PassiveWubi-v1.0.1-win-x64/
dist/PassiveWubi-v1.0.1-win-x64.zip
dist/PassiveWubi-v1.0.1-win-x64.zip.sha256
```

## 发布

推送形如 `v1.0.0` 的标签会触发 `.github/workflows/release.yml`，在 Windows Runner 上构建便携包并创建 GitHub Release。也可以手动运行发布脚本后上传 `dist` 中的 zip 和校验文件。

## 致谢

五笔输入法由王永民先生发明。项目使用公开资料整理版本键位与字根口诀；如发现键位内容需要校正，欢迎提交 Issue 或 Pull Request。

## 许可证

[MIT License](LICENSE)
