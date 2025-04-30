using GoogleSpreadSheetLoader.OneButton;
using GoogleSpreadSheetLoader.Setting;
using UnityEditor;
using UnityEngine;
using static GoogleSpreadSheetLoader.Download.GSSL_Download;
// ReSharper disable CheckNamespace
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable ConvertToConstant.Local

namespace GoogleSpreadSheetLoader.Simple
{
    public enum eSimpleViewState
    {
        None,
        Prepare,
        DownloadingSpreadSheet,
        DownloadingSheet,
        GenerateSheetData,
        GenerateTableScript,
        GenerateTableData,
        GenerateTableLinker,
        Done,
    }
    
    internal class SimpleView
    {
        private static eSimpleViewState _currState;
        private static string _progressValue; // ex) (0/0)
        private static string _progressText; // ex) 스프레드 시트 다운 진행중 (0/0)
        
        private Vector2 _scrollPos = new(0, 0);

        internal void DrawSimpleView()
        {
            DrawSpreadSheetList();

            DrawButton();
        }

        internal static void SetProgressState(eSimpleViewState state, string progressValue ="")
        {
            _currState = state;

            _progressValue = progressValue;

            string stepValue = $"({(int)_currState}/{(int)eSimpleViewState.Done})";
            _progressText = _currState switch
            {
                eSimpleViewState.None => "",
                eSimpleViewState.Prepare => "준비 중",
                eSimpleViewState.DownloadingSpreadSheet => $"{stepValue} 스프레드 시트 다운로드 중 {progressValue}",
                eSimpleViewState.DownloadingSheet => $"{stepValue} 시트 다운로드 중 {progressValue}",
                eSimpleViewState.GenerateSheetData => $"{stepValue} 시트 데이터 생성 중",
                eSimpleViewState.GenerateTableScript => $"{stepValue} 테이블 스크립트 생성 중",
                eSimpleViewState.GenerateTableData => $"{stepValue} 테이블 데이터 생성 중",
                eSimpleViewState.GenerateTableLinker => $"{stepValue} 테이블 링커 생성 중",
                eSimpleViewState.Done => "완료",
                _ => "정의되지 않은 상태",
            };
        }

        private void DrawSpreadSheetList()
        {
            EditorGUILayout.Separator();

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Height(500));
            for (var i = 0; i < GSSL_Setting.SettingData.listSpreadSheetInfo.Count; i++)
            {
                EditorGUILayout.Separator();
                var info = GSSL_Setting.SettingData.listSpreadSheetInfo[i];

                EditorGUILayout.LabelField($"  {i + 1}. {info.spreadSheetName}", EditorStyles.boldLabel, GUILayout.Width(150));

                DrawSheetDataList(info.spreadSheetId);
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawSheetDataList(string spreadSheetId)
        {
            var list = GSSL_DownloadedSheet.GetSheetData(spreadSheetId);

            if (list.Count == 0)
            {
                EditorGUILayout.LabelField("     다운로드되어 있는 시트 없음");
                return;
            }

            foreach (var sheetData in list)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"     * {sheetData.title}", GUILayout.Width(180));
                EditorGUILayout.LabelField($"({sheetData.tableStyle.ToString()})");
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawButton()
        {
            EditorGUILayout.Separator();

            if (_currState != eSimpleViewState.None)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(_progressText, GUILayout.Width(_progressText.Length * 14));
                EditorGUILayout.EndHorizontal();
             
                if(GSSL_Setting.AdvanceMode)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("강제 초기화 (에러 났을때)", GUILayout.Width(180)))
                    {
                        _currState = eSimpleViewState.None;
                    }
                    EditorGUILayout.Space(30);
                    EditorGUILayout.EndHorizontal();
                }
                
                return;
            }
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("리스트 새로고침", GUILayout.Width(120)))
                GSSL_DownloadedSheet.Reset();
            if (GUILayout.Button("다운로드 & 변환", GUILayout.Width(200)))
                _ = GSSL_OneButton.OneButtonProcessSpreadSheet();
            EditorGUILayout.Space(30);
            EditorGUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

            if (GSSL_Setting.AdvanceMode)
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("(고급) 시트 다운", GUILayout.Width(160)))
                    _ = DownloadSpreadSheetOnly();
                EditorGUILayout.Space(30);
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}