#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "${ROOT_DIR}"

VERSION="${1:-}"
if [[ -z "${VERSION}" ]]; then
  VERSION="$(python3 -c 'import json; print(json.load(open("Joi.json"))["version"])')"
fi

if [[ -z "${VERSION}" ]]; then
  echo "Could not determine release version." >&2
  exit 1
fi

find_godot() {
  if [[ -n "${GODOT_PATH:-}" && -x "${GODOT_PATH}" ]]; then
    printf '%s\n' "${GODOT_PATH}"
    return 0
  fi

  if command -v godot >/dev/null 2>&1; then
    command -v godot
    return 0
  fi

  if command -v godot4 >/dev/null 2>&1; then
    command -v godot4
    return 0
  fi

  local mac_godot="/Applications/Godot_mono.app/Contents/MacOS/Godot"
  if [[ -x "${mac_godot}" ]]; then
    printf '%s\n' "${mac_godot}"
    return 0
  fi

  return 1
}

GODOT_BIN="$(find_godot || true)"
if [[ -z "${GODOT_BIN}" ]]; then
  echo "Godot executable not found. Set GODOT_PATH=/path/to/Godot and retry." >&2
  exit 1
fi

DIST_DIR="${ROOT_DIR}/dist/workshop"
STAGING_DIR="${DIST_DIR}/Joi-${VERSION}"
CONTENT_DIR="${STAGING_DIR}/content"
PCK_PATH="${CONTENT_DIR}/Joi.pck"
MOD_ID_TMP=""

if [[ -f "${ROOT_DIR}/mod_id.txt" ]]; then
  MOD_ID_TMP="$(mktemp)"
  cp "${ROOT_DIR}/mod_id.txt" "${MOD_ID_TMP}"
elif [[ -f "${STAGING_DIR}/mod_id.txt" ]]; then
  MOD_ID_TMP="$(mktemp)"
  cp "${STAGING_DIR}/mod_id.txt" "${MOD_ID_TMP}"
fi

echo "Building Joi ${VERSION}..."
dotnet build Joi.sln -c ExportRelease

DLL_PATH="${ROOT_DIR}/.godot/mono/temp/bin/ExportRelease/Joi.dll"
if [[ ! -f "${DLL_PATH}" ]]; then
  DLL_PATH="$(find "${ROOT_DIR}/.godot/mono/temp/bin" -type f -path '*/ExportRelease/*' -name Joi.dll -print | sort | tail -n 1)"
fi

if [[ -z "${DLL_PATH}" || ! -f "${DLL_PATH}" ]]; then
  echo "Joi.dll was not found after build." >&2
  exit 1
fi

rm -rf "${STAGING_DIR}"
mkdir -p "${CONTENT_DIR}"
if [[ -n "${MOD_ID_TMP}" ]]; then
  cp "${MOD_ID_TMP}" "${STAGING_DIR}/mod_id.txt"
  rm -f "${MOD_ID_TMP}"
fi

echo "Exporting Joi.pck..."
mkdir -p packages
touch packages/.gdignore
"${GODOT_BIN}" --headless --export-pack "BasicExport" "${PCK_PATH}"

cp "${DLL_PATH}" "${CONTENT_DIR}/Joi.dll"
cp Joi.json "${CONTENT_DIR}/Joi.json"
cp Joi/mod_image.png "${STAGING_DIR}/image.png"
cp workshop.json "${STAGING_DIR}/workshop.json"

python3 - "${CONTENT_DIR}/Joi.json" "${VERSION}" <<'PY'
import json
import sys
from pathlib import Path

manifest = Path(sys.argv[1])
version = sys.argv[2]
data = json.loads(manifest.read_text())
data["version"] = version
manifest.write_text(json.dumps(data, indent=2) + "\n")
PY

cat > "${STAGING_DIR}/README.md" <<'EOF'
# Steam Workshop Mod

Upload this directory with ModUploader.

- `workshop.json` configures the Steam Workshop item.
- `image.png` is the Workshop preview image.
- `content/` contains the mod files uploaded to Workshop.
EOF

echo
echo "Workshop upload folder:"
echo "  ${STAGING_DIR}"
