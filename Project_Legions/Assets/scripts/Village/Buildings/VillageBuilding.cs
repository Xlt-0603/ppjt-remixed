using System;
using UnityEngine;

namespace PPCorps
{
    public class VillageBuilding : MonoBehaviour
    {
        [SerializeField] private BuildingDataSO _data;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private int _currentLevel = 1;
        [SerializeField] private string _panelName;

        public BuildingType Type => _data != null ? _data.buildingType : BuildingType.VillageCenter;
        public int CurrentLevel => _currentLevel;
        public int MaxLevel => _data != null ? _data.MaxLevel : 1;
        public BuildingDataSO Data => _data;

        public event Action<VillageBuilding> OnClicked;
        public event Action<VillageBuilding, int> OnLevelChanged;

        private void Awake()
        {
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            OnClicked += b =>
            {
                Debug.Log($"[村庄] 点击了 {b.name}");
                if (!string.IsNullOrEmpty(_panelName))
                {
                    var mgr = VillageManager.Instance;
                    if (mgr != null && mgr.PanelRoot != null)
                    {
                        var panel = mgr.PanelRoot.transform.Find(_panelName)?.gameObject;
                        if (panel != null)
                            mgr.OpenPanel(panel);
                    }
                }
            };
        }

        public void NotifyClicked()
        {
            OnClicked?.Invoke(this);
            StartCoroutine(ClickPopAnimation());
        }

        private System.Collections.IEnumerator ClickPopAnimation()
        {
            float duration = 0.06f;
            Vector3 from = Vector3.one;
            Vector3 to = Vector3.one * 1.05f;

            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                transform.localScale = Vector3.Lerp(from, to, t / duration);
                yield return null;
            }
            transform.localScale = to;

            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                transform.localScale = Vector3.Lerp(to, from, t / duration);
                yield return null;
            }
            transform.localScale = from;
        }

        public void Init(BuildingDataSO data, int level)
        {
            _data = data;
            _currentLevel = Mathf.Clamp(level, 1, data.MaxLevel);
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();
            UpdateVisual();
        }

        public bool CanUpgrade(int villageLevel, int gold)
        {
            if (_data == null) return false;
            int idx = _currentLevel - 1;
            if (idx >= _data.upgradeCosts.Length - 1) return false;
            return villageLevel >= _data.upgradeLevelRequirements[idx + 1]
                && gold >= _data.upgradeCosts[idx + 1];
        }

        public void Upgrade()
        {
            if (_currentLevel >= MaxLevel) return;
            _currentLevel++;
            UpdateVisual();
            OnLevelChanged?.Invoke(this, _currentLevel);
        }

        private void UpdateVisual()
        {
            if (_spriteRenderer == null || _data == null) return;
            int idx = Mathf.Clamp(_currentLevel - 1, 0, _data.levelSprites.Length - 1);
            if (_data.levelSprites[idx] != null)
                _spriteRenderer.sprite = _data.levelSprites[idx];
        }
    }
}
