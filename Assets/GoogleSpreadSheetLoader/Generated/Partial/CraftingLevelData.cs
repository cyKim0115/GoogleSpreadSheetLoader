    using System;
    using UnityEngine;

    public partial class CraftingLevelData
    {
        public double GetCost(int level)
        {
            if (level < start_level || level > end_level)
            {
                throw new Exception($"CraftingTableData : 레벨 범위 아님 start({start_level}) {level} end({end_level})");
            }

            if (start_level == end_level)
                return start_cost;
            
            var progress = (float)(level - start_level) / (end_level - start_level);
            var result = (end_cost - start_cost) * progress + start_cost;
            
            return result;
        }
        
        public double GetPrice(int level)
        {
            if (level < start_level || level > end_level)
            {
                throw new Exception($"CraftingTableData : 레벨 범위 아님 start({start_level}) {level} end({end_level})");
            }
            
            if (start_level == end_level)
                return start_price;

            var progress = (float)(level - start_level) / (end_level - start_level);
            var result = (end_price - start_price) * progress + start_price;
            
            return result;
        }
    }