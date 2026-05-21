using UnityEngine;

namespace PPCorps
{
    public class VillageDataManager : MonoBehaviour
    {
        private const string SAVE_KEY = "VillageSaveData";

        public VillageSaveData CurrentData { get; private set; }

        private void Awake()
        {
            CurrentData = new VillageSaveData();
        }

        public void SaveData()
        {
            var vm = VillageManager.Instance;
            if (vm == null) return;

            CurrentData.villageLevel = vm.VillageLevel;
            CurrentData.gold = vm.Currency.gold;
            CurrentData.gems = vm.Currency.gems;
            CurrentData.foodJade = vm.Currency.foodJade;
            CurrentData.techPoints = vm.Currency.techPoints;

            var bm = vm.GetComponentInChildren<BuildingManager>();
            if (bm != null)
            {
                CurrentData.buildings.Clear();
                foreach (var b in bm.AllBuildings)
                {
                    CurrentData.buildings.Add(new BuildingSaveData
                    {
                        type = b.Type,
                        level = b.CurrentLevel
                    });
                }
            }

            string json = JsonUtility.ToJson(CurrentData);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
        }

        public void LoadData()
        {
            if (!PlayerPrefs.HasKey(SAVE_KEY))
            {
                CurrentData = new VillageSaveData();
                return;
            }

            string json = PlayerPrefs.GetString(SAVE_KEY);
            CurrentData = JsonUtility.FromJson<VillageSaveData>(json);
            if (CurrentData == null)
                CurrentData = new VillageSaveData();

            var vm = VillageManager.Instance;
            if (vm == null) return;

            vm.SetCurrency(CurrentData.gold, CurrentData.gems, CurrentData.foodJade, CurrentData.techPoints);
            vm.SetLevel(CurrentData.villageLevel);

            var bm = vm.GetComponentInChildren<BuildingManager>();
            if (bm != null)
            {
                foreach (var save in CurrentData.buildings)
                {
                    var building = bm.GetBuilding(save.type);
                    if (building != null)
                        building.Init(building.Data, save.level);
                }
            }
        }
    }
}
