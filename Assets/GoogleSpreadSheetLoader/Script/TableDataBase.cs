using System;
using System.Collections.Generic;
using UnityEngine;

namespace TableData
{
    [Serializable]
    public abstract class TableDataBase : ScriptableObject
    {
        public virtual void SetData(List<string> data)
        {
            
        }
    }
}