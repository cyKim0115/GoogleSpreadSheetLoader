using System;
using System.Collections.Generic;
using TableData;
using UnityEngine;

[Serializable]
public partial class DefaultValueData : IData
{
    public string ID => _ID;
    [SerializeField] private string _ID;

    public string value => _value;
    [SerializeField] private string _value;

	public void SetData(List<string> data)
	{
		_ID = data[1].ToString();
		_value = data[2].ToString();
	}
}
