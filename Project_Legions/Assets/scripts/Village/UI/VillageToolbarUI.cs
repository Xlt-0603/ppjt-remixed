using UnityEngine;
using UnityEngine.UI;

namespace PPCorps
{
    public class VillageToolbarUI : MonoBehaviour
    {
        [SerializeField] private Button _combatBtn;
        [SerializeField] private Button _deckBtn;
        [SerializeField] private Button _shopBtn;
        [SerializeField] private Button _mailBtn;

        private void Start()
        {
            if (_combatBtn != null)
                _combatBtn.onClick.AddListener(OnCombatClicked);
            if (_deckBtn != null)
                _deckBtn.onClick.AddListener(OnDeckClicked);
            if (_shopBtn != null)
                _shopBtn.onClick.AddListener(OnShopClicked);
            if (_mailBtn != null)
                _mailBtn.onClick.AddListener(OnMailClicked);
        }

        private void OnCombatClicked()
        {
            VillageManager.Instance?.EnterBattle();
        }

        private void OnDeckClicked()
        {
            Debug.Log("[Toolbar] Deck - not implemented in MVP");
        }

        private void OnShopClicked()
        {
            var vm = VillageManager.Instance;
            if (vm == null) return;
            var shopPanel = vm.PanelRoot?.transform.Find("Panel_Shop")?.gameObject;
            if (shopPanel != null)
                vm.OpenPanel(shopPanel);
        }

        private void OnMailClicked()
        {
            Debug.Log("[Toolbar] Mail - not implemented in MVP");
        }
    }
}
