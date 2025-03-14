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
                List<string> variableDeclarations = new List<string>();
                List<int> validColumns = new List<int>();

                List<List<string>> sheetRows = JsonConvert.DeserializeObject<List<List<string>>>(sheet.data);

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
                        if(string.IsNullOrEmpty(row[column - 1]))
                            continue;
                        
                        if (!dicEnumStr.ContainsKey(column))
                        {
                            Debug.LogError("2");
                        }
                        
                        dicEnumStr[column].Add(row[column - 1]);
                    }
                }

                string data = "\n";
                // 만든 string List를 토대로 enum 작성
                foreach (var list in dicEnumStr.Values)
                {
                    var name = list[0];
                    list.RemoveAt(0);
                    
                    data += $"\npublic enum {name}\n{{\n";
                    foreach (var str in list)
                    {
                        data += $"\t{str},\n";
                    }
                    data += $"}}\n";
                }
                
                File.WriteAllText(dataFilePath, data);
            }

            AssetDatabase.Refresh();
        }
    }
}