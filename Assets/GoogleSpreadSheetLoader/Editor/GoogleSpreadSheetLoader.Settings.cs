using UnityEditor;
using UnityEngine;

namespace GoogleSpreadSheetLoader
{
    public partial class GoogleSpreadSheetLoaderWindow
    {
        private readonly string _settingDataPath = $"Assets/GoogleSpreadSheetLoader/SettingData.asset";
        private SettingData _settingData;
        private bool _apiKeyEditToggle;
        private bool _sheetInfoEditToggle;
        private bool _sheetTargetEditToggle;
        private string _spreadSheetOpenUrl = "https://docs.google.com/spreadsheets/d/{0}/edit?key={1}";
        
        private void DrawSettingView()
        {
            SettingView_DrawApiKey();
            
            SettingView_DrawSpreadSheetsInfos();

            SettingView_DrawAddDelBtns();

            SettingView_SheetSettings();
            
            SettingView_SaveSettingData();
        }
        
        private bool Setting_CheckAndCreate()
        {
            if (_settingData == null)
            {
                // 없으면 파일 생성
                if (!AssetDatabase.AssetPathExists(_settingDataPath))
                {
                    SettingData obj = ScriptableObject.CreateInstance<SettingData>();
                    obj = new SettingData();

                    AssetDatabase.CreateAsset(obj, _settingDataPath);
                }

                _settingData =
                    AssetDatabase.LoadAssetAtPath<SettingData>(_settingDataPath);
            }

            return _settingData != null;
        }

        private void SettingView_DrawApiKey()
        {
            EditorGUILayout.Separator();

            _apiKeyEditToggle = EditorGUILayout.ToggleLeft("  API 키",_apiKeyEditToggle, EditorStyles.whiteLargeLabel);
            
            if (_apiKeyEditToggle)
                _settingData.apiKey = EditorGUILayout.TextField(_settingData.apiKey);
            else
                EditorGUILayout.LabelField(_settingData.apiKey);
        }

        private void SettingView_DrawSpreadSheetsInfos()
        {
            EditorGUILayout.Separator();

            _sheetInfoEditToggle = EditorGUILayout.ToggleLeft("  스프레드 시트 데이터 입력",_sheetInfoEditToggle, EditorStyles.whiteLargeLabel);

            int sheetNameWidth = 100;
            for (int i = 0; i < _settingData.listSpreadSheetInfo.Count; i++)
            {
                SpreadSheetInfo _info = _settingData.listSpreadSheetInfo[i];
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{i + 1}. ",GUILayout.Width(20));

                if (_sheetInfoEditToggle)
                {
                    _info.spreadSheetName = EditorGUILayout.TextField(_info.spreadSheetName,GUILayout.Width(sheetNameWidth));
                    _info.spreadSheetId = EditorGUILayout.TextField(_info.spreadSheetId);
                }
                else
                {
                    EditorGUILayout.LabelField(_info.spreadSheetName,GUILayout.Width(sheetNameWidth));
                    EditorGUILayout.LabelField(_info.spreadSheetId);
                    GUILayout.FlexibleSpace();
                    // if (GUILayout.Button("연결"))
                    // {
                    //     string url = string.Format(_spreadSheetOpenUrl, _info.spreadSheetId, _settingData.apiKey);
                    //     Application.OpenURL(url);
                    // }
                }
                
                EditorGUILayout.EndHorizontal();
            }
        }

        private void SettingView_DrawAddDelBtns()
        {
            if (!_sheetInfoEditToggle) return;

            EditorGUILayout.Separator();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (_settingData.listSpreadSheetInfo.Count > 0 && GUILayout.Button("-", GUILayout.Width(100)))
            {
                _settingData.listSpreadSheetInfo.RemoveAt(_settingData.listSpreadSheetInfo.Count - 1);
            }

            if (GUILayout.Button("+", GUILayout.Width(100)))
            {
                _settingData.listSpreadSheetInfo.Add(new SpreadSheetInfo()
                    { spreadSheetName = "Name Here", spreadSheetId = "Id Here" });
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void SettingView_SheetSettings()
        {
            EditorGUILayout.Separator();

            _sheetTargetEditToggle = EditorGUILayout.ToggleLeft("  시트 설정",_sheetTargetEditToggle, EditorStyles.whiteLargeLabel);
            if (_sheetTargetEditToggle)
            {
                // 무시
                EditorGUILayout.BeginHorizontal();
                GUILayout.Box(" 시트의 제목에서",GUILayout.Width(100));
                EditorGUILayout.TextField(_settingData.sheetTargetStr,GUILayout.Width(50));
                // EditorGUILayout.EndHorizontal();
                // EditorGUILayout.BeginHorizontal();
                GUILayout.Box("문구가 포함되어 있을 경우, 대상 시트에서 ",GUILayout.Width(250));
                _settingData.sheetTarget = (SettingData.eSheetTargetStandard)EditorGUILayout.EnumPopup(_settingData.sheetTarget,GUILayout.Width(60));
                EditorGUILayout.EndHorizontal();
                
                // 이넘 타입
                EditorGUILayout.BeginHorizontal();
                GUILayout.Box(" 다음의 문구가 있다면 enum타입을 정의하는 테이블로 분류합니다.");
                EditorGUILayout.TextField(_settingData.sheet_enumTypeStr,GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();
                
                // 로컬라이제이션 타입
                EditorGUILayout.BeginHorizontal();
                GUILayout.Box(" 다음의 같은 문구가 있다면 Localization테이블로 분류합니다.");
                EditorGUILayout.TextField(_settingData.sheet_localizationTypeStr,GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                // 무시
                EditorGUILayout.LabelField($" 시트의 제목에서 \"{_settingData.sheetTargetStr}\" 문구가 포함되어 있을경우, 대상 시트에서 \"{_settingData.sheetTarget.ToString()}\"");
                
                // 이넘 타입
                EditorGUILayout.LabelField($" 다음의 문구가 있다면 enum타입을 정의하는 테이블로 분류합니다. \"{_settingData.sheet_enumTypeStr}\"");
                
                // 로컬라이제이션 타입
                EditorGUILayout.LabelField($" 다음의 같은 문구가 있다면 Localization테이블로 분류합니다. \"{_settingData.sheet_localizationTypeStr}\"");
            }
        }
        
        private void SettingView_SaveSettingData()
        {
            GUILayout.FlexibleSpace();
            
            EditorGUILayout.Separator();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("세팅 데이터 저장", GUILayout.Width(200)))
            {
                EditorUtility.SetDirty(_settingData);
                AssetDatabase.SaveAssets();
            }
            
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(30);
        }
    }
}