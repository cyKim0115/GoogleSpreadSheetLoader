using System.Collections.Generic;
using TableData;
using UnityEngine;

[CreateAssetMenu(fileName = "CraftingLevelTable", menuName = "Tables/CraftingLevelTable")]
public partial class CraftingLevelTable : ScriptableObject, ITable
{
    public List<CraftingLevelData> dataList = new List<CraftingLevelData>();

	public void SetData(List<List<string>> data)
	{
		dataList = new List<CraftingLevelData>();
		foreach (var item in data)
		{
			CraftingLevelData newData = new();
			newData.SetData(item);
			dataList.Add(newData);
		}
	}
}
