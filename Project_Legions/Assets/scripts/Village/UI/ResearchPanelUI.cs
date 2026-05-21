using UnityEngine;
using UnityEngine.UI;

namespace PPCorps
{
    public class ResearchPanelUI : BuildingPanelUI
    {
        [SerializeField] private Button _closeBtn;

        private void Start()
        {
            if (_closeBtn != null)
                _closeBtn.onClick.AddListener(OnCloseClicked);
        }
    }
}
