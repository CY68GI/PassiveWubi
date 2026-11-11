# 更新日志

## v2.0.0 - 2026-11-11

### 架构迁移

- 将 Windows WPF 和 Linux Avalonia 客户端迁移为单一 Flutter 工程。
- 使用 Dart 重写设置、题库、练习、日志、更新和桌面交互逻辑。
- Windows/Linux 共用 Flutter UI 与三版本五笔题库资源。

### 功能

- 保留置顶字根表键盘、透明度、缩放、深浅主题和潜移默化模式。
- 增加速度测试、五笔口诀、字根、拆字和词组练习。
- 增加 Windows 目录发布包和 Ubuntu `.deb` 构建流程。
- 增加 Flutter/Dart 测试和 Windows/Linux CI。

## v1.2.0 - 2026-11-10

- C#/Avalonia 版本的最后功能版本；历史实现保留在 Git 标签 `v1.2.0`。