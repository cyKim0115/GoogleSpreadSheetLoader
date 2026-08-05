#!/usr/bin/env bash
# Unity batchmode로 루트 GoogleSpreadSheetLoader.unitypackage 생성 (Linux/macOS / CI).
# 프로젝트를 Editor로 연 상태에서는 실행하지 마세요.
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$REPO_ROOT"

COMMIT=0
PUSH=0
UNITY_EXE="${UNITY_EDITOR:-}"

while [[ $# -gt 0 ]]; do
  case "$1" in
    --commit) COMMIT=1; shift ;;
    --push) PUSH=1; shift ;;
    --unity)
      UNITY_EXE="$2"
      shift 2
      ;;
    *)
      echo "Unknown arg: $1" >&2
      exit 2
      ;;
  esac
done

VERSION="$(sed -n 's/^m_EditorVersion: //p' ProjectSettings/ProjectVersion.txt | head -n1 | tr -d '\r')"
if [[ -z "$VERSION" ]]; then
  echo "m_EditorVersion not found" >&2
  exit 1
fi

if [[ -z "$UNITY_EXE" ]]; then
  for c in \
    "/Applications/Unity/Hub/Editor/${VERSION}/Unity.app/Contents/MacOS/Unity" \
    "${HOME}/Unity/Hub/Editor/${VERSION}/Editor/Unity" \
    "/opt/unity/Editor/Unity"
  do
    if [[ -x "$c" ]]; then
      UNITY_EXE="$c"
      break
    fi
  done
fi

if [[ -z "${UNITY_EXE}" || ! -x "${UNITY_EXE}" ]]; then
  echo "Unity ${VERSION} not found. Set UNITY_EDITOR or pass --unity /path/to/Unity" >&2
  exit 1
fi

mkdir -p Logs
LOG_FILE="Logs/gssl-export-unitypackage.log"
RESULT_FILE="Logs/gssl-export-unitypackage-result.json"
PACKAGE="GoogleSpreadSheetLoader.unitypackage"
METHOD="GoogleSpreadSheetLoader.Export.GSSL_ExportUnityPackage.Export"

rm -f "$RESULT_FILE"

echo "Unity:  $UNITY_EXE"
echo "Version: $VERSION"
echo "Method: $METHOD"

set +e
"$UNITY_EXE" -batchmode -nographics -quit \
  -projectPath "$REPO_ROOT" \
  -executeMethod "$METHOD" \
  -logFile "$LOG_FILE"
EXIT_CODE=$?
set -e

if [[ "$EXIT_CODE" -ne 0 ]]; then
  echo "Unity exit code: $EXIT_CODE" >&2
  tail -n 80 "$LOG_FILE" || true
  exit "$EXIT_CODE"
fi

if [[ ! -f "$PACKAGE" ]]; then
  echo "Package missing: $PACKAGE" >&2
  tail -n 80 "$LOG_FILE" || true
  exit 1
fi

echo "OK: $PACKAGE ($(wc -c < "$PACKAGE") bytes)"
[[ -f "$RESULT_FILE" ]] && cat "$RESULT_FILE"

if [[ "$PUSH" -eq 1 && "$COMMIT" -eq 0 ]]; then
  echo "--push requires --commit" >&2
  exit 2
fi

if [[ "$COMMIT" -eq 1 ]]; then
  git add -- "$PACKAGE"
  if git diff --cached --quiet -- "$PACKAGE"; then
    echo "No package changes — skip commit."
  else
    git commit -m "리소스 - GoogleSpreadSheetLoader.unitypackage 배치 Export 갱신"
    if [[ "$PUSH" -eq 1 ]]; then
      git push origin HEAD
    fi
  fi
fi
