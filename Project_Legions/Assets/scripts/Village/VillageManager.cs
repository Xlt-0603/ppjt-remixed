using System;
using UnityEngine;

namespace PPCorps
{
    public class VillageManager : MonoBehaviour
    {
        public static VillageManager Instance { get; private set; }

        [Header("数据")]
        [SerializeField] private CurrencyData _currency = new CurrencyData();
        [SerializeField] private int _villageLevel = 1;

        [Header("场景引用")]
        [SerializeField] private BuildingManager _buildingManager;
        [SerializeField] private VillageDataManager _dataManager;

        [Header("UI 引用")]
        [SerializeField] private ResourceBarUI _resourceBar;
        [SerializeField] private VillageToolbarUI _toolbar;
        [SerializeField] private GameObject _panelRoot;

        public int VillageLevel => _villageLevel;
        public CurrencyData Currency => _currency;
        public VillageState State { get; private set; }
        public GameObject PanelRoot => _panelRoot;

        public event Action<CurrencyData> OnCurrencyChanged;

        private void Awake()
        {
            Instance = this;
            State = VillageState.Idle;

            if (_buildingManager == null)
                _buildingManager = GetComponentInChildren<BuildingManager>();
            if (_dataManager == null)
                _dataManager = GetComponent<VillageDataManager>();
            if (_resourceBar == null)
                _resourceBar = FindObjectOfType<ResourceBarUI>();
            if (_toolbar == null)
                _toolbar = FindObjectOfType<VillageToolbarUI>();
        }

        private void Start()
        {
            if (_dataManager != null)
                _dataManager.LoadData();
            RefreshUI();
        }

        public void RefreshUI()
        {
            _resourceBar?.Refresh(_currency);
        }

        public void OpenPanel(GameObject panel)
        {
            CloseAllPanels();
            if (panel != null)
            {
                panel.SetActive(true);
                State = VillageState.InPanel;
            }
        }

        public void CloseAllPanels()
        {
            if (_panelRoot == null) return;
            for (int i = 0; i < _panelRoot.transform.childCount; i++)
                _panelRoot.transform.GetChild(i).gameObject.SetActive(false);
            State = VillageState.Idle;
        }

        public bool SpendGold(int amount)
        {
            if (_currency.gold < amount) return false;
            _currency.gold -= amount;
            OnCurrencyChanged?.Invoke(_currency);
            RefreshUI();
            _dataManager?.SaveData();
            return true;
        }

        public bool SpendGems(int amount)
        {
            if (_currency.gems < amount) return false;
            _currency.gems -= amount;
            OnCurrencyChanged?.Invoke(_currency);
            RefreshUI();
            _dataManager?.SaveData();
            return true;
        }

        public void AddGold(int amount)
        {
            _currency.gold += amount;
            OnCurrencyChanged?.Invoke(_currency);
            RefreshUI();
            _dataManager?.SaveData();
        }

        public void SetCurrency(int gold, int gems, int foodJade, int techPoints)
        {
            _currency.gold = gold;
            _currency.gems = gems;
            _currency.foodJade = foodJade;
            _currency.techPoints = techPoints;
        }

        public void SetLevel(int level)
        {
            _villageLevel = level;
        }

        public void EnterBattle()
        {
            _dataManager?.SaveData();
            UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
        }
    }
}
