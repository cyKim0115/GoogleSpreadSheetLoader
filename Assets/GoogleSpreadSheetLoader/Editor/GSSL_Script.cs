using System.IO;
using GoogleSpreadSheetLoader.Setting;
using UnityEngine;

namespace GoogleSpreadSheetLoader.Script
{
    public class GSSL_Script
    {
        public static async Awaitable CreateScript()
        {
            string url = $"{Application.dataPath}/GoogleSpreadSheetLoader/Script/TableData.txt";
            if (!File.Exists(url))
            {
                Debug.LogError($"No file found \n {url}");
            }
            else
            {
                FileStream fileStream = File.Open($"{GSSL_Setting.ScriptPath}/TableData.txt", FileMode.Open);
                StreamReader reader = new StreamReader(fileStream);
                string readString = await reader.ReadToEndAsync();
                
                Debug.Log(readString);
            }
        }
    }
}