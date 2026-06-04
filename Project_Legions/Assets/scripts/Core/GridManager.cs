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
        private HashSet<int> _reservedCells = new HashSet<int>();

        private void Awake() => Instance = this;

        /// <summary>返回格子中心的 X 坐标</summary>
        public float GridToWorldX(GridPosition pos)
        {
            return _gridOrigin.x + pos.col * _cellSize + _cellSize * 0.5f;
        }

        public GridPosition WorldToGrid(Vector3 worldPos)
        {
            float c = (worldPos.x - _gridOrigin.x) / _cellSize - 0.5f;
            int col = Mathf.RoundToInt(c);
            col = Mathf.Clamp(col, 0, _cols - 1);
            return new GridPosition(col);
        }

        public bool IsInBounds(GridPosition pos)
            => pos.col >= 0 && pos.col < _cols;

        public int Cols => _cols;
        public float CellSize => _cellSize;

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

        public void Reserve(GridPosition pos) => _reservedCells.Add(pos.col);

        public void Unreserve(GridPosition pos) => _reservedCells.Remove(pos.col);

        public bool IsReserved(GridPosition pos) => _reservedCells.Contains(pos.col);

        public bool CanOccupy(GridPosition pos, UnitBase unit)
        {
            if (!IsInBounds(pos)) return false;

            var list = GetOccupants(pos);
            if (list.Count == 0) return true;

            bool hasNonTower = list.Any(u => !(u is Tower));
            if (!(unit is Tower) && hasNonTower) return false;

            if (!(unit is Tower) && list.Any(u => u is Tower && u.IsEnemy != unit.IsEnemy))
                return false;

            return true;
        }
    }
}
