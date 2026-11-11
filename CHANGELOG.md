# 更新日志

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
