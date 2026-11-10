#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
APP_DIR="${XDG_DATA_HOME:-$HOME/.local/share}/passivewubi"
BIN_DIR="$HOME/.local/bin"
DESKTOP_DIR="${XDG_DATA_HOME:-$HOME/.local/share}/applications"
ICON_DIR="${XDG_DATA_HOME:-$HOME/.local/share}/icons/hicolor/scalable/apps"
SOURCE_BINARY="$SCRIPT_DIR/PassiveWubi.Linux"

if [[ ! -f "$SOURCE_BINARY" ]]; then
  echo "PassiveWubi.Linux not found next to install-ubuntu.sh" >&2
  exit 1
fi

mkdir -p "$APP_DIR" "$BIN_DIR" "$DESKTOP_DIR" "$ICON_DIR"
install -m 755 "$SOURCE_BINARY" "$APP_DIR/PassiveWubi.Linux"
install -m 644 "$SCRIPT_DIR/ABOUT.md" "$APP_DIR/ABOUT.md"
install -m 644 "$SCRIPT_DIR/README.md" "$APP_DIR/README.md"
install -m 644 "$SCRIPT_DIR/CHANGELOG.md" "$APP_DIR/CHANGELOG.md"
install -m 644 "$SCRIPT_DIR/LICENSE" "$APP_DIR/LICENSE"
if [[ -f "$SCRIPT_DIR/PassiveWubi.svg" ]]; then
  install -m 644 "$SCRIPT_DIR/PassiveWubi.svg" "$ICON_DIR/passivewubi.svg"
fi

ln -sfn "$APP_DIR/PassiveWubi.Linux" "$BIN_DIR/passivewubi"

cat > "$DESKTOP_DIR/passivewubi.desktop" <<EOF
[Desktop Entry]
Type=Application
Name=肌忆五笔
Comment=PassiveWubi 五笔置顶学习键盘
Exec=$APP_DIR/PassiveWubi.Linux
Icon=$ICON_DIR/passivewubi.svg
Terminal=false
Categories=Education;Utility;
EOF

chmod 644 "$DESKTOP_DIR/passivewubi.desktop"
echo "Installed PassiveWubi to $APP_DIR"
echo "Run with: passivewubi"
