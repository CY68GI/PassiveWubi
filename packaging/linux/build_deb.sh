#!/usr/bin/env bash
set -euo pipefail

VERSION="${1:-2.1.2}"
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
  "$STAGE/usr/lib/passivewubi/PassiveWubiData" \
  "$STAGE/usr/bin" "$STAGE/usr/share/applications" \
  "$STAGE/usr/share/icons/hicolor/scalable/apps" \
  "$STAGE/usr/share/doc/passivewubi" "$STAGE/usr/share/man/man1" \
  "$STAGE/usr/share/lintian/overrides"

cp -a "$BUNDLE/." "$STAGE/usr/lib/passivewubi/"
chmod 755 "$STAGE/usr/lib/passivewubi/passive_wubi"

cat > "$STAGE/usr/bin/passivewubi" <<'EOF'
#!/bin/sh
exec /usr/lib/passivewubi/passive_wubi "$@"
EOF
chmod 755 "$STAGE/usr/bin/passivewubi"

cp "$ROOT/assets/icons/passive_wubi.svg" \
  "$STAGE/usr/share/icons/hicolor/scalable/apps/passivewubi.svg"
cp "$ROOT/LICENSE" "$STAGE/usr/share/doc/passivewubi/copyright"
cat > "$STAGE/usr/share/doc/passivewubi/changelog" <<EOF
passivewubi ($VERSION) unstable; urgency=medium

  * PassiveWubi Flutter desktop release.

 -- CY68GI <cy68gi@users.noreply.github.com>  Wed, 11 Nov 2026 10:00:00 +0000
EOF
gzip -9 -n "$STAGE/usr/share/doc/passivewubi/changelog"

cat > "$STAGE/usr/share/man/man1/passivewubi.1" <<'EOF'
.TH PASSIVEWUBI 1
.SH NAME
passivewubi \- 肌忆五笔桌面学习工具
.SH SYNOPSIS
.B passivewubi
.SH DESCRIPTION
启动肌忆五笔置顶字根键盘和五笔输入练习。
EOF
gzip -9 -n "$STAGE/usr/share/man/man1/passivewubi.1"

cat > "$STAGE/usr/share/lintian/overrides/passivewubi" <<'EOF'
passivewubi: embedded-library freetype [usr/lib/passivewubi/lib/libflutter_linux_gtk.so]
passivewubi: embedded-library libjpeg [usr/lib/passivewubi/lib/libflutter_linux_gtk.so]
passivewubi: shared-library-lacks-prerequisites [usr/lib/passivewubi/lib/libapp.so]
EOF

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
Depends: libc6, libgtk-3-0, libstdc++6, libx11-6, libxi6
Maintainer: CY68GI <cy68gi@users.noreply.github.com>
Homepage: https://github.com/CY68GI/PassiveWubi
Description: Chinese Wubi learning keyboard and practice tool
 肌忆五笔提供屏幕置顶字根键盘、速度测试和五笔拆字、词组练习。
EOF

mkdir -p "$ROOT/dist"
rm -f "$OUT"
find "$STAGE" -type d -exec chmod 755 {} +
find "$STAGE" -type f -exec chmod 644 {} +
chmod 755 "$STAGE/usr/lib/passivewubi/passive_wubi"
chmod 755 "$STAGE/usr/bin/passivewubi"
chmod 777 "$STAGE/usr/lib/passivewubi/PassiveWubiData"

if command -v patchelf >/dev/null 2>&1; then
  find "$STAGE/usr/lib/passivewubi" -type f -name '*.so*' \
    -exec patchelf --set-rpath '$ORIGIN' {} +
  patchelf --set-rpath '$ORIGIN/lib' "$STAGE/usr/lib/passivewubi/passive_wubi" || true
fi

if command -v strip >/dev/null 2>&1; then
  find "$STAGE/usr/lib/passivewubi" -type f -name '*.so*' \
    -exec strip --strip-unneeded {} + || true
  strip --strip-unneeded "$STAGE/usr/lib/passivewubi/passive_wubi" || true
fi

dpkg-deb --build --root-owner-group "$STAGE" "$OUT"
rm -rf "$STAGE"
echo "Built $OUT"
