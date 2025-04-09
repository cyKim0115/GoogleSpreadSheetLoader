using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace TableData
{
    [CreateAssetMenu(fileName = "TableLinker", menuName = "Tables/TableLinker")]
    public partial class TableLinker : ScriptableObject
    {
		 public CraftingLevelTable CraftingLevelTable;
		 public DefaultValueTable DefaultValueTable;
		 public StageTable StageTable;
		 public UpgradeTable UpgradeTable;

    }
}