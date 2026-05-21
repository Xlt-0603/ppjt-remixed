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
        public int attackRange = 1;
        public int moveSpeed = 1;
        public int deployCost = 3;

        [Header("攻击节奏（8拍）")]
        public bool attackOnBeat1 = true;
        public bool attackOnBeat2;
        public bool attackOnBeat3;
        public bool attackOnBeat4;
        public bool attackOnBeat5 = true;
        public bool attackOnBeat6;
        public bool attackOnBeat7;
        public bool attackOnBeat8;

        [Header("动画持续节拍（0=用原有逻辑）")]
        public int attackAnimBeats;
    }
}
