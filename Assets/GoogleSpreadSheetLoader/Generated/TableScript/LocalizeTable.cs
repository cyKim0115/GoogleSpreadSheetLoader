using System.Collections.Generic;
using TableData;
using UnityEngine;

[CreateAssetMenu(fileName = "LocalizeTable", menuName = "Tables/LocalizeTable")]
public partial class LocalizeTable : ScriptableObject, ITable
{
    public List<LocalizeData> dataList = new List<LocalizeData>();

	public void SetData(List<List<string>> data)
	{
		dataList = new List<LocalizeData>();
		foreach (var item in data)
		{
			LocalizeData newData = new();
			newData.SetData(item);
			dataList.Add(newData);
		}
	}
}
