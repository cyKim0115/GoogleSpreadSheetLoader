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
    public class SimpleView
    {
        internal static string spreadSheetDownloadMessage = "";
        internal static string sheetDownloadMessage = "";
        internal static eDownloadState spreadSheetDownloadState = eDownloadState.None;
        internal static eDownloadState sheetDownloadState = eDownloadState.None;

        private Vector2 _sheetDownloadScrollPos = new(0, 0);

        public void DrawSimpleView()
        {
            DrawSpreadSheetList();

            DrawButton();
        }

        private void DrawSpreadSheetList()
        {
            EditorGUILayout.Separator();

            for (var i = 0; i < GSSL_Setting.SettingData.listSpreadSheetInfo.Count; i++)
            {
                EditorGUILayout.Separator();
                var info = GSSL_Setting.SettingData.listSpreadSheetInfo[i];

                // EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"  {i + 1}. {info.spreadSheetName}", EditorStyles.boldLabel, GUILayout.Width(150));
                // EditorGUILayout.LabelField(info.spreadSheetId);
                // EditorGUILayout.EndHorizontal();

                DrawSheetDataList(info.spreadSheetId);
            }
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

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("리스트 새로고침", GUILayout.Width(120)))
                GSSL_DownloadedSheet.Reset();
            if (GUILayout.Button("다운로드 & 변환", GUILayout.Width(200)))
                _ = GSSL_OneButton.OneButtonProcess(GetAllSpreadSheet());
            EditorGUILayout.Space(30);
            EditorGUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

            if (GSSL_Setting.AdvanceMode)
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("(고급) 시트 다운", GUILayout.Width(160)))
                    _ = DownloadSpreadSheet();
                EditorGUILayout.Space(30);
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}