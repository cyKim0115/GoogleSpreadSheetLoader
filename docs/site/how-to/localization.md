# Localization 시트

다국어 문자열을 JSON으로 뽑는 방법입니다. 시트 **이름**에 Setting의 localization 타입 문자열(기본 `Localization`)이 들어가야 합니다. 실제 이름은 프로젝트마다 다를 수 있습니다 (예: `UIString_Localization`).

## 권장 열

| 열 | 역할 |
|----|------|
| `id-string` / `ID-string` | JSON `Key` |
| `{Language}-string` | 언어별 값 → `Assets/Resources/Localize_{Language}.json` |
| `#` 또는 빈 헤더 | 메모/카테고리 (선택) |

예: `Korean-string`, `English-string` → `Localize_Korean.json`, `Localize_English.json`.

## 권장 행

| 행 | 역할 | 서식 |
|----|------|------|
| Row 1 | 스키마 | 배경 `#C9DAF8` |
| Row 2 | 테스트 행 (자동번역·파이프라인용, **권장**) | 글자색 `#999999`, **삭제 금지** |
| Row 3+ | 실제 키 | 일반 데이터. 새 키는 row2 **아래** |

- 가변 문구는 `{0}` 같은 플레이스홀더를 시트에 둡니다.
- JSON은 `{ "Key", "Value" }` 객체 **배열**입니다.
- GSSL은 row1 이후 **모든 행**(테스트 행 포함)을 JSON에 넣습니다.

{% hint style="warning" %}
`Localize_*.json`을 직접 편집하지 마세요. 키·문구 변경은 시트 → Sync 경로만 사용합니다.
{% endhint %}

## 런타임

패키지의 `LocalizeTable` / `GetLocalizeText`는 **샘플 API**입니다.  
프로젝트 전용 로거·언어 유틸·UI 바인딩은 소비 프로젝트에서 연결하세요. → [런타임에서 쓰기](runtime-usage.md)
