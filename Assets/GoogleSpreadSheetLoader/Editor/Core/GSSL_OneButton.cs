using System;
using System.Collections.Generic;
using System.Linq;
using GoogleSpreadSheetLoader.Download;
using GoogleSpreadSheetLoader.Generate;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using static GoogleSpreadSheetLoader.SheetData;

namespace GoogleSpreadSheetLoader.OneButton
{
    public class GSSL_OneButton
    {
        public static async Awaitable OneButtonProcess(List<GSSL_DownloadInfo> listDownloadInfo)
        {
            await GSSL_Download.DownloadSheet(listDownloadInfo);

            var listSheetData = GSSL_Generate.GetSheetDataList()
                .Where(x => listDownloadInfo.Any(y => y.GetSheetName() == x.title));

            var dicSheetData = new Dictionary<eTableStyle, List<SheetData>>();
            
            dicSheetData.TryAdd(eTableStyle.EnumType, new ());
            dicSheetData.TryAdd(eTableStyle.None, new ());
            dicSheetData.TryAdd(eTableStyle.Localization, new ());
            
            foreach (var sheetData in listSheetData)
            {
                dicSheetData[sheetData.tableStyle].Add(sheetData);
            }

            foreach ((eTableStyle tableStyle, var list) in dicSheetData)
            {
                switch (tableStyle)
                {
                    case eTableStyle.None:
                        GSSL_Generate.GenerateTableScripts(list);
                        break;
                    case eTableStyle.EnumType:
                        GSSL_Generate.GenerateEnumDef(list);
                        break;
                    case eTableStyle.Localization:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            dicSheetData.Remove(eTableStyle.EnumType);
            var str = JsonConvert.SerializeObject(dicSheetData);
            EditorPrefs.SetString("GenerateData", str);
            
            AssetDatabase.Refresh();
        }
        
        [InitializeOnLoadMethod]
        private static void CheckPrefsAndGenerateTableData()
        {
            if (!EditorPrefs.HasKey("GenerateData"))
            {
                return;
            }

            var str = EditorPrefs.GetString("GenerateData");
            EditorPrefs.DeleteKey("GenerateData");

            var dic = JsonConvert.DeserializeObject<Dictionary<eTableStyle, List<SheetData>>>(str);

            foreach ((eTableStyle tableStyle, var list) in dic)
            {
                switch (tableStyle)
                {
                    case eTableStyle.None:
                        GSSL_Generate.GenerateTableData(list);
                        break;
                    case eTableStyle.Localization:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}