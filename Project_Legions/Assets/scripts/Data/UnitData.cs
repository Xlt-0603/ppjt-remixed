using UnityEngine;

namespace PPCorps
{
    [CreateAssetMenu(fileName = "NewUnitData", menuName = "砰砰军团/单位数据")]
    public class UnitData : ScriptableObject
    {
        public string unitName;
        public GameObject prefab;
        public Sprite icon;
        public UnitType unitType;
        public int maxHP = 10;
        public int attackPower = 2;
        public float attackRange = 1f;
        public int attackIntervalInBeats = 4;
        public float moveSpeed = 1f;
        public int deployCost = 3;
    }
}
