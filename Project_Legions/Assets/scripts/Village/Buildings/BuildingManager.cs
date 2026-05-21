using System.Collections.Generic;
using UnityEngine;

namespace PPCorps
{
    public class BuildingManager : MonoBehaviour
    {
        private Dictionary<BuildingType, VillageBuilding> _buildingMap = new Dictionary<BuildingType, VillageBuilding>();
        private List<VillageBuilding> _allBuildings = new List<VillageBuilding>();

        public IReadOnlyList<VillageBuilding> AllBuildings => _allBuildings;

        private void Start()
        {
            CollectBuildings();
        }

        public void CollectBuildings()
        {
            _buildingMap.Clear();
            _allBuildings.Clear();
            var buildings = GetComponentsInChildren<VillageBuilding>(true);
            foreach (var b in buildings)
            {
                _buildingMap[b.Type] = b;
                _allBuildings.Add(b);
            }
        }

        public VillageBuilding GetBuilding(BuildingType type)
        {
            _buildingMap.TryGetValue(type, out var building);
            return building;
        }

        public bool TryUpgrade(BuildingType type, out int cost)
        {
            cost = 0;
            var building = GetBuilding(type);
            if (building == null) return false;
            var vm = VillageManager.Instance;
            if (vm == null) return false;
            if (!building.CanUpgrade(vm.VillageLevel, vm.Currency.gold))
                return false;
            int idx = building.CurrentLevel - 1;
            cost = building.Data.upgradeCosts[idx];
            vm.SpendGold(cost);
            building.Upgrade();
            return true;
        }
    }
}
