using System.IO;

namespace GoogleSpreadSheetLoader.Generate
{
    public partial class GSSL_Generate
    {
        private static readonly string tableScriptSavePath = "Assets/GoogleSpreadSheetLoader/Generated/TableScript/";
        private static readonly string dataScriptSavePath = "Assets/GoogleSpreadSheetLoader/Generated/DataScript/";
        private static readonly string dataSavePath = "Assets/GoogleSpreadSheetLoader/Generated/DataScript/";
        private static readonly string enumDefSavePath = "Assets/GoogleSpreadSheetLoader/Generated/Enum/";
        private static readonly string localizationSavePath = "Assets/GoogleSpreadSheetLoader/Generated/Localization/";

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

            if (!Directory.Exists(localizationSavePath))
            {
                Directory.CreateDirectory(localizationSavePath);
            }
        }
    }
}