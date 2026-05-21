using UnityEngine;

namespace PPCorps
{
    [CreateAssetMenu(fileName = "NewBuildingData", menuName = "砰砰军团/建筑数据")]
    public class BuildingDataSO : ScriptableObject
    {
        public string buildingName;
        public BuildingType buildingType;
        public Sprite icon;

        [Header("外观")]
        public Sprite[] levelSprites;

        [Header("升级条件")]
        public int[] upgradeCosts;
        public int[] upgradeLevelRequirements;
        public string[] unlockDescriptions;

        [Header("位置")]
        public Vector2 worldPosition;

        public int MaxLevel => levelSprites != null ? levelSprites.Length : 1;
    }
}
