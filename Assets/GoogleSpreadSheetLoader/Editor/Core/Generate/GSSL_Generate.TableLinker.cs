namespace GoogleSpreadSheetLoader.Generate
{
    public class GSSL_Generate_TableLinker
    {
        private static string tableLinkerContents =
            "using System.Collections.Generic;\nusing UnityEngine;\nusing UnityEngine.Serialization;\n\nnamespace TableData\n{\n    [CreateAssetMenu(fileName = \"TableLinker\", menuName = \"Tables/TableLinker\")]\n    public partial class TableLinker : ScriptableObject\n    {\n        {0}\n    }\n}";

        public static void CreateTableLinker()
        {
            var sheetDataList = GSSL_Generate.GetSheetDataList();

            var declation = "";

            foreach (var sheetData in sheetDataList)
            {
                
            }
        }
    }
}