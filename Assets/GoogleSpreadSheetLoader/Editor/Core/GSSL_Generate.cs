using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TableData;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace GoogleSpreadSheetLoader.Generate
{
    public class GSSL_Generate
    {
        private static readonly string tableScriptSavePath = "Assets/GoogleSpreadSheetLoader/Generated/TableScript/";
        private static readonly string dataScriptSavePath = "Assets/GoogleSpreadSheetLoader/Generated/DataScript/";
        private static readonly string dataSavePath = "Assets/GoogleSpreadSheetLoader/Generated/DataScript/";
        private static readonly string enumDefSavePath = "Assets/GoogleSpreadSheetLoader/Generated/Enum/";

        private static void CheckAndCreateDirectory()
        {
            if (!Directory.Exists(tableScriptSavePath))
            {
                Directory.CreateDirectory(tableScriptSavePath);
            }

            if (!Directory.Exists(dataScriptSavePath))
            {
                Directory.CreateDirectory(dataScriptSavePath);
            }

            if (!Directory.Exists(dataSavePath))
            {
                Directory.CreateDirectory(dataSavePath);
            }

            if (!Directory.Exists(enumDefSavePath))
            {
                Directory.CreateDirectory(enumDefSavePath);
            }
        }

        public static void GenerateTableScripts(List<SheetData> sheets)
        {
            CheckAndCreateDirectory();

            foreach (var sheet in sheets)
            {
                string dataClassName = sheet.title + "Data";
                string tableClassName = sheet.title + "Table";
                string dataFilePath = dataScriptSavePath + dataClassName + ".cs";
                string tableFilePath = tableScriptSavePath + tableClassName + ".cs";

                List<string> variableDeclarations = new List<string>();
                List<int> validColumns = new List<int>();

                List<List<string>> sheetRows = JsonConvert.DeserializeObject<List<List<string>>>(sheet.data);

                if (sheetRows == null || sheetRows.Count < 2) continue;

                var setData = "";
                List<string> headers = sheetRows[0];
                for (int i = 0; i < headers.Count; i++)
                {
                    if (string.IsNullOrWhiteSpace(headers[i]) || !headers[i].Contains("-"))
                        continue;

                    string[] splitHeader = headers[i].Split('-');
                    if (splitHeader.Length < 2) continue;

                    string varName = splitHeader[0].Trim();
                    string varType = ConvertToCSharpType(splitHeader[1].Trim());

                    variableDeclarations.Add($"    public {varType} {varName} => _{varName};\n");
                    variableDeclarations.Add($"    [SerializeField] private {varType} _{varName};\n\n");
                    validColumns.Add(i);

                    if (varType == "string")
                    {
                        setData += $"\t\t_{varName} = data[{i}].ToString();\n";
                    }
                    else
                    {
                        setData += $"\t\t_{varName} = {varType}.Parse(data[{i}]);\n";
                    }
                }

                setData = "\tpublic void SetData(List<string> data)\n\t{\n" + $"{setData}" + "\t}\n";

                string dataClassTemplate = $"using System;\n"
                                           + $"using System.Collections.Generic;\n"
                                           + "using TableData;\n"
                                           + "using UnityEngine;\n"
                                           + "\n"
                                           + "[Serializable]\n"
                                           + $"public class {dataClassName} : {nameof(IData)}\n{{\n"
                                           + string.Join("", variableDeclarations)
                                           + string.Join("", setData)
                                           + "}\n";

                File.WriteAllText(dataFilePath, dataClassTemplate);

                setData = "\tpublic void SetData(List<List<string>> data)\n\t{\n"
                          + $"\t\tdataList = new List<{dataClassName}>();\n"
                          + $"\t\tforeach (var item in data)\n"
                          + $"\t\t{{\n\t\t\t{dataClassName} newData = new();\n"
                          + $"\t\t\tnewData.SetData(item);\n"
                          + $"\t\t\tdataList.Add(newData);\n\t\t}}"
                          + "\n\t}\n";

                string tableTemplate = $"using System.Collections.Generic;\n"
                                       + "using TableData;\n"
                                       + "using UnityEngine;\n"
                                       + "\n"
                                       + $"[CreateAssetMenu(fileName = \"{tableClassName}\", menuName = \"Tables/{tableClassName}\")]\n"
                                       + $"public partial class {tableClassName} : ScriptableObject, {nameof(ITable)}\n{{\n"
                                       + $"    public List<{dataClassName}> dataList = new List<{dataClassName}>();\n\n"
                                       + string.Join("", setData)
                                       + "}\n";

                File.WriteAllText(tableFilePath, tableTemplate);
            }

            AssetDatabase.Refresh();
        }

        public static void GenerateTableData(List<SheetData> listSheet)
        {
            CheckAndCreateDirectory();

            foreach (var sheet in listSheet)
            {
                string tableAssetPath = dataSavePath + sheet.title + "Table.asset";
                string className = sheet.title + "Data";
                string tableClassName = sheet.title + "Table";

                List<List<string>> sheetRows = JsonConvert.DeserializeObject<List<List<string>>>(sheet.data);

                if (sheetRows == null || sheetRows.Count < 2) return;

                List<string> headers = sheetRows[0];
                List<int> validColumns = new List<int>();

                for (int i = 0; i < headers.Count; i++)
                {
                    if (!string.IsNullOrWhiteSpace(headers[i]) && headers[i].Contains("-"))
                    {
                        validColumns.Add(i);
                    }
                }

                var tableAsset = ScriptableObject.CreateInstance(Type.GetType(tableClassName));
                tableAsset.hideFlags = HideFlags.None;
                if (tableAsset == null)
                {
                    Debug.LogError($"Failed to create instance of {tableClassName}");
                    return;
                }

                sheetRows.RemoveAt(0);

                (tableAsset as ITable).SetData(sheetRows);

                AssetDatabase.CreateAsset(tableAsset, tableAssetPath);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static string ConvertToCSharpType(string type)
        {
            return type switch
            {
                "int" => "int",
                "float" => "float",
                "bool" => "bool",
                "long" => "long",
                "string" => "string",
                _ when type.Contains(".") => type, // 네임스페이스가 포함된 사용자 정의 타입 처리
                _ => "string"
            };
        }

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