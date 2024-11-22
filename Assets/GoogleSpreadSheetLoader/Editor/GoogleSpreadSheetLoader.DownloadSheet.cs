using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace GoogleSpreadSheetLoader
{
    public partial class GoogleSpreadSheetLoaderWindow
    {
        private readonly string _downloadSheetUrl =
            "https://sheets.googleapis.com/v4/spreadsheets/{0}/values/{1}?key={2}";
        private readonly string _sheetDataAssetPath = "Assets/GoogleSpreadSheetLoader/Generated/SheetData";
        
        private async Awaitable DownloadSheet()
        {
            // 다운로드 대상 정리
            List<(string spreadsheetId,string sheetName)> listDownloadTarget = new List<(string spreadsheetId, string sheetName)>();

            foreach (KeyValuePair<string, Dictionary<string, bool>> spreadSheetPair in _dicDownloadSheetCheck)
            {
                string spreadsheetId = spreadSheetPair.Key;

                foreach (KeyValuePair<string, bool> sheetPair in spreadSheetPair.Value)
                {
                    if (!sheetPair.Value)
                        continue;

                    string sheetName = sheetPair.Key;

                    listDownloadTarget.Add((spreadsheetId, sheetName));
                }
            }

            _sheetDownloadState = eDownloadState.Downloading;

            // 다운로드
            List<((string spreadSheetId, string sheetName), UnityWebRequestAsyncOperation oper)> listInfoOperPair =
                new List<((string spreadSheetId, string sheetName), UnityWebRequestAsyncOperation oper)>();
            try
            {
                foreach ((string spreadSheetId, string sheetName) info in listDownloadTarget)
                {
                    string url = string.Format(_downloadSheetUrl, info.spreadSheetId,info.sheetName, _settingData.apiKey);

                    UnityWebRequest webRequest = UnityWebRequest.Get(url);

                    UnityWebRequestAsyncOperation asyncOperator = webRequest.SendWebRequest();
                    listInfoOperPair.Add((info, asyncOperator));
                }

                do
                {
                    // Debug.Log($"({listOperator.Count(x=>x.webRequest.isDone)}/{listOperator.Count})");
                    _sheetDownloadMessage =
                        $"다운로드 중 ({listInfoOperPair.Count(x => x.oper.isDone)}/{listInfoOperPair.Count})";
                    Repaint();
                    await Task.Delay(100);
                } while (listInfoOperPair.Any(x => !x.oper.isDone));
            }
            finally
            {
                _sheetDownloadMessage = "다운로드 완료";
                _sheetDownloadState = eDownloadState.Complete;
                await Task.Delay(1000);
                _dicDownloadSheetCheck.Clear();
                _sheetDownloadState = eDownloadState.None;
            }

            if (!Directory.Exists(_sheetDataAssetPath))
            {
                Directory.CreateDirectory(_sheetDataAssetPath);
            }
            
            // 다운로드 받은 데이터 정리
            foreach (((string spreadSheetId, string sheetName) info, UnityWebRequestAsyncOperation oper) pair in listInfoOperPair)
            {
                SheetData sheetData = CreateInstance<SheetData>();

                sheetData.title = pair.info.sheetName;

                if (sheetData.title.Contains(_settingData.sheet_enumTypeStr))
                    sheetData.tableStyle = SheetData.eTableStyle.EnumType;
                else if (sheetData.title.Contains(_settingData.sheet_localizationTypeStr))
                    sheetData.tableStyle = SheetData.eTableStyle.Localization;
                else
                    sheetData.tableStyle = SheetData.eTableStyle.None;
                
                JObject jObj = JObject.Parse(pair.oper.webRequest.downloadHandler.text);

                if (!jObj.TryGetValue("values",out var values))
                {
                    Debug.LogError($"변환 실패 - {pair.info.sheetName} \n{pair.oper.webRequest.downloadHandler.text}");
                    
                    continue;
                }

                sheetData.data = values.ToString();
                
                AssetDatabase.CreateAsset(sheetData,$"{_sheetDataAssetPath}/{sheetData.title}.asset");
            }
        }
    }
}