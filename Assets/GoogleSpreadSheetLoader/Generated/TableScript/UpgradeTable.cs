using System.Collections.Generic;
using TableData;
using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeTable", menuName = "Tables/UpgradeTable")]
public partial class UpgradeTable : ScriptableObject, ITable
{
    public List<UpgradeData> dataList = new List<UpgradeData>();

	public void SetData(List<List<string>> data)
	{
		dataList = new List<UpgradeData>();
		foreach (var item in data)
		{
			UpgradeData newData = new();
			newData.SetData(item);
			dataList.Add(newData);
		}
	}
}
