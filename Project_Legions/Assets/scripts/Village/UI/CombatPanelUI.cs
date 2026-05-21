using UnityEngine;
using UnityEngine.UI;

namespace PPCorps
{
    public class CombatPanelUI : BuildingPanelUI
    {
        [SerializeField] private Button _pvpBtn;
        [SerializeField] private Button _pveBtn;
        [SerializeField] private Button _backBtn;

        private void Start()
        {
            if (_pvpBtn != null)
                _pvpBtn.onClick.AddListener(OnPVPClicked);
            if (_pveBtn != null)
                _pveBtn.onClick.AddListener(OnPVEClicked);
            if (_backBtn != null)
                _backBtn.onClick.AddListener(OnCloseClicked);
        }

        private void OnPVPClicked()
        {
            VillageManager.Instance?.EnterBattle();
        }

        private void OnPVEClicked()
        {
            Debug.Log("[CombatPanel] PvE - not implemented in MVP");
        }
    }
}
