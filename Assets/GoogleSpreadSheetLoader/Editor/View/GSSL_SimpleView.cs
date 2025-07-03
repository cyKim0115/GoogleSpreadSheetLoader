using GoogleSpreadSheetLoader.OneButton;
using GoogleSpreadSheetLoader.Setting;
using UnityEditor;
using UnityEngine;
using static GoogleSpreadSheetLoader.Download.GSSL_Download;
using static GoogleSpreadSheetLoader.GSSL_State;

namespace GoogleSpreadSheetLoader.Simple
{
    internal class SimpleView
    {
        private Vector2 _scrollPos = new(0, 0);

        internal void DrawSimpleView()
        {
            DrawSpreadSheetList();
            DrawButton();
        }

        private void DrawSpreadSheetList()
        {
            EditorGUILayout.Separator();

            var boxStyle = new GUIStyle(GUI.skin.box);
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Height(500));
            EditorGUILayout.BeginVertical(boxStyle);
            for (var i = 0; i < GSSL_Setting.SettingData.listSpreadSheetInfo.Count; i++)
            {
                EditorGUILayout.Separator();
                var info = GSSL_Setting.SettingData.listSpreadSheetInfo[i];

                EditorGUILayout.LabelField($"  {i + 1}. {info.spreadSheetName}", EditorStyles.boldLabel, GUILayout.Width(150));

                DrawSheetDataList(info.spreadSheetId);
            }
            EditorGUILayout.EndVertical();
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

            if (CurrState != eGSSL_State.None)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(ProgressText, GUILayout.Width(ProgressText.Length * 12));
                EditorGUILayout.Space(30);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(30);
             
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("강제 초기화 (에러 났을때)", GUILayout.Width(180)))
                {
                    SetProgressState(eGSSL_State.None);
                }
                EditorGUILayout.Space(30);
                EditorGUILayout.EndHorizontal();
                
                return;
            }
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
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