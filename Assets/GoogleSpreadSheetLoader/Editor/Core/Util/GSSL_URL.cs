namespace GoogleSpreadSheetLoader
{
    public static class GSSL_URL
    {
        public static readonly string DownloadSheetUrl = "https://sheets.googleapis.com/v4/spreadsheets/{0}/values/{1}?key={2}";
        public static readonly string DownloadSpreadSheetUrl = "https://sheets.googleapis.com/v4/spreadsheets/{0}?key={1}";
        public static readonly string SpreadSheetOpenUrl = "https://docs.google.com/spreadsheets/d/{0}/edit?key={1}";
    }
}