using GoogleSpreadSheetLoader.Setting;
using UnityEngine.Networking;

namespace GoogleSpreadSheetLoader.Download
{
    public class GSSL_DownloadInfo
    {
        public GSSL_DownloadInfo(string spreadSheetId, string sheetName)
        {
            _spreadSheetId = spreadSheetId;
            _sheetName = sheetName;

            _url = string.Format(GSSL_URL.DownloadSheetUrl, _spreadSheetId, _sheetName,
                GSSL_Setting.SettingData.apiKey);
            _webRequest = UnityWebRequest.Get(_url);
        }

        public string SpreadSheetId => _spreadSheetId;
        private readonly string _spreadSheetId;
        public string SheetName => _sheetName;
        private readonly string _sheetName;
        public string URL => _url;
        private readonly string _url;

        public bool IsDone => _webRequest?.isDone ?? false;
        public string DownloadText => _webRequest?.downloadHandler?.text ?? "";
        
        private readonly UnityWebRequest _webRequest;
        
        public void SendAndGetAsyncOperation()
        {
            _webRequest.SendWebRequest();
        }
    }
}