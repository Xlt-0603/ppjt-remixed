using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace PPCorps
{
    public class UnitCard : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private Text _nameText;
        [SerializeField] private Text _costText;

        private UnitData _data;
        private DragHandler _dragHandler;

        public void Setup(UnitData data, DragHandler dragHandler)
        {
            _data = data;
            _dragHandler = dragHandler;

            if (_iconImage != null && data.icon != null)
                _iconImage.sprite = data.icon;

            if (_nameText != null)
                _nameText.text = data.unitName;

            if (_costText != null)
                _costText.text = data.deployCost.ToString();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_dragHandler != null && _data != null)
                _dragHandler.StartDrag(_data);
        }
    }
}
