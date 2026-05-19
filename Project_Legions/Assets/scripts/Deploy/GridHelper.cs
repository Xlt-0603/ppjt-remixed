using UnityEngine;

namespace PPCorps
{
    public class GridHelper : MonoBehaviour
    {
        [SerializeField] private float _tileSize = 1.17f;
        [SerializeField] private Vector2 _gridOrigin;
        [SerializeField] private int _gridCols = 12;
        [SerializeField] private int _gridRows = 4;

        public Vector3 ScreenToGridPoint(Vector2 screenPos)
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10));

            int col = Mathf.RoundToInt((worldPos.x - _gridOrigin.x) / _tileSize);
            int row = Mathf.RoundToInt((worldPos.y - _gridOrigin.y) / _tileSize);

            col = Mathf.Clamp(col, 0, _gridCols - 1);
            row = Mathf.Clamp(row, 0, _gridRows - 1);

            return new Vector3(
                _gridOrigin.x + col * _tileSize,
                _gridOrigin.y + row * _tileSize,
                0
            );
        }

        public bool IsInBounds(Vector3 gridPos)
        {
            int col = Mathf.RoundToInt((gridPos.x - _gridOrigin.x) / _tileSize);
            int row = Mathf.RoundToInt((gridPos.y - _gridOrigin.y) / _tileSize);
            return col >= 0 && col < _gridCols && row >= 0 && row < _gridRows;
        }

        public bool IsOccupied(Vector3 gridPos)
        {
            var units = GameManager.Instance.GetAllUnits();
            foreach (var unit in units)
            {
                if (unit == null || unit.IsDead) continue;
                if (Vector3.Distance(unit.LogicalPosition, gridPos) < _tileSize * 0.4f)
                    return true;
            }
            return false;
        }
    }
}
