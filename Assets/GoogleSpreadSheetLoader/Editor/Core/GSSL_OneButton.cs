using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoogleSpreadSheetLoader.Download;
using GoogleSpreadSheetLoader.Generate;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using static GoogleSpreadSheetLoader.SheetData;
using static GoogleSpreadSheetLoader.GSSL_State;

namespace GoogleSpreadSheetLoader.OneButton
{
    public static class GSSL_OneButton
    {
        private static readonly string GenerateDataPrefsKey = "GenerateData";
        private static bool GenerateDataFlag => EditorPrefs.HasKey(GenerateDataPrefsKey);
        private static string GenerateDataString
        {
            get => EditorPrefs.HasKey(GenerateDataPrefsKey) ? EditorPrefs.GetString(GenerateDataPrefsKey) : string.Empty;
            set
            {
                if (string.IsNullOrEmpty(value))
                    EditorPrefs.DeleteKey(GenerateDataPrefsKey);
                else
                    EditorPrefs.SetString(GenerateDataPrefsKey, value);
            }
        }

        public static bool TableLinkerFlag
        {
            get => EditorPrefs.HasKey("TableLinkerLink");
            set
            {
                if (value)
                {
                    EditorPrefs.SetString("TableLinkerLink", true.ToString());
                }
                else
                {
                    EditorPrefs.DeleteKey("TableLinkerLink");
                }
            }
        }

        public static async Awaitable OneButtonProcessSpreadSheet()
        {
            try
            {
                GSSL_Path.ClearGeneratedFolder();
                
                GSSL_Log.Log("Download SpreadSheet Start");
                var listDownloadInfo = await GSSL_Download.DownloadSpreadSheetAll();
                GSSL_Log.Log("Download SpreadSheet Done");

                GSSL_Log.Log("Download Sheet Start");
                await OneButtonProcessSheet(listDownloadInfo);
                GSSL_Log.Log("Download Sheet Done");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Google SpreadSheet Loader 처리 중 에러가 발생했습니다: {ex.Message}");
                
                // 에러 발생 시 상태 초기화
                SetProgressState(eGSSL_State.None);
                EditorWindow.focusedWindow?.Repaint();
                
                // 에러 발생 시 생성된 데이터 초기화
                GenerateDataString = string.Empty;
                TableLinkerFlag = false;
                
                // 에러 발생 시 다운로드된 시트 데이터 초기화
                GSSL_DownloadedSheet.ClearAllSheetData();
                
                throw; // 에러를 다시 던져서 UI에서 처리할 수 있도록 함
            }
        }

        internal static async Awaitable OneButtonProcessSheet(List<RequestInfo> listRequestInfo)
        {
            try
            {
                await GSSL_Download.DownloadSheet(listRequestInfo);

                var listSheetData = GSSL_DownloadedSheet.GetAllSheetData()
                    .Where(x => listRequestInfo.Any(downloadInfo => downloadInfo.SheetName == x.title));

                var dicSheetData = new Dictionary<eTableStyle, List<SheetData>>();

                dicSheetData.TryAdd(eTableStyle.EnumType, new());
                dicSheetData.TryAdd(eTableStyle.Common, new());
                dicSheetData.TryAdd(eTableStyle.Localization, new());

                foreach (var sheetData in listSheetData)
                {
                    dicSheetData[sheetData.tableStyle].Add(sheetData);
                }

                SetProgressState(eGSSL_State.GenerateTableScript);
                foreach ((eTableStyle tableStyle, var list) in dicSheetData)
                {
                    switch (tableStyle)
                    {
                        case eTableStyle.Common:
                            GSSL_Generate.GenerateTableScripts(list);
                            break;
                        case eTableStyle.EnumType:
                            GSSL_Generate.GenerateEnumDef(list);
                            break;
                        case eTableStyle.Localization:
                            GSSL_Generate.GenerateLocalize(list);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                dicSheetData.Remove(eTableStyle.EnumType);
                dicSheetData.Remove(eTableStyle.Localization);
                var str = JsonConvert.SerializeObject(dicSheetData);
                GenerateDataString = str;
                TableLinkerFlag = true;

                GSSL_Generate.GenerateTableLinkerScript();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                CheckPrefsAndGenerateTableData();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"시트 처리 중 에러가 발생했습니다: {ex.Message}");
                
                // 에러 발생 시 상태 초기화
                SetProgressState(eGSSL_State.None);
                EditorWindow.focusedWindow?.Repaint();
                
                // 에러 발생 시 생성된 데이터 초기화
                GenerateDataString = string.Empty;
                TableLinkerFlag = false;
                
                // 에러 발생 시 다운로드된 시트 데이터 초기화
                GSSL_DownloadedSheet.ClearAllSheetData();
                
                throw; // 에러를 다시 던져서 상위에서 처리할 수 있도록 함
            }
        }

        private static async Awaitable GenerateTableLinkerAsync()
        {
            await Task.Delay(100);

            GSSL_Log.Log("Generate Table Start");
            GSSL_Generate.GenerateTableLinkerData();
            GSSL_Log.Log("Generate Table Done");
        }

        [InitializeOnLoadMethod]
        private static void CheckPrefsAndGenerateTableData()
        {
            GSSL_Log.Log($"Generate Data Check ({GenerateDataFlag})");

            if (GenerateDataFlag)
            {
                SetProgressState(eGSSL_State.GenerateTableData);

                var str = GenerateDataString;

                GSSL_Log.Log("Generate Data Start");
                GenerateData(str);
                GSSL_Log.Log("Generate Data Done");

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            GSSL_Log.Log($"Generate TableLinker Check ({TableLinkerFlag})");
            if (TableLinkerFlag)
            {
                SetProgressState(eGSSL_State.GenerateTableLinker);

                GenerateTableLinkerAsync().GetAwaiter().GetResult();

                TableLinkerFlag = false;
            }

            GenerateDataString = string.Empty;

            Task.Run(async () =>
            {
                SetProgressState(eGSSL_State.Done);
                await Task.Delay(500);
                SetProgressState(eGSSL_State.None);
            });
        }

        private static void GenerateData(string str)
        {
            var dic = JsonConvert.DeserializeObject<Dictionary<eTableStyle, List<SheetData>>>(str);

            foreach ((eTableStyle tableStyle, var list) in dic)
            {
                switch (tableStyle)
                {
                    case eTableStyle.Common:
                        GSSL_Generate.GenerateTableData(list);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}