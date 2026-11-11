#!/usr/bin/env bash
set -euo pipefail

VERSION="${1:-2.0.0}"
ROOT="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/../.." && pwd)"
BUNDLE="$ROOT/build/linux/x64/release/bundle"
STAGE="$ROOT/dist/passivewubi-deb-$VERSION"
OUT="$ROOT/dist/PassiveWubi-Flutter-v${VERSION}-linux-amd64.deb"

if [[ ! -x "$BUNDLE/passive_wubi" ]]; then
  echo "Flutter Linux bundle not found; run flutter build linux --release first." >&2
  exit 1
fi

rm -rf "$STAGE"
mkdir -p "$STAGE/DEBIAN" "$STAGE/usr/lib/passivewubi" \
  "$STAGE/usr/bin" "$STAGE/usr/share/applications" \
  "$STAGE/usr/share/icons/hicolor/scalable/apps"

cp -a "$BUNDLE/." "$STAGE/usr/lib/passivewubi/"
chmod 755 "$STAGE/usr/lib/passivewubi/passive_wubi"

cat > "$STAGE/usr/bin/passivewubi" <<'EOF'
#!/bin/sh
exec /usr/lib/passivewubi/passive_wubi "$@"
EOF
chmod 755 "$STAGE/usr/bin/passivewubi"

cp "$ROOT/assets/icons/passive_wubi.svg" \
  "$STAGE/usr/share/icons/hicolor/scalable/apps/passivewubi.svg"

cat > "$STAGE/usr/share/applications/passivewubi.desktop" <<'EOF'
[Desktop Entry]
Type=Application
Name=肌忆五笔
Comment=PassiveWubi 五笔置顶学习键盘
Exec=/usr/bin/passivewubi
Icon=passivewubi
Terminal=false
Categories=Education;Utility;
EOF

INSTALLED_SIZE="$(du -sk "$STAGE" | awk '{print $1}')"

cat > "$STAGE/DEBIAN/control" <<EOF
Package: passivewubi
Version: $VERSION
Section: education
Priority: optional
Architecture: amd64
Installed-Size: $INSTALLED_SIZE
Depends: libgtk-3-0, libx11-6, libxi6, libstdc++6
Maintainer: CY68GI <cy68gi@users.noreply.github.com>
Description: PassiveWubi
 肌忆五笔：潜移默化学习五笔的置顶字根键盘和练习工具。
EOF

mkdir -p "$ROOT/dist"
rm -f "$OUT"
dpkg-deb --build --root-owner-group "$STAGE" "$OUT"
rm -rf "$STAGE"
echo "Built $OUT"
