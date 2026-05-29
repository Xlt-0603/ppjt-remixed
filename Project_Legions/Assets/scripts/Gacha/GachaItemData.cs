using UnityEngine;

namespace PPCorps
{
    [CreateAssetMenu(fileName = "NewGachaItem", menuName = "砰砰军团/抽卡物品")]
    public class GachaItemData : ScriptableObject
    {
        public string itemName;
        public Sprite icon;
        public Rarity rarity;
        public bool isUp;
        public int maxCopies = 12;
    }

    public enum Rarity
    {
        白,
        蓝,
        金,
        彩
    }
}
