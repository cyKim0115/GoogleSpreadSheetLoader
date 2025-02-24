using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GoogleSpreadSheetLoader.Script
{
    public static class GSSL_Script
    {
        private static string templatePath = "Assets/GoogleSpreadSheetLoader/Script/TableData.txt";
        private static string scriptSavePath = "Assets/GoogleSpreadSheetLoader/Generated/Tables/";
        
        public static void CreateScript(List<SheetData> selectedTables)
        {
            if (!File.Exists(templatePath))
            {
                Debug.LogError("Template file not found: " + templatePath);
                return;
            }

            if (!Directory.Exists("Assets/GoogleSpreadSheetLoader/Generated/Tables/"))
            {
                Directory.CreateDirectory("Assets/GoogleSpreadSheetLoader/Generated/Tables/");
            }
            
            foreach (var sheet in selectedTables)
            {
                if (sheet.tableStyle != SheetData.eTableStyle.None) continue;
                
                string template = File.ReadAllText(templatePath);
                string className = sheet.title + "Table";
                string filePath = scriptSavePath + className + ".cs";
                
                List<string> variableDeclarations = new List<string>();
                List<string> setDataLogic = new List<string>();
                List<int> validColumns = new List<int>();
                
                string[] lines = sheet.data.Split('\n');
                if (lines.Length < 2) continue;
                
                string[] headers = lines[0].Split(',');
                for (int i = 0; i < headers.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(headers[i]) || !headers[i].Contains("-"))
                        continue;
                    
                    string[] splitHeader = headers[i].Split('-');
                    string varName = splitHeader[0].Trim();
                    string varType = ConvertToCSharpType(splitHeader[1].Trim());
                    
                    variableDeclarations.Add($"public {varType} {varName};");
                    validColumns.Add(i);
                }
                
                for (int i = 1; i < lines.Length; i++)
                {
                    string[] row = lines[i].Split(',');
                    List<string> values = new List<string>();
                    
                    foreach (int colIndex in validColumns)
                    {
                        values.Add(ParseValue(row[colIndex]));
                    }
                    
                    setDataLogic.Add($"        dataList.Add(new {className}() {{ {string.Join(", ", values)} }});");
                }
                
                string finalScript = template
                    .Replace("ClassName", className)
                    .Replace("VariableArea", string.Join("\n    ", variableDeclarations))
                    .Replace("SetDataArea", string.Join("\n", setDataLogic));
                
                File.WriteAllText(filePath, finalScript);
                AssetDatabase.Refresh();
            }
        }
        
        private static string ConvertToCSharpType(string type)
        {
            return type switch
            {
                "int" => "int",
                "float" => "float",
                "bool" => "bool",
                "string" => "string",
                _ => "string"
            };
        }
        
        private static string ParseValue(string value)
        {
            if (int.TryParse(value, out _)) return value;
            if (float.TryParse(value, out _)) return value + "f";
            if (bool.TryParse(value, out _)) return value.ToLower();
            return "\"" + value + "\"";
        }
    }
}