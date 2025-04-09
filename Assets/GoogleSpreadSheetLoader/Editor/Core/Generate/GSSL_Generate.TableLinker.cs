using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GoogleSpreadSheetLoader.Generate
{
    public partial class GSSL_Generate
    {
        private static string tableLinkerScriptPath = "Assets/GoogleSpreadSheetLoader/Generated/Script/";
        private static string tableLinkerDataPath = "Assets/GoogleSpreadSheetLoader/Generated/SerializeObject/";


        public static void GenerateTableLinkerScript(List<SheetData> sheetDataList)
        {
            var declation = "";

            foreach (var sheetData in sheetDataList)
            {
                var className = sheetData.title + "Table";

                declation += $"\t\t public {className} {className};\n";
            }

            var contents =
                "using System.Collections.Generic;\n" +
                "using UnityEngine;\n" +
                "using UnityEngine.Serialization;\n\n" +
                "namespace TableData\n" +
                "{\n" +
                "    [CreateAssetMenu(fileName = \"TableLinker\", menuName = \"Tables/TableLinker\")]\n" +
                "    public partial class TableLinker : ScriptableObject\n" +
                "    {\n"
                + declation
                + "\n    }\n}";
            var path = tableLinkerScriptPath + "TableLinker.cs";

            File.WriteAllText(path, contents);
        }
        
        public static void GenerateTableLinkerData()
        {
            var tableLinkerAssetPath = tableLinkerDataPath + "TableLinker.asset";
            
            var tableLinkerAsset = ScriptableObject.CreateInstance(Type.GetType("TableLinker"));
            tableLinkerAsset.hideFlags = HideFlags.None;

            AssignFirstMatchingAssets(tableLinkerAsset);
            
            AssetDatabase.CreateAsset(tableLinkerAsset, tableLinkerAssetPath);

        }
        
        private static void AssignFirstMatchingAssets(UnityEngine.Object target)
        {
            if (target == null)
            {
                Debug.LogWarning("타겟 오브젝트가 null입니다.");
                return;
            }

            Type targetType = target.GetType();
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.GetIterator();

            property.Next(true); // 첫번째는 항상 "m_Script"라서 무시

            while (property.NextVisible(false))
            {
                if (property.propertyType == SerializedPropertyType.ObjectReference)
                {
                    Type propertyType = GetFieldType(targetType, property.name);

                    if (propertyType != null && typeof(UnityEngine.Object).IsAssignableFrom(propertyType))
                    {
                        string[] guids = AssetDatabase.FindAssets($"t:{propertyType.Name}");

                        if (guids.Length > 0)
                        {
                            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath(path, propertyType);
                            property.objectReferenceValue = asset;
                        }
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static Type GetFieldType(Type targetType, string propertyName)
        {
            FieldInfo field = targetType.GetField(propertyName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (field != null)
            {
                return field.FieldType;
            }

            // SerializedProperty는 필드명이 그대로 오지 않으므로 대체 시도
            foreach (var f in targetType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                if (ObjectNames.NicifyVariableName(f.Name) == propertyName)
                {
                    return f.FieldType;
                }
            }

            return null;
        }
    }
}