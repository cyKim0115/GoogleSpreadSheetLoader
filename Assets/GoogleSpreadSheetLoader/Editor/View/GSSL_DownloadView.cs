using System.Collections.Generic;
using GoogleSpreadSheetLoader.Setting;
using UnityEditor;
using UnityEngine;
using static GoogleSpreadSheetLoader.Download.GSSL_Download;
// ReSharper disable CheckNamespace
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable ConvertToConstant.Local

namespace GoogleSpreadSheetLoader.Download
{
    public class DownloadView
    {
        internal static string spreadSheetDownloadMessage = "";
        internal static string sheetDownloadMessage = "";
        internal static eDownloadState spreadSheetDownloadState = eDownloadState.None;
        internal static eDownloadState sheetDownloadState = eDownloadState.None;
        
        private Dictionary<int, bool> _dicDownloadSpreadSheetCheck = new();

        private Dictionary<string, Dictionary<string, bool>> _dicDownloadSheetCheck = new();

        // Key : SpreadSheetId , Value : SheetNames
        private Dictionary<string, List<string>> _dicSheetNames = new();
        
        private Vector2 _sheetDownloadScrollPos = new(0, 0);

        public void DrawDownloadView()
        {
            DrawSpreadSheetList();
        }

        private void DrawSpreadSheetList()
        {
            EditorGUILayout.Separator();

            for (var i = 0; i < GSSL_Setting.SettingData.listSpreadSheetInfo.Count; i++)
            {
                var info = GSSL_Setting.SettingData.listSpreadSheetInfo[i];
                
                _dicDownloadSpreadSheetCheck.TryAdd(i, false);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{i + 1}. {info.spreadSheetName}", GUILayout.Width(150));
                EditorGUILayout.LabelField(info.spreadSheetId);
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}