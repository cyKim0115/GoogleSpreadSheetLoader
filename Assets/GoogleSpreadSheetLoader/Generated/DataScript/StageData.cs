using System;
using System.Collections.Generic;
using TableData;
using UnityEngine;

[Serializable]
public partial class StageData : IData
{
    public int ID => _ID;
    [SerializeField] private int _ID;

    public StageType type => _type;
    [SerializeField] private StageType _type;

    public int group => _group;
    [SerializeField] private int _group;

    public int level => _level;
    [SerializeField] private int _level;

    public string prefab_name => _prefab_name;
    [SerializeField] private string _prefab_name;

    public string color => _color;
    [SerializeField] private string _color;

	public void SetData(List<string> data)
	{
		_ID = int.Parse(data[0]);
		_type = StageType.Parse<StageType>(data[1]);
		_group = int.Parse(data[2]);
		_level = int.Parse(data[3]);
		_prefab_name = data[4].ToString();
		_color = data[5].ToString();
	}
}
