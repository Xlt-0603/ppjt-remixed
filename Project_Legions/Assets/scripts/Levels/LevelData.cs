using System;
using UnityEngine;

namespace PPCorps
{
    [CreateAssetMenu(fileName = "NewLevel", menuName = "砰砰军团/关卡数据")]
    public class LevelData : ScriptableObject
    {
        public string levelName;

        [Header("覆盖全局 BPM（0=使用默认）")]
        public float bpmOverride;

        [Header("出场表")]
        public SpawnEntry[] entries;
    }

    [Serializable]
    public struct SpawnEntry
    {
        public UnitData enemyData;

        [Header("出场时机")]
        [Range(1, 99)] public int bar;
        [Range(1, 8)] public int beat;

        [Header("出场位置（网格列 0~23）")]
        [Range(0, 23)] public int col;
    }
}
