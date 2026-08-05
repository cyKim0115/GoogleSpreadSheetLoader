# .unitypackage 배포 Export

에디터를 **끈 상태**에서 Unity batchmode로 루트 `GoogleSpreadSheetLoader.unitypackage`를 갱신합니다.

{% hint style="warning" %}
대상 프로젝트를 Unity Editor로 연 채 batchmode를 돌리지 마세요. `Library/` 경합이 날 수 있습니다.
{% endhint %}

## 포함 / 제외

| 포함 | 제외 |
|------|------|
| `Assets/GoogleSpreadSheetLoader/**` (런타임·에디터 스크립트 등) | `Generated/` |
| | `SettingData.asset` (로컬 서비스 계정 경로) |
| | `Assets/TextMesh Pro`, 샘플 씬 등 패키지 밖 에셋 |

Newtonsoft.Json은 패키지에 넣지 않습니다. 소비 프로젝트에서 UPM으로 설치합니다.

## 로컬 (Windows)

```powershell
# 에디터 종료 후
.\scripts\export-unitypackage.ps1

# 생성 + 커밋 + 푸시
.\scripts\export-unitypackage.ps1 -Commit -Push
```

Unity 경로를 직접 지정하려면:

```powershell
.\scripts\export-unitypackage.ps1 -UnityExe "C:\Program Files\Unity\Hub\Editor\6000.4.12f1\Editor\Unity.exe"
```

또는 환경 변수 `UNITY_EDITOR`.

## 로컬 (macOS / Linux)

```bash
chmod +x scripts/export-unitypackage.sh
./scripts/export-unitypackage.sh
./scripts/export-unitypackage.sh --commit --push
```

## 메뉴 (에디터를 이미 연 경우)

**Tools > GSSL > Export Unity Package** — 같은 Export 로직. 배포 자동화의 기본 경로는 batchmode 스크립트입니다.

## CI (GitHub Actions)

워크플로: `.github/workflows/export-unitypackage.yml`

1. 리포지토리 Secrets에 game-ci용 `UNITY_LICENSE`, `UNITY_EMAIL`, `UNITY_PASSWORD` 설정  
   ([activation](https://game.ci/docs/github/activation))
2. Actions → **Export Unity Package** → Run workflow  
   - `commit_and_push` 체크 시 패키지를 커밋·푸시
3. Release 발행 시 아티팩트로 첨부 시도

로그·결과: `Logs/gssl-export-unitypackage.log`, `Logs/gssl-export-unitypackage-result.json` (`Logs/`는 gitignore)

## executeMethod

```text
GoogleSpreadSheetLoader.Export.GSSL_ExportUnityPackage.Export
```
