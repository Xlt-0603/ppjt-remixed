using UnityEngine;

namespace PPCorps
{
    public class DragHandler : MonoBehaviour
    {
        [SerializeField] private GameObject _ghostPrefab;
        [SerializeField] private Color _validColor = new Color(0, 1, 0, 0.6f);
        [SerializeField] private Color _invalidColor = new Color(1, 0, 0, 0.6f);

        private GridHelper _grid;
        private UnitData _currentData;
        private GameObject _ghost;
        private SpriteRenderer _ghostSprite;
        private bool _isDragging;

        private void Start()
        {
            _grid = GetComponent<GridHelper>();
        }

        private void Update()
        {
            if (GameManager.Instance.State != GameState.Deploy)
            {
                if (_isDragging) CancelDrag();
                return;
            }

            if (!_isDragging) return;

            Vector3 gridPos = _grid.ScreenToGridPoint(Input.mousePosition);
            _ghost.transform.position = gridPos;

            bool canPlace = _grid.IsInBounds(gridPos) && !_grid.IsOccupied(gridPos);
            _ghostSprite.color = canPlace ? _validColor : _invalidColor;

            if (Input.GetMouseButtonUp(0))
            {
                if (canPlace)
                    DeploySystem.Instance.PlaceUnit(_currentData, gridPos);
                CancelDrag();
            }

            if (Input.GetMouseButtonDown(1))
                CancelDrag();
        }

        public void StartDrag(UnitData data)
        {
            if (_isDragging) return;
            if (data == null || data.prefab == null) return;

            _currentData = data;
            _ghost = Instantiate(_ghostPrefab);

            _ghostSprite = _ghost.GetComponent<SpriteRenderer>();
            if (_ghostSprite != null)
            {
                SpriteRenderer prefabSprite = data.prefab.GetComponent<SpriteRenderer>();
                if (prefabSprite != null)
                    _ghostSprite.sprite = prefabSprite.sprite;
            }

            _isDragging = true;
        }

        private void CancelDrag()
        {
            if (_ghost != null)
                Destroy(_ghost);
            _isDragging = false;
            _currentData = null;
        }
    }
}
