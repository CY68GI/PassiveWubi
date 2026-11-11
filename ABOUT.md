# About 肌忆五笔 · PassiveWubi

## 项目是什么

肌忆五笔是一个常驻桌面的五笔学习工具。它把五笔字根表显示在屏幕置顶的半透明键盘上，让字根、键名和一级简码持续出现在日常工作的视线边缘。

项目英文名为 **PassiveWubi**，中文名为 **肌忆五笔**。项目已迁移到 **Flutter + Dart**，使用 MIT License 开源。

## 功能

- 五笔 86 版、98 版和新世纪版。
- 置顶字根表键盘，不显示助记歌。
- 键盘透明度、缩放、拖拽定位、深浅主题。
- 系统托盘和后台驻留。
- 速度测试和五笔口诀、字根、拆字、词组练习。
- 练习题库随五笔版本切换。
- 关于、更新日志、日志目录、GitHub 更新检查。
- Windows 用户启动项和 Ubuntu XDG 自启动。

## 架构

Flutter 主工程位于仓库根目录，`lib/core` 保存 Dart 数据模型、设置、日志、更新和练习会话逻辑，`lib/ui` 保存主界面、置顶键盘和练习界面。Windows/Linux 共用 Dart 代码和资源，系统窗口、托盘和打包配置由 Flutter runner 提供。

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
- Ubuntu 的置顶和透明效果依赖桌面合成器；GNOME 托盘可能需要 AppIndicator 扩展，应用内设置按钮和托盘入口均可使用。
- 其他 Linux 发行版尚未列入官方支持范围。

## Ubuntu 安装

```bash
sudo dpkg -i PassiveWubi-Flutter-v2.0.1-linux-amd64.deb
passivewubi
```

## 项目地址

- GitHub：<https://github.com/CY68GI/PassiveWubi>
- Issues：<https://github.com/CY68GI/PassiveWubi/issues>
- Releases：<https://github.com/CY68GI/PassiveWubi/releases>

## 许可证

MIT License。五笔输入法由王永民先生发明；项目使用公开字根资料整理版本键位数据。
