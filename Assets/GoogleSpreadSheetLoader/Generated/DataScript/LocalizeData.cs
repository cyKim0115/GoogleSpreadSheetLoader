using System;
using System.Collections.Generic;
using TableData;
using UnityEngine;

[Serializable]
public partial class LocalizeData : IData
{
    public string ID => _ID;
    [SerializeField] private string _ID;

    public string kr => _kr;
    [SerializeField] private string _kr;

	public void SetData(List<string> data)
	{
		_ID = data[1].ToString();
		_kr = data[2].ToString();
	}
}
