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
        public UnitClass unitClass;
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

        [Header("动画起止拍（0=该拍无独立动画）")]
        public int animStartBeat1;
        public int animEndBeat1;
        public int animStartBeat2;
        public int animEndBeat2;
        public int animStartBeat3;
        public int animEndBeat3;
        public int animStartBeat4;
        public int animEndBeat4;
        public int animStartBeat5;
        public int animEndBeat5;
        public int animStartBeat6;
        public int animEndBeat6;
        public int animStartBeat7;
        public int animEndBeat7;
        public int animStartBeat8;
        public int animEndBeat8;

        [Header("抛投")]
        [Tooltip("勾选后优先攻击攻击范围内最远的敌人，而非最近的")]
        public bool preferFarthestTarget;

        [Header("先锋")]
        public bool isVanguard;
        public int vanguardScanMin = 2;
        public int vanguardScanMax = 5;
        public int vanguardDamage = 3;
        public int vanguardChargeBeats = 8;
    }
}
