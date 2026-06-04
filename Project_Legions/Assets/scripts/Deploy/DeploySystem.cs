using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace PPCorps
{
    public class DeploySystem : MonoBehaviour
    {
        public static DeploySystem Instance { get; private set; }

        [Header("费用")]
        [SerializeField] private int _startEnergy = 3;

        [Header("回费配置")]
        [SerializeField] private int _barsPerRegen = 15;

        [Header("部署范围")]
        [SerializeField] private float _playerSpawnY = -0.5f;
        [SerializeField] private int _placeableColMin = 0;

        private static readonly int[] RegenTargets = { 3, 3, 3, 4, 4, 4, 4, 4, 4, 5, 5, 5, 5, 6, 6, 6, 6 };

        private class PendingDeploy
        {
            public GridPosition gridPos;
            public UnitData data;
            public GameObject ghost;
        }

        private List<PendingDeploy> _pendingDeploys = new List<PendingDeploy>();
        private int _regenStep;
        private int _barsSinceLastRegen;

        public int Energy { get; private set; }
        public float PlayerSpawnY => _playerSpawnY;
        public int GetPlaceableColMin() => _placeableColMin;

        private void Awake()
        {
            Instance = this;
            Energy = _startEnergy;
        }

        private void Start()
        {
            GameManager.Instance.OnBeat += OnGameBeat;
            GameManager.Instance.OnStateChanged += OnStateChanged;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnBeat -= OnGameBeat;
                GameManager.Instance.OnStateChanged -= OnStateChanged;
            }
        }

        private void OnStateChanged(GameState state)
        {
            if (state == GameState.Battle)
                ExecutePendingDeploys();
            else if (state == GameState.Win || state == GameState.Lose)
                ClearPendingDeploys();
        }

        private void OnGameBeat(int bar, int beat)
        {
            if (GameManager.Instance.State != GameState.Battle) return;

            if (beat == 1)
            {
                ExecutePendingDeploys();

                _barsSinceLastRegen++;
                if (_barsSinceLastRegen >= _barsPerRegen)
                {
                    _barsSinceLastRegen = 0;
                    int target = _regenStep < RegenTargets.Length ? RegenTargets[_regenStep] : 6;
                    Energy = target;
                    if (_regenStep < RegenTargets.Length) _regenStep++;
                }
            }
        }

        public void QueueDeploy(UnitData data, GridPosition pos, GameObject ghost)
        {
            if (!CanPlaceUnit(data, pos)) return;

            Energy -= data.deployCost;
            GridManager.Instance.Reserve(pos);

            _pendingDeploys.Add(new PendingDeploy
            {
                gridPos = pos,
                data = data,
                ghost = ghost
            });

            SpriteRenderer sr = ghost.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = new Color(0.4f, 0.6f, 1f, 0.9f);
        }

        private void ExecutePendingDeploys()
        {
            foreach (var pending in _pendingDeploys)
            {
                GridManager.Instance.Unreserve(pending.gridPos);

                float x = GridManager.Instance.GridToWorldX(pending.gridPos);
                Vector3 worldPos = new Vector3(x, _playerSpawnY, 0);
                GameObject go = Instantiate(pending.data.prefab, worldPos, Quaternion.identity);

                UnitBase unit = go.GetComponent<UnitBase>();
                if (unit != null)
                {
                    SetField(unit, "isEnemy", false);
                    SetField(unit, "data", pending.data);
                    SetField(unit, "_gridPos", pending.gridPos);
                    unit.GridInitialized = true;

                    for (int i = 0; i < unit.OccupiedCols; i++)
                        GridManager.Instance.Occupy(pending.gridPos + i, unit);

                    GameManager.Instance.RegisterUnit(unit);
                }

                if (pending.ghost != null)
                    Destroy(pending.ghost);
            }
            _pendingDeploys.Clear();
        }

        private void ClearPendingDeploys()
        {
            foreach (var pending in _pendingDeploys)
            {
                GridManager.Instance.Unreserve(pending.gridPos);
                if (pending.ghost != null)
                    Destroy(pending.ghost);
            }
            _pendingDeploys.Clear();
        }

        public int GetMaxDeployCol()
        {
            var units = GameManager.Instance.GetAllUnits()
                .Where(u => u != null && !u.IsDead)
                .ToList();

            int cols = GridManager.Instance.Cols;
            int frontmostPlayerCol = -1;
            int backmostEnemyCol = cols;

            foreach (var unit in units)
            {
                int col = unit.GridPos.col;
                if (!unit.IsEnemy)
                {
                    if (col > frontmostPlayerCol)
                        frontmostPlayerCol = col;
                }
                else
                {
                    if (col < backmostEnemyCol)
                        backmostEnemyCol = col;
                }
            }

            int maxAllowed;

            if (frontmostPlayerCol >= cols / 2)
                maxAllowed = frontmostPlayerCol - 1;
            else
                maxAllowed = cols / 2 - 1;

            if (backmostEnemyCol < cols)
                maxAllowed = Mathf.Min(maxAllowed, backmostEnemyCol - 1);

            return maxAllowed;
        }

        public bool CanPlaceUnit(UnitData data, GridPosition pos)
        {
            if (data == null) return false;
            if (Energy < data.deployCost) return false;
            if (GameManager.Instance.State != GameState.Deploy && GameManager.Instance.State != GameState.Battle) return false;
            if (pos.col < _placeableColMin || pos.col > GetMaxDeployCol()) return false;
            if (!GridManager.Instance.IsInBounds(pos)) return false;
            if (GridManager.Instance.IsReserved(pos)) return false;
            if (GridManager.Instance.GetOccupants(pos).Count > 0) return false;
            if (IsPlayerUnitOnField(data)) return false;
            return true;
        }

        private bool IsPlayerUnitOnField(UnitData data)
        {
            foreach (var unit in GameManager.Instance.GetAllUnits())
            {
                if (unit == null || unit.IsDead || unit.IsEnemy) continue;
                if (unit.Data == data) return true;
            }
            return false;
        }

        private static void SetField(object obj, string fieldName, object value)
        {
            var type = obj.GetType();
            FieldInfo field = null;
            while (type != null && field == null)
            {
                field = type.GetField(fieldName,
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                type = type.BaseType;
            }
            if (field != null)
                field.SetValue(obj, value);
        }
    }
}
