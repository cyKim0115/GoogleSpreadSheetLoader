using System.Collections.Generic;
using TableData;
using UnityEngine;

[CreateAssetMenu(fileName = "DefaultValueTable", menuName = "Tables/DefaultValueTable")]
public partial class DefaultValueTable : ScriptableObject, ITable
{
    public List<DefaultValueData> dataList = new List<DefaultValueData>();

	public void SetData(List<List<string>> data)
	{
		dataList = new List<DefaultValueData>();
		foreach (var item in data)
		{
			DefaultValueData newData = new();
			newData.SetData(item);
			dataList.Add(newData);
		}
	}
}
