using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoogleSpreadSheetLoader.Download;
using GoogleSpreadSheetLoader.Generate;
using GoogleSpreadSheetLoader.Simple;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using static GoogleSpreadSheetLoader.SheetData;

namespace GoogleSpreadSheetLoader.OneButton
{
    public abstract class GSSL_OneButton
    {
        private static readonly string TableLinkerPrefsKey = "TableLinkerLink";
        private static readonly string GenerateDataPrefsKey = "GenerateData";
        private static bool GenerateDataFlag => EditorPrefs.HasKey(GenerateDataPrefsKey);
        private static string GenerateDataString
        {
            get => EditorPrefs.HasKey(GenerateDataPrefsKey) ? EditorPrefs.GetString(GenerateDataPrefsKey) : string.Empty;
            set {
                if (string.IsNullOrEmpty(value))
                    EditorPrefs.DeleteKey(GenerateDataPrefsKey);
                else
                    EditorPrefs.SetString(GenerateDataPrefsKey, value);
            }
        }

        public static bool TableLinkerFlag
        {
            get => EditorPrefs.HasKey(TableLinkerPrefsKey);
            set {
                if (value)
                {
                    EditorPrefs.SetString(TableLinkerPrefsKey, true.ToString());
                }
                else
                {
                    EditorPrefs.DeleteKey(TableLinkerPrefsKey);
                }
            }
        }

        public static async Awaitable OneButtonProcessSpreadSheet()
        {
            try
            {
                var listDownloadInfo = await GSSL_Download.DownloadSpreadSheetAll();
                GSSL_Log.Log("Download SpreadSheet Done");

                GSSL_Log.Log("Download Sheet Start");
                await OneButtonProcessSheet(listDownloadInfo);
                GSSL_Log.Log("Download Sheet Done");
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public static async Awaitable OneButtonProcessSheet(List<GSSL_DownloadInfo> listDownloadInfo)
        {
            await GSSL_Download.DownloadSheet(listDownloadInfo);

            var listSheetData = GSSL_Generate.GetSheetDataList()
                .Where(x => listDownloadInfo.Any(downloadInfo => downloadInfo.SheetName == x.title));

            var dicSheetData = new Dictionary<eTableStyle, List<SheetData>>();

            dicSheetData.TryAdd(eTableStyle.EnumType, new());
            dicSheetData.TryAdd(eTableStyle.Common, new());
            dicSheetData.TryAdd(eTableStyle.Localization, new());

            foreach (var sheetData in listSheetData)
            {
                var listExistingSheetTitle = new List<string>();
                var listExistingSheetData = GSSL_Generate.GetSheetDataList();
                if(listExistingSheetData?.Count > 0)
                {
                    listExistingSheetTitle.AddRange(from item in listExistingSheetData select item.title);
                }
                bool isRefresh = false;
                await GSSL_Download.DownloadSheet(listDownloadInfo);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                GSSL_DownloadedSheet.Reset();

                SimpleView.SetProgressState(eSimpleViewState.GenerateSheetData);
                
                var listSheetData = GSSL_Generate.GetSheetDataList()
                    .Where(x => listDownloadInfo.Any(downloadInfo => downloadInfo.SheetName == x.title));

                var dicSheetData = new Dictionary<eTableStyle, List<SheetData>>();

                dicSheetData.TryAdd(eTableStyle.EnumType, new());
                dicSheetData.TryAdd(eTableStyle.Common, new());
                dicSheetData.TryAdd(eTableStyle.Localization, new());

                foreach (var sheetData in listSheetData)
                {
                    dicSheetData[sheetData.tableStyle].Add(sheetData);
                }
                
                foreach(var title in listSheetData.Select(x=>x.title))
                {
                    if (!listExistingSheetTitle.Contains(title))
                    {
                        isRefresh = true;
                        break;
                    }
                }

                SimpleView.SetProgressState(eSimpleViewState.GenerateTableScript);
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

                GSSL_Generate.GenerateTableLinkerScript(dicSheetData[eTableStyle.Common]);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                if(!isRefresh)
                    CheckPrefsAndGenerateTableData();
            }

            foreach ((eTableStyle tableStyle, var list) in dicSheetData)
            {
                try
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
            
            GSSL_Generate.GenerateTableLinkerScript(dicSheetData[eTableStyle.Common]);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            CheckPrefsAndGenerateTableData();
        }

        private static async Awaitable GenerateTableLinkerAsync()
        {
            await Task.Delay(100);

            GSSL_Generate.GenerateTableLinkerData();
        }

        [InitializeOnLoadMethod]
        private static void CheckPrefsAndGenerateTableData()
        {
            GSSL_Log.Log($"Generate Data Check ({GenerateDataFlag})");

            if (GenerateDataFlag)
            {
                SimpleView.SetProgressState(eSimpleViewState.GenerateTableData);
                
                var str = GenerateDataString;

                GSSL_Log.Log("Generate Data Start");
                GenerateData(str);
                GSSL_Log.Log("Generate Data Done");

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            if (TableLinkerFlag)
            {
                SimpleView.SetProgressState(eSimpleViewState.GenerateTableLinker);
                
                GenerateTableLinkerAsync();

                TableLinkerFlag = false;
            }

            GenerateDataString = string.Empty;

            Task.Run(async () => {
                SimpleView.SetProgressState(eSimpleViewState.Done);
                await Task.Delay(500);
                SimpleView.SetProgressState(eSimpleViewState.None);
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

            TableLinkerFlag = true;
        }
    }
}