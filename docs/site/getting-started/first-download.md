# 첫 다운로드

서비스 계정과 스프레드시트 ID가 준비됐다면, 에디터 창에서 한 번 전체 동기화를 돌려 생성물을 확인합니다.

{% stepper %}
{% step %}
## 시트 필터 (선택)

Open Window에서 시트 이름 필터를 조정할 수 있습니다.

| 옵션 | 의미 |
|------|------|
| 시트 타겟 기준 | **포함** 또는 **제외** |
| 시트 타겟 문자열 | 기본값 `#` — 포함 모드면 해당 문자열이 있는 시트만, 제외 모드면 그런 시트는 건너뜀 |
{% endstep %}

{% step %}
## 다운로드 실행

에디터 윈도우의 다운로드(또는 원클릭 처리) 버튼을 누릅니다.  
GSSL이 Sheets API로 데이터를 받고 캐시·코드·에셋을 생성합니다.
{% endstep %}

{% step %}
## 생성물 확인

대략 다음이 생깁니다.

| 종류 | 위치 (기본) |
|------|-------------|
| 캐시 | `Assets/GoogleSpreadSheetLoader/Generated/Cache/` |
| Enum / Table / Data 스크립트 | `.../Generated/Script/` |
| ScriptableObject | `.../Generated/SerializeObject/TableData/` |
| 다국어 JSON | `Assets/Resources/Localize_*.json` |

시트 타입에 따라 EnumDef → Enum, Common → 테이블, Localization → JSON이 나뉩니다.  
형식 상세는 [시트 형식](../concepts/sheet-formats.md)을 참고하세요.
{% endstep %}

{% step %}
## 메뉴로도 가능

| 메뉴 | 용도 |
|------|------|
| `Tools/GSSL/Open Window` | 설정·다운로드 UI |
| `Tools/GSSL/Sync Pending Sheets` | Agent pending (`mode: update`) |
| `Tools/GSSL/Regenerate Pending Sheets` | 캐시 기반 재생성 (`mode: regenerate`) |
{% endstep %}
{% endstepper %}

이후 일부 시트만 자주 고친다면 [Agent 동기화](../how-to/agent-sync.md)가 편합니다.
