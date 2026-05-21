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

        private Vector3 _mouseDownPos;

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

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
                _mouseDownPos = Input.mousePosition;

            if (Input.GetMouseButtonUp(0))
            {
                if (Vector3.Distance(_mouseDownPos, Input.mousePosition) > 10f)
                {
                    Debug.Log($"[VillageManager] 拖拽忽略 (移动 {Vector3.Distance(_mouseDownPos, Input.mousePosition):F1}px)");
                    return;
                }

                Camera cam = Camera.main;
                if (cam == null)
                {
                    Debug.LogError("[VillageManager] Camera.main 为空!");
                    return;
                }

                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

                Debug.Log($"[VillageManager] 点击检测: ray origin={ray.origin}, direction={ray.direction}, hit={hit.collider?.name}");

                if (hit.collider != null)
                {
                    Debug.Log($"[VillageManager] 碰撞体: {hit.collider.name} (layer={hit.collider.gameObject.layer})");

                    // 列出该对象上所有组件
                    var comps = hit.collider.GetComponents<Component>();
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    sb.Append("[VillageManager] 组件: ");
                    foreach (var c in comps)
                        sb.Append(c?.GetType().Name ?? "null").Append(" ");
                    Debug.Log(sb.ToString());

                    if (hit.collider.TryGetComponent<VillageBuilding>(out var building))
                    {
                        Debug.Log($"[VillageManager] VillageBuilding 已找到, 调用 NotifyClicked");
                        building.NotifyClicked();
                    }
                    else
                        Debug.LogError($"[VillageManager] 对象上没有 VillageBuilding 组件 (共 {comps.Length} 个组件)");
                }
            }
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
