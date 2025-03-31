using System;
using System.Collections.Generic;
using TableData;
using UnityEngine;

[Serializable]
public partial class UpgradeData : IData
{
    public int ID => _ID;
    [SerializeField] private int _ID;

    public int stage => _stage;
    [SerializeField] private int _stage;

    public int idx => _idx;
    [SerializeField] private int _idx;

    public UpgradeType type => _type;
    [SerializeField] private UpgradeType _type;

    public int value => _value;
    [SerializeField] private int _value;

    public string option => _option;
    [SerializeField] private string _option;

    public double cost => _cost;
    [SerializeField] private double _cost;

    public string icon => _icon;
    [SerializeField] private string _icon;

	public void SetData(List<string> data)
	{
		_ID = int.Parse(data[0]);
		_stage = int.Parse(data[1]);
		_idx = int.Parse(data[2]);
		_type = UpgradeType.Parse<UpgradeType>(data[3]);
		_value = int.Parse(data[4]);
		_option = data[5].ToString();
		_cost = double.Parse(data[6]);
		_icon = data[7].ToString();
	}
}
