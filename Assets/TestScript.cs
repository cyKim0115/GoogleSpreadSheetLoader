using TableData;
using UnityEngine;
using TMPro;

public class TestScript : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputFieldDataId;
    [SerializeField] private TMP_Text txtDataValue;
    
    [Space(5)]
    [SerializeField] private SystemLanguage language;
    [SerializeField] private TMP_InputField inputFieldLocalizeKey;
    [SerializeField] private TMP_InputField inputFieldLocalizeParam1;
    [SerializeField] private TMP_InputField inputFieldLocalizeParam2;
    [SerializeField] private TMP_Text txtLocalizeValue;

    private TableLinker _tableLinker;
    
    private void Start()
    {
        _tableLinker = Resources.Load<TableLinker>("TableLinker");
    }

    public void OnClickData()
    {
        // inputFieldDataId.text;
        // _tableLinker.StageTable.dataList.Find(x=>x.ID == )
    }
    
    public void OnClickLocalizeInitialize()
    {
        LocalizeTable.Initialize(language);
    }

    public void OnClickLocalize()
    {
        var localizeKey = inputFieldLocalizeKey.text;
        var param1 = inputFieldLocalizeParam1.text;
        var param2 = inputFieldLocalizeParam2.text;
        
        txtLocalizeValue.text = localizeKey.GetLocalizeText(param1, param2);
    }
}