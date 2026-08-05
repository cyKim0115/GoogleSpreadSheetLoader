# 설치

Unity 프로젝트에 Google SpreadSheet Loader(GSSL)를 넣는 방법입니다.

{% stepper %}
{% step %}
## 패키지 가져오기

다음 중 하나를 선택합니다.

- GitHub에서 [저장소](https://github.com/cyKim0115/GoogleSpreadSheetLoader)를 클론
- 배포된 루트 `GoogleSpreadSheetLoader.unitypackage`를 다운로드 (GitHub 레포/Release)
{% endstep %}

{% step %}
## Unity에 임포트

클론한 경우 `Assets/GoogleSpreadSheetLoader/`가 프로젝트에 포함되도록 복사하거나 서브모듈/UPM 방식으로 붙입니다.  
유니티패키지인 경우 **Assets > Import Package > Custom Package**로 임포트합니다.

패키지 파일을 **만드는** 쪽(배포)은 [`.unitypackage 배포 Export`](../how-to/export-package.md)를 참고하세요.
{% endstep %}

{% step %}
## 의존성 확인

Package Manager에서 Newtonsoft.Json이 있는지 확인합니다.

- 패키지 ID: `com.unity.nuget.newtonsoft-json`
{% endstep %}

{% step %}
## 창 열기

메뉴 **Tools > GSSL > Open Window**로 설정 창을 엽니다.  
정상이면 다음 단계 [서비스 계정 설정](service-account.md)으로 진행합니다.
{% endstep %}
{% endstepper %}

## Unity 버전

GSSL은 Unity **6000.2** 이상의 `Awaitable` API를 사용합니다. 그 미만이면 컴파일·다운로드 경로가 맞지 않을 수 있습니다.
