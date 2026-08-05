# Google SpreadSheet Loader

Unity 에디터 확장 도구로, Google SpreadSheet에서 데이터를 다운로드하고 Unity에서 사용할 수 있는 형태로 자동 변환하는 도구입니다.

📚 **문서:** [cykim.gitbook.io/googlespreadsheetloader](https://cykim.gitbook.io/googlespreadsheetloader/)

## 주요 기능

- 🔐 **서비스 계정 인증**: Google 서비스 계정(JSON 키)으로 OAuth2 토큰을 발급받아 인증하므로, 문서를 "링크가 있는 모든 사용자"로 공개할 필요 없이 **비공개 스프레드시트**도 다운로드
- 📥 **Google SpreadSheet 다운로드**: Google Sheets API v4를 사용하여 스프레드시트 데이터를 자동으로 다운로드
- 🔄 **자동 코드 생성**: 다운로드한 데이터를 기반으로 C# 클래스 및 ScriptableObject 자동 생성
- 📊 **테이블 데이터 관리**: 스프레드시트를 Unity의 ScriptableObject로 변환하여 게임 데이터로 활용
- 🌐 **다국어 지원**: Localization 시트를 JSON 형식으로 변환하여 다국어 시스템 구축
- 🔢 **Enum 자동 생성**: 스프레드시트 데이터를 기반으로 Enum 타입 자동 생성
- 🤖 **Agent 동기화**: `.cursor/gssl-pending.json`과 `Tools/GSSL/Sync Pending Sheets`로 시트 단위 선택 동기화

## 설치 방법

1. 이 저장소를 클론하거나 Unity 패키지 파일(`GoogleSpreadSheetLoader.unitypackage`)을 다운로드합니다.
2. Unity 프로젝트에 패키지를 임포트합니다.
3. Unity 에디터에서 `Tools > GSSL > Open Window` 메뉴를 열어 설정을 시작합니다.

## 사용 방법

### 1. 서비스 계정 준비 (Google Cloud Console)

1. [Google Cloud Console](https://console.cloud.google.com/)에서 프로젝트를 생성하거나 선택합니다.
2. **Google Sheets API**를 활성화합니다.
3. `IAM 및 관리자 > 서비스 계정`에서 서비스 계정을 생성합니다.
4. 생성한 서비스 계정에서 **키 추가 > 새 키 만들기 > JSON**을 선택하여 JSON 키 파일을 내려받습니다.
5. 내려받은 JSON 파일에 있는 서비스 계정 이메일(`client_email`)을 확인합니다.
6. 다운로드할 각 스프레드시트를 이 서비스 계정 이메일과 **공유**합니다(뷰어 이상 권한).
   - 이 방식은 API 키와 달리 문서를 "링크가 있는 모든 사용자"로 공개할 필요가 없습니다.

### 2. Unity 에디터 설정

1. Unity 에디터에서 `Tools > GSSL > Open Window` 메뉴를 선택합니다.
2. `설정 편집`을 눌러 편집 모드로 진입합니다.
3. **서비스 계정 JSON 경로**에 내려받은 JSON 파일의 절대 경로를 지정합니다(`찾아보기` 버튼 사용).
   - 경로가 유효하면 서비스 계정 이메일이 표시됩니다. 아직 공유하지 않은 스프레드시트가 있다면 이 이메일과 공유하세요.
4. 다운로드할 스프레드시트 정보를 추가합니다.
   - 스프레드시트 이름과 ID를 입력합니다.
   - 스프레드시트 ID는 URL에서 확인할 수 있습니다: `https://docs.google.com/spreadsheets/d/{SPREADSHEET_ID}/edit`
5. `적용`을 눌러 설정을 저장합니다.

> ⚠️ 서비스 계정 JSON 키는 민감 정보입니다. 저장소에 커밋되지 않도록 프로젝트 밖의 안전한 경로에 두는 것을 권장합니다.

### 3. 시트 필터링 설정

- **시트 타겟 기준**: 포함 또는 제외 옵션 선택
- **시트 타겟 문자열**: 필터링할 문자열 지정 (기본값: `#`)
  - 포함 모드: 지정한 문자열을 포함하는 시트만 다운로드
  - 제외 모드: 지정한 문자열을 포함하는 시트는 제외

### 4. 데이터 다운로드 및 생성

1. 에디터 윈도우에서 다운로드 버튼을 클릭합니다.
2. 스프레드시트 데이터가 자동으로 다운로드됩니다.
3. 다운로드된 데이터를 기반으로 다음 항목들이 자동 생성됩니다:
   - **Enum**: `EnumDef` 타입으로 지정된 시트에서 Enum 클래스 생성
   - **TableData**: 테이블 데이터 클래스 및 ScriptableObject 생성
   - **Localization**: `Localization` 타입으로 지정된 시트에서 다국어 JSON 파일 생성

### 5. Agent 동기화 (선택)

에디터를 연 채로 AI/에이전트가 시트만 골라 갱신할 때 사용합니다.

**전제**

- Unity Editor가 해당 프로젝트를 연 상태
- Unity Skills 서버 실행 (`Window > UnitySkills > Start Server`)
- MCP Google Sheets 도구로 스프레드시트 읽기/쓰기 가능
- `SettingData`에 서비스 계정 JSON 절대 경로 설정, 대상 시트는 서비스 계정과 공유됨

**흐름**

1. `Assets/GoogleSpreadSheetLoader/Generated/Cache/cache_index.json` 및 `Generated/Cache/<Sheet>.txt`로 스키마만 **읽기**
2. Google Sheets에서 값·서식 수정
3. `.cursor/gssl-pending.json` 작성 예:

```json
{
  "mode": "update",
  "sheets": ["MyTableSheet"]
}
```

4. Unity 메뉴 `Tools/GSSL/Sync Pending Sheets` 실행
5. `.cursor/gssl-result.json`의 `status`가 `success`인지 확인
6. 생성물 diff만 커밋 대상으로 삼음

**금지:** `Generated/**`, `Assets/Resources/Localize_*.json`을 손으로 고쳐 데이터 반영하기. 시트 수정 직후 `mode: "regenerate"`만으로 최신화를 대체하지 않기.

상세 절차: `.cursor/skills/project-workflows/gssl-agent-workflow/SKILL.md`

## 스프레드시트 형식

### 테이블 데이터 형식 (Common)

첫 번째 행은 헤더(스키마)로 사용되며, 다음 형식을 따릅니다:

```
변수명-타입
```

예시:
```
ID-int
Name-string
HP-float
Type-ItemType
```

지원하는 타입:
- `int`, `float`, `bool`, `long`, `double`, `string`
- 사용자 정의 Enum 타입

헤더에 `-`가 없으면 GSSL 코드젠 필드로 쓰이지 않습니다. 코멘트/메모 열(`#` 또는 빈 헤더)에 적합합니다.

### 권장 시트 서식

| 역할 | 색 | 비고 |
|------|----|------|
| Common / Localization **스키마 행**(row 1) | 하늘색 `#C9DAF8` | 데이터 컬럼만 |
| 코멘트/메모 열 | 회색 `#D9D9D9` (변형 `#999999`) | 런타임 필드 아님 |
| Localization **테스트 행**(row 2) | 글자색 `#999999` | 삭제하지 말 것 |
| EnumType | 시트별 기존 패턴 | Common 하늘색 규칙을 강제하지 않음 |

서식·MCP 색상 값·체크리스트: `.cursor/skills/project-workflows/gssl-sheet-writing/SKILL.md`

### Enum 생성 형식

Enum을 생성하려면 시트 이름에 `EnumDef` 문자열이 포함되어야 합니다.

- 헤더에 Enum 이름을 지정합니다.
- `-`가 포함된 헤더는 인덱스로 사용됩니다 (예: `ItemType-0`, `ItemType-1`)
- 각 행의 값이 Enum 항목으로 추가됩니다.

### Localization 형식

다국어 데이터를 생성하려면 시트 **이름**에 Setting의 localization 타입 문자열(기본값 `Localization`)이 포함되어야 합니다. 시트명은 프로젝트마다 다를 수 있습니다 (예: `UIString_Localization`).

**권장 열**

- `id-string` (또는 `ID-string`): JSON `Key`
- `{Language}-string`: 언어별 값. 열 이름마다 `Assets/Resources/Localize_{열이름}.json` 생성  
  (예: `Korean-string`, `English-string` → `Localize_Korean.json`, `Localize_English.json`)
- `#` 또는 빈 헤더: 메모/카테고리 (선택)

**권장 행 구조**

| 행 | 역할 |
|----|------|
| Row 1 | 스키마 (`변수명-타입`), 하늘색 `#C9DAF8` |
| Row 2 | 테스트 행 (자동번역·파이프라인용, 권장). 글자색 `#999999`. **삭제·덮어쓰기 금지** |
| Row 3+ | 실제 로컬라이즈 키. 새 키는 테스트 행 **아래**에 추가 |

- 가변 문구는 시트에 `{0}` 같은 플레이스홀더를 둡니다.
- 저장 형식은 `{ "Key": ..., "Value": ... }` 항목의 **배열**입니다.
- GSSL은 row 1 이후 모든 행(테스트 행 포함)을 JSON에 넣습니다.

**생성물·런타임**

- JSON은 GSSL이 생성합니다. **직접 편집하지 마세요.** 키·문구 변경은 시트 → Sync 경로만 사용합니다.
- 패키지에 포함된 `LocalizeTable` / `GetLocalizeText`는 **샘플 API**입니다. 프로젝트 전용 로거·언어 유틸과의 연동은 소비 프로젝트에서 처리합니다.

## 생성되는 파일 구조

```
Assets/
├── GoogleSpreadSheetLoader/
│   └── Generated/
│       ├── Cache/                 # 다운로드 캐시 (읽기 전용으로 취급)
│       ├── Script/
│       │   ├── Enum/              # 생성된 Enum 클래스
│       │   ├── TableScript/       # 생성된 테이블 클래스
│       │   └── DataScript/        # 생성된 데이터 클래스
│       └── SerializeObject/
│           └── TableData/         # 생성된 ScriptableObject 에셋
└── Resources/
    └── Localize_*.json            # 다국어 JSON 파일
```

## 사용 예시

### 생성된 테이블 데이터 사용

```csharp
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public ItemTable itemTable; // Inspector에서 할당
    
    void Start()
    {
        foreach (var item in itemTable.dataList)
        {
            Debug.Log($"Item: {item.Name}, HP: {item.HP}");
        }
    }
}
```

### 다국어 시스템 사용 (샘플)

```csharp
using UnityEngine;

public class LocalizationExample : MonoBehaviour
{
    void Start()
    {
        LocalizeTable.Initialize(SystemLanguage.Korean);
        string text = "ITEM_NAME".GetLocalizeText();
        Debug.Log(text);
    }
}
```

언어 전환·UI 바인딩·프로젝트 유틸 연동은 각 게임/앱 프로젝트의 가이드를 따르세요.

## 메뉴 요약

| 메뉴 | 용도 |
|------|------|
| `Tools/GSSL/Open Window` | 설정·다운로드 UI |
| `Tools/GSSL/Sync Pending Sheets` | Agent pending (`mode: update`) 동기화 |
| `Tools/GSSL/Regenerate Pending Sheets` | 캐시 기반 재생성 (`mode: regenerate`) |

## 요구사항

- Unity 6000.2 이상 (`Awaitable` API 사용)
- Google Cloud 서비스 계정 JSON 키 (Google Sheets API 활성화 필요)
- Newtonsoft.Json (`com.unity.nuget.newtonsoft-json`, Unity Package Manager를 통해 설치)
