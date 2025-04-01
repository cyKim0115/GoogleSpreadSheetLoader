using System.Collections.Generic;
using System.IO;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace GoogleSpreadSheetLoader.Generate
{
    public partial class GSSL_Generate
    {
        public static void GenerateEnumDef(List<SheetData> sheets)
        {
            CheckAndCreateDirectory();

            var dataFilePath = enumDefSavePath + "EnumDef.cs";
            Dictionary<int, List<string>> dicEnumStr = new();
            Dictionary<string, string> dicDuplicateCheck = new();
            
            foreach (var sheet in sheets)
            {
                var validColumns = new List<int>();

                var sheetRows = JsonConvert.DeserializeObject<List<List<string>>>(sheet.data);

                if (sheetRows == null || sheetRows.Count < 2) continue;

                // 종류 별로 일단 담은
                var headers = sheetRows[0];
                for (int i = 0; i < headers.Count; i++)
                {
                    var enumName = headers[i];
                    
                    if (string.IsNullOrWhiteSpace(enumName))
                        continue;
                    
                    validColumns.Add(i);

                    if (!dicDuplicateCheck.TryAdd(enumName, sheet.name))
                    {
                        Debug.LogError($"! 중복된 enum 이름 ({enumName})! \n ({sheet.name},{dicDuplicateCheck[enumName]})");
                        continue;
                    }

                    if (!dicEnumStr.TryAdd(i, new List<string>() { enumName }))
                    {
                        Debug.LogError($"이게 왜 실패하지?");
                    }
                }

                // 맨 앞줄은 이름이었으니 제외
                sheetRows.RemoveAt(0);

                // 그 후 enum들을 추가로 넣어줌
                foreach (var row in sheetRows)
                {
                    foreach (var column in validColumns)
                    {
                        if (row.Count <= column)
                        {
                            break;
                        }
                        
                        if(string.IsNullOrEmpty(row[column]))
                            continue;
                        
                        dicEnumStr[column].Add(row[column]);
                    }
                }

                List<string> listEnumTitle = new();
                Dictionary<string, List<string>> dicEnumName = new();
                Dictionary<string, List<string>> dicEnumIdx = new();
                foreach (var list in dicEnumStr.Values)
                {
                    var isIdx = list[0].Contains('-');
                    var dictKey = isIdx ? list[0].Split('-')[0]: list[0];
                    
                    if(!listEnumTitle.Contains(dictKey))
                        listEnumTitle.Add(dictKey);

                    List<string> currentList = null;
                    
                    if (!isIdx)
                    {
                        dicEnumName.TryAdd(dictKey, new());
                        currentList =dicEnumName[dictKey];
                    }
                    else
                    {
                        dicEnumIdx.TryAdd(dictKey, new());
                        currentList =dicEnumIdx[dictKey];
                    }
                    
                    list.RemoveAt(0);
                    foreach (var value in list)
                    {
                        currentList.Add(value);
                    }
                }

                string data = "\n";
                // 만든 string List를 토대로 enum 작성
                foreach (var title in listEnumTitle)
                {
                    var listName = dicEnumName[title];
                    var listIdx = dicEnumIdx[title];
                    
                    data += $"\npublic enum {title}\n{{\n";
                    for (var i = 0; i < listName.Count; i++)
                    {
                        data += $"\t{listName[i]} = {listIdx[i]},\n";
                    }
                    data += $"}}\n";
                }
                
                File.WriteAllText(dataFilePath, data);
            }

            AssetDatabase.Refresh();
        }
    }
}