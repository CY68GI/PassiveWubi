# 更新日志

## v2.1.4 - 2026-11-11

### GitHub Ubuntu Runner

- Ubuntu 构建依赖安装强制使用 IPv4 并限制为 GTK/X11 最小依赖集。
- Ubuntu 依赖步骤设置超时，避免异常网络长期占用发布任务。
- Ubuntu 发布 runner 改用官方 IPv4 APT 镜像，避免默认 Azure 镜像连接卡住。

## v2.1.3 - 2026-11-11

### Ubuntu 构建

- Ubuntu 发布任务只补装 GTK/X11 所需的少量开发包，减少 GitHub Runner 依赖安装时间。
- Windows ZIP 与 Ubuntu `.deb` 继续由双平台构建，并由 Windows 任务统一发布。

## v2.1.2 - 2026-11-11

### 发布稳定性

- Ubuntu 发布任务直接使用 GitHub Runner 预置的 APT 包索引，避免系统索引更新长时间阻塞。
- 继续同时发布 Windows ZIP 与 Ubuntu `.deb`，由 Windows 发布任务统一上传。

## v2.1.1 - 2026-11-11

### Ubuntu 发布

- GitHub Release 恢复 Ubuntu `.deb` 构建和附件上传。
- Windows 继续构建 Windows ZIP，最终 Release 仍由 Windows 任务统一发布。
- 程序设置和日志统一放入程序目录的 `PassiveWubiData/` 子目录。
- Ubuntu 安装包创建可写 `PassiveWubiData/` 目录，解决系统安装目录不可写问题。

## v2.1.0 - 2026-11-11

### 桌面体验

- 重绘全部应用、托盘和窗口图标，统一为橙色圆角矩形与白色“五”。
- 主界面和置顶键盘改为独立窗口，可以同时存在，并由主界面统一显示或隐藏。
- 移除悬浮键盘内容与系统窗口的多余透明阴影。
- 修复 Linux 主界面和悬浮键盘闪烁，透明度改由 Flutter 内容统一绘制。
- 设置、日志和运行数据只保存在程序目录，不再写入 AppData、XDG 或其他用户目录。
- GitHub Release 改为只在 Windows runner 构建和发布 Windows 包；Ubuntu 用于开发测试。
- 在软件关于页和 README 中增加对王永民先生、DeepSeek V4 Flash 和 ChatGPT Codex 的致谢。

## v2.0.3 - 2026-11-11

### 发布流程

- 在最终发布任务中检出仓库并显式设置 GitHub 仓库上下文。
- 确保 `gh release` 能创建 Release 并上传 Windows、Ubuntu 安装附件。
- 重新生成带 2.0.3 版本号的 Windows 和 Ubuntu 发布包。

## v2.0.2 - 2026-11-11

### 发布修正

- 修复 Release 工作流下载构建附件后使用错误路径导致上传失败的问题。
- 统一 Windows 与 Ubuntu 发布包的版本号和自动更新检查版本。
- 重新生成并验证 Ubuntu `.deb` 安装包与 Windows 发布包。

## v2.0.1 - 2026-11-11

### 架构迁移

- 将 Windows WPF 和 Linux Avalonia 客户端迁移为单一 Flutter 工程。
- 使用 Dart 重写设置、题库、练习、日志、更新和桌面交互逻辑。
- Windows/Linux 共用 Flutter UI 与三版本五笔题库资源。

### 功能

- 保留置顶字根表键盘、透明度、缩放、深浅主题和潜移默化模式。
- 增加速度测试、五笔口诀、字根、拆字和词组练习。
- 增加 Windows 目录发布包和 Ubuntu `.deb` 构建流程。
- 增加 Flutter/Dart 测试和 Windows/Linux CI。

### 打包修正

- 修复 Ubuntu `.deb` 中 Flutter 插件库的 RUNPATH，使安装后可正常定位插件。
- 增加 Debian changelog、copyright、man page 和 lintian override。
- Release 工作流改为先创建或复用 Release，再分别上传构建附件。

## v1.2.0 - 2026-11-10

- C#/Avalonia 版本的最后功能版本；历史实现保留在 Git 标签 `v1.2.0`。
