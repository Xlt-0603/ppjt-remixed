using UnityEngine;

namespace PPCorps
{
    public class BuildingPanelUI : MonoBehaviour
    {
        [SerializeField] protected GameObject _panelObject;

        public virtual void Open()
        {
            if (_panelObject != null)
                _panelObject.SetActive(true);
        }

        public virtual void Close()
        {
            if (_panelObject != null)
                _panelObject.SetActive(false);
        }

        public void OnCloseClicked()
        {
            VillageManager.Instance?.CloseAllPanels();
        }
    }
}
