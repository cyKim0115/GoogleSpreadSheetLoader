using System.IO;
using UnityEditor;
using UnityEngine;

namespace GoogleSpreadSheetLoader
{
    public partial class GoogleSpreadSheetLoaderWindow
    {
        private readonly string _ScriptPath = "Assets/GoogleSpreadSheetLoader/Script";
        
        private async Awaitable CreateScript()
        {
            string url = $"{Application.dataPath}/GoogleSpreadSheetLoader/Script/TableData.txt";
            if (!File.Exists(url))
            {
                Debug.LogError($"No file found \n {url}");
            }
            else
            {
                FileStream fileStream = File.Open($"{_ScriptPath}/TableData.txt", FileMode.Open);
                StreamReader reader = new StreamReader(fileStream);
                string readString = await reader.ReadToEndAsync();
                
                Debug.Log(readString);
            }
        }
    }
}