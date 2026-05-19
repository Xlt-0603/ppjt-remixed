using UnityEngine;

namespace PPCorps
{
    public class DeployPanel : MonoBehaviour
    {
        [SerializeField] private UnitData[] _availableUnits;
        [SerializeField] private GameObject _cardPrefab;
        [SerializeField] private Transform _cardContainer;

        private DragHandler _dragHandler;

        private void Start()
        {
            _dragHandler = FindObjectOfType<DragHandler>();
            CreateCards();
        }

        private void CreateCards()
        {
            if (_cardPrefab == null || _cardContainer == null) return;

            foreach (UnitData data in _availableUnits)
            {
                if (data == null) continue;

                GameObject card = Instantiate(_cardPrefab, _cardContainer);
                UnitCard cardScript = card.GetComponent<UnitCard>();
                if (cardScript != null)
                    cardScript.Setup(data, _dragHandler);
            }
        }
    }
}
