using System;
using UnityEngine;

namespace PPCorps
{
    public class VillageBuilding : MonoBehaviour
    {
        [SerializeField] private BuildingDataSO _data;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private int _currentLevel = 1;

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

        private void OnMouseDown()
        {
            OnClicked?.Invoke(this);
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
