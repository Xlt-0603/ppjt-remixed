using System.Collections.Generic;
using UnityEngine;

namespace PPCorps
{
    [CreateAssetMenu(fileName = "NewBanner", menuName = "砰砰军团/卡池配置")]
    public class GachaBanner : ScriptableObject
    {
        public string bannerName;

        [Header("UP角色")]
        public GachaItemData[] up彩;
        public GachaItemData[] up金;

        [Header("常驻角色")]
        public GachaItemData[] 常驻彩;
        public GachaItemData[] 常驻金;
        public GachaItemData[] 常驻蓝;
        public GachaItemData[] 常驻白;

        public List<GachaItemData> GetAllAvailable(Rarity rarity, bool isUp)
        {
            var list = new List<GachaItemData>();
            if (isUp)
            {
                if (rarity == Rarity.彩) AddAll(list, up彩);
                if (rarity == Rarity.金) AddAll(list, up金);
            }
            else
            {
                if (rarity == Rarity.彩) AddAll(list, 常驻彩);
                if (rarity == Rarity.金) AddAll(list, 常驻金);
                if (rarity == Rarity.蓝) AddAll(list, 常驻蓝);
                if (rarity == Rarity.白) AddAll(list, 常驻白);
            }
            return list;
        }

        private void AddAll(List<GachaItemData> list, GachaItemData[] arr)
        {
            if (arr == null) return;
            foreach (var item in arr)
                if (item != null) list.Add(item);
        }
    }
}
