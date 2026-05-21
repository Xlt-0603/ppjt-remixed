using UnityEngine;
using UnityEngine.UI;

namespace PPCorps
{
    public class CanteenPanelUI : BuildingPanelUI
    {
        [SerializeField] private Button _closeBtn;

        private void Start()
        {
            if (_closeBtn != null)
                _closeBtn.onClick.AddListener(OnCloseClicked);
        }
    }
}
