using System;
using System.Collections.Generic;
using TableData;
using UnityEngine;

[Serializable]
public partial class CraftingLevelData : IData
{
    public int ID => _ID;
    [SerializeField] private int _ID;

    public int stage => _stage;
    [SerializeField] private int _stage;

    public int step => _step;
    [SerializeField] private int _step;

    public CraftingType type => _type;
    [SerializeField] private CraftingType _type;

    public int start_level => _start_level;
    [SerializeField] private int _start_level;

    public int end_level => _end_level;
    [SerializeField] private int _end_level;

    public double start_cost => _start_cost;
    [SerializeField] private double _start_cost;

    public double end_cost => _end_cost;
    [SerializeField] private double _end_cost;

    public double start_price => _start_price;
    [SerializeField] private double _start_price;

    public double end_price => _end_price;
    [SerializeField] private double _end_price;

    public float craft_time => _craft_time;
    [SerializeField] private float _craft_time;

	public void SetData(List<string> data)
	{
		_ID = int.Parse(data[0]);
		_stage = int.Parse(data[1]);
		_step = int.Parse(data[2]);
		_type = CraftingType.Parse<CraftingType>(data[3]);
		_start_level = int.Parse(data[4]);
		_end_level = int.Parse(data[5]);
		_start_cost = double.Parse(data[6]);
		_end_cost = double.Parse(data[7]);
		_start_price = double.Parse(data[8]);
		_end_price = double.Parse(data[9]);
		_craft_time = float.Parse(data[10]);
	}
}
