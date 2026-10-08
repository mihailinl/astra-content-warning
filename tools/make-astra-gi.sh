#!/bin/bash
# Lay out astra-gi.zip LOCALLY exactly as GAME-INTEGRATION-MANIFEST.md §3/§5 describe, from a local
# foundation build and a local BepInEx distribution — no CI needed to test the layout.
#
# Usage: tools/make-astra-gi.sh
#   FOUNDATION_DIR=<dir>   the foundation's Mono install folder, i.e. the content of
#                          BepInEx/plugins/Astra/ (default: ../astra-bepinex/artifacts/foundation-mono
#                          — build it there first with `tools/pack.sh`, from this repo's sibling checkout)
#   BEPINEX_DIST=<dir>     an unpacked BepInEx_win_x64_5.4.23.5.zip (default: ./bepinex-dist)
#   CONFIG=<Debug|Release> this repo's own build configuration to ship (default: Release)
#
# .github/workflows/release.yml does the same layout in CI, from a BepInEx zip it downloads and
# verifies by digest and a foundation release asset (still a TODO placeholder there — the
# foundation has no GitHub release yet).
set -euo pipefail
cd "$(dirname "$0")/.."

ASSEMBLY_NAME=AstraContentWarning
PLUGIN_FOLDER=AstraContentWarning
TFM=netstandard2.1
CONFIG=${CONFIG:-Release}

FOUNDATION_DIR=${FOUNDATION_DIR:-../astra-bepinex/artifacts/foundation-mono}
BEPINEX_DIST=${BEPINEX_DIST:-./bepinex-dist}

for need in "$FOUNDATION_DIR/BepInEx/plugins/Astra/Astra.Unity.dll" "$BEPINEX_DIST/BepInEx/core/BepInEx.dll" "bin/$CONFIG/$TFM/$ASSEMBLY_NAME.dll"; do
  [ -f "$need" ] || { echo "make-astra-gi.sh: missing '$need' — build the foundation (tools/pack.sh in astra-bepinex), unpack BepInEx into BEPINEX_DIST, and 'dotnet build -c $CONFIG' this repo first" >&2; exit 1; }
done

OUT=$(mktemp -d)
trap 'rm -rf "$OUT"' EXIT

mkdir -p "$OUT/BepInEx/core" "$OUT/BepInEx/plugins/$PLUGIN_FOLDER" "$OUT/licenses"

# BepInEx 5's core (preloader, Harmony, …) — never its config: BepInEx writes BepInEx.cfg itself
# on first run, with its own real defaults.
cp -r "$BEPINEX_DIST/BepInEx/core/." "$OUT/BepInEx/core/"

# The foundation — one copy; every integration's zip bundles it (manifest §5).
cp -r "$FOUNDATION_DIR/BepInEx/plugins/Astra" "$OUT/BepInEx/plugins/Astra"

# This integration's own build.
cp "bin/$CONFIG/$TFM/$ASSEMBLY_NAME.dll" "$OUT/BepInEx/plugins/$PLUGIN_FOLDER/"
cp "bin/$CONFIG/$TFM/astra-item.json" "$OUT/BepInEx/plugins/$PLUGIN_FOLDER/"

# Beside the exe: Doorstop's proxy (verified by the digest astra-gi.toml pins) and our own
# doorstop_config.ini (shipped with enabled = false — see that file's own comment).
cp "$BEPINEX_DIST/winhttp.dll" "$OUT/winhttp.dll"
cp doorstop_config.ini "$OUT/doorstop_config.ini"

# Licences for everything bundled (manifest §2): this integration's own, BepInEx's LGPL-2.1 (with
# a link to its source), and the foundation's MIT. Not named by any [[files]]/[[game_files]] row,
# so Astra's installer ignores them — they ship in the zip for compliance, and
# astra-gi.toml's [integration].license is what Astra shows on the consent sheet.
cp LICENSE "$OUT/licenses/astra-content-warning-LICENSE.txt"
cp licenses/BepInEx-LICENSE.txt "$OUT/licenses/BepInEx-LICENSE.txt"
cp licenses/astra-foundation-LICENSE.txt "$OUT/licenses/astra-foundation-LICENSE.txt"

cp astra-gi.toml "$OUT/astra-gi.toml"

rm -f astra-gi.zip
( cd "$OUT" && zip -qr - . ) > astra-gi.zip
echo "wrote $(pwd)/astra-gi.zip:"
unzip -l astra-gi.zip
