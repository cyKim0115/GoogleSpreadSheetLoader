using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoogleSpreadSheetLoader.Setting;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using static GoogleSpreadSheetLoader.GSSL_State;

namespace GoogleSpreadSheetLoader.Download
{
    internal static partial class GSSL_Download
    {
        public static async Awaitable DownloadSheet(List<RequestInfo> listDownloadInfo)
        {
            // 다운로드
            try
            {
                foreach (var info in listDownloadInfo)
                {
                    info.SendAndGetAsyncOperation();
                }

                var totalCount = listDownloadInfo.Count;
                do
                {
                    string progressString = $"({listDownloadInfo.Count(x => x.IsDone)}/{totalCount})";
                    SetProgressState(eGSSL_State.DownloadingSheet, progressString);
                    EditorWindow.focusedWindow?.Repaint();
                    await Task.Delay(100);
                } while (listDownloadInfo.Any(x => !x.IsDone));
            }
            finally
            {
                string progressString = $"(Done)";
                SetProgressState(eGSSL_State.DownloadingSheet, progressString);
                EditorWindow.focusedWindow?.Repaint();
                await Task.Delay(500);
            }

            var sheetDataAssetPath = GSSL_Path.GetPath(ePath.SheetData);

            // 다운로드 받은 데이터 정리
            foreach (var info in listDownloadInfo)
            {
                SheetData sheetData = ScriptableObject.CreateInstance<SheetData>();
                sheetData.spreadSheetId = info.SpreadSheetId;
                sheetData.title = info.SheetName;

                if (sheetData.title.Contains(GSSL_Setting.SettingData.sheet_enumTypeStr))
                    sheetData.tableStyle = SheetData.eTableStyle.EnumType;
                else if (sheetData.title.Contains(GSSL_Setting.SettingData.sheet_localizationTypeStr))
                    sheetData.tableStyle = SheetData.eTableStyle.Localization;
                else
                    sheetData.tableStyle = SheetData.eTableStyle.Common;

                JObject jObj = JObject.Parse(info.DownloadText);

                if (!jObj.TryGetValue("values", out var values))
                {
                    Debug.LogError($"변환 실패 - {info.SheetName}\n"
                                   + $"{info.URL}\n"
                                   + $"{info.DownloadText}");

                    continue;
                }

                sheetData.data = values.ToString();

                AssetDatabase.CreateAsset(sheetData, $"{sheetDataAssetPath}/{sheetData.title}.asset");
            }
        }
    }
}