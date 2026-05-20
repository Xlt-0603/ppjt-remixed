using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PPCorps
{
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }

        [SerializeField] private float _cellSize = 1.3333f;
        [SerializeField] private Vector2 _gridOrigin;
        [SerializeField] private int _cols = 24;

        private Dictionary<int, List<UnitBase>> _occupants = new Dictionary<int, List<UnitBase>>();

        private void Awake() => Instance = this;

        public Vector3 GridToWorld(GridPosition pos)
        {
            return new Vector3(
                _gridOrigin.x + pos.col * _cellSize,
                _gridOrigin.y,
                0
            );
        }

        public GridPosition WorldToGrid(Vector3 worldPos)
        {
            int col = Mathf.RoundToInt((worldPos.x - _gridOrigin.x) / _cellSize);
            col = Mathf.Clamp(col, 0, _cols - 1);
            return new GridPosition(col);
        }

        public bool IsInBounds(GridPosition pos)
            => pos.col >= 0 && pos.col < _cols;

        public int Cols => _cols;

        public bool IsTowerCell(GridPosition pos)
            => pos.col == 2 || pos.col == 3 || pos.col == 20 || pos.col == 21;

        public void Occupy(GridPosition pos, UnitBase unit)
        {
            if (!_occupants.ContainsKey(pos.col))
                _occupants[pos.col] = new List<UnitBase>();
            if (!_occupants[pos.col].Contains(unit))
                _occupants[pos.col].Add(unit);
        }

        public void Leave(GridPosition pos, UnitBase unit)
        {
            if (_occupants.ContainsKey(pos.col))
                _occupants[pos.col].Remove(unit);
        }

        public List<UnitBase> GetOccupants(GridPosition pos)
        {
            if (_occupants.ContainsKey(pos.col))
                return _occupants[pos.col];
            return new List<UnitBase>();
        }

        public bool CanOccupy(GridPosition pos, UnitBase unit)
        {
            if (!IsInBounds(pos)) return false;

            var list = GetOccupants(pos);
            if (list.Count == 0) return true;

            if (IsTowerCell(pos))
            {
                bool hasTower = list.Any(u => u is Tower);
                bool isTower = unit is Tower;

                if (hasTower && isTower) return false;
                if (list.Count >= 2) return false;
                return true;
            }

            return false;
        }
    }
}
