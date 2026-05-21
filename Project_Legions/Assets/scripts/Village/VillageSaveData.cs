using System;
using System.Collections.Generic;
using UnityEngine;

namespace PPCorps
{
    public enum BuildingType
    {
        CombatCenter,
        Canteen,
        Fishing,
        ResearchLab,
        Shop,
        VillageCenter
    }

    public enum DecorationLayer
    {
        Foreground,
        Midground,
        Background
    }

    public enum VillageState
    {
        Idle,
        InPanel,
        Decorating
    }

    [Serializable]
    public class VillageSaveData
    {
        public int villageLevel = 1;
        public int gold = 500;
        public int gems = 100;
        public int foodJade = 0;
        public int techPoints = 0;
        public List<BuildingSaveData> buildings = new List<BuildingSaveData>();
        public List<DecorationSaveData> decorations = new List<DecorationSaveData>();
        public List<string> ownedBeanIds = new List<string>();
    }

    [Serializable]
    public class BuildingSaveData
    {
        public BuildingType type;
        public int level = 1;
    }

    [Serializable]
    public class DecorationSaveData
    {
        public string itemId;
        public DecorationLayer layer;
        public float posX;
        public float posY;
    }

    [Serializable]
    public class CurrencyData
    {
        public int gold;
        public int gems;
        public int foodJade;
        public int techPoints;
    }
}
