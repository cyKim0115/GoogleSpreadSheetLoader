# 서비스 계정 설정

GSSL은 Google Sheets **API 키**가 아니라 **서비스 계정 JSON**으로 OAuth2 토큰을 받아 인증합니다.  
문서를 “링크가 있는 모든 사용자”로 공개할 필요 없이, 서비스 계정 이메일과 **공유**만 하면 비공개 시트도 받을 수 있습니다.

{% hint style="warning" %}
서비스 계정 JSON 키는 민감 정보입니다. 저장소에 커밋하지 말고, 가능하면 Unity 프로젝트 **밖** 경로에 두세요.
{% endhint %}

{% stepper %}
{% step %}
## Google Cloud 프로젝트

1. [Google Cloud Console](https://console.cloud.google.com/)에서 프로젝트를 만들거나 선택합니다.
2. **Google Sheets API**를 활성화합니다.
{% endstep %}

{% step %}
## 서비스 계정과 키

1. **IAM 및 관리자 > 서비스 계정**에서 계정을 생성합니다.
2. **키 추가 > 새 키 만들기 > JSON**으로 키 파일을 내려받습니다.
3. JSON 안의 `client_email`을 메모합니다.
{% endstep %}

{% step %}
## 스프레드시트 공유

다운로드할 각 스프레드시트를 `client_email`과 공유합니다 (**뷰어** 이상).
{% endstep %}

{% step %}
## Unity SettingData

1. **Tools > GSSL > Open Window**
2. **설정 편집**으로 들어갑니다.
3. **서비스 계정 JSON 경로**에 키 파일의 **절대 경로**를 지정합니다 (`찾아보기` 사용 가능).
4. 경로가 유효하면 서비스 계정 이메일이 표시됩니다.
5. 스프레드시트 **이름**과 **ID**를 추가합니다.  
   ID는 URL의 `https://docs.google.com/spreadsheets/d/{SPREADSHEET_ID}/edit` 구간입니다.
6. **적용**으로 저장합니다.
{% endstep %}
{% endstepper %}

설정 에셋은 보통 `Assets/GoogleSpreadSheetLoader/SettingData.asset`에 있으며, JSON **경로 문자열**만 들고 있습니다. 키 파일 자체는 커밋 대상이 아닙니다.
