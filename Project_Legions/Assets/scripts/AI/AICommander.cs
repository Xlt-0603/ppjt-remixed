using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace PPCorps
{
    [System.Serializable]
    public class AIDeckEntry
    {
        public UnitData unitData;
        public int weight = 1;
    }

    public class AICommander : MonoBehaviour
    {
        [Header("卡组")]
        [SerializeField] private AIDeckEntry[] _deck;

        [Header("费用（与玩家相同）")]
        [SerializeField] private int _startEnergy = 3;
        [SerializeField] private int _barsPerRegen = 15;

        [Header("生成位置")]
        [SerializeField] private float _enemySpawnY = -0.5f;

        [Header("部署范围")]
        [SerializeField] private int _placeableColMin;

        private static readonly int[] RegenTargets = { 3, 3, 3, 4, 4, 4, 4, 4, 4, 5, 5, 5, 5, 6, 6, 6, 6 };

        private int _energy;
        private int _regenStep;
        private int _barsSinceLastRegen;
        private List<int> _recentDeployIndices = new List<int>();

        private List<PendingAIDeploy> _pendingDeploys = new List<PendingAIDeploy>();

        private class PendingAIDeploy
        {
            public GridPosition gridPos;
            public UnitData data;
        }

        private void Start()
        {
            _energy = _startEnergy;
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
            {
                ExecutePendingDeploys();
                for (int i = 0; i < 5; i++)
                    TryDeploy();
            }
            else if (state == GameState.Win || state == GameState.Lose)
                _pendingDeploys.Clear();
        }

        private void OnGameBeat(int bar, int beat)
        {
            if (GameManager.Instance.State == GameState.Deploy || GameManager.Instance.State == GameState.Battle)
            {
                if (beat == 1)
                {
                    ExecutePendingDeploys();

                    _barsSinceLastRegen++;
                    if (_barsSinceLastRegen >= _barsPerRegen)
                    {
                        _barsSinceLastRegen = 0;
                        int target = _regenStep < RegenTargets.Length ? RegenTargets[_regenStep] : 6;
                        _energy = target;
                        if (_regenStep < RegenTargets.Length) _regenStep++;
                    }
                }

                TryDeploy();
            }
        }

        private class AffordEntry
        {
            public int index;
            public AIDeckEntry entry;
        }

        private void TryDeploy()
        {
            if (_deck == null || _deck.Length == 0) return;

            var affordable = new List<AffordEntry>();
            for (int i = 0; i < _deck.Length; i++)
            {
                var e = _deck[i];
                if (e != null && e.unitData != null && _energy >= e.unitData.deployCost)
                    affordable.Add(new AffordEntry { index = i, entry = e });
            }

            if (affordable.Count == 0) return;

            int pick = PickUnit(affordable);
            if (pick < 0) return;

            AffordEntry chosen = affordable[pick];
            UnitData data = chosen.entry.unitData;

            GridPosition pos = PickColumn(data);
            if (pos.col < 0) return;

            _energy -= data.deployCost;
            GridManager.Instance.Reserve(pos);

            _pendingDeploys.Add(new PendingAIDeploy
            {
                gridPos = pos,
                data = data
            });

            _recentDeployIndices.Add(chosen.index);
            if (_recentDeployIndices.Count > 5)
                _recentDeployIndices.RemoveAt(0);
        }

        private int PickUnit(List<AffordEntry> affordable)
        {
            int best = -1;
            float bestScore = float.MinValue;

            for (int i = 0; i < affordable.Count; i++)
            {
                int idx = affordable[i].index;
                AIDeckEntry entry = affordable[i].entry;

                int lastUsed = _recentDeployIndices.LastIndexOf(idx);
                int distance = lastUsed >= 0 ? _recentDeployIndices.Count - lastUsed : _recentDeployIndices.Count + 1;

                float diversityBonus = distance * 2f;
                float weightBonus = entry.weight;
                float costBonus = entry.unitData.deployCost <= _energy / 2 ? 3f : 0f;

                float score = diversityBonus + weightBonus + costBonus;
                if (score > bestScore)
                {
                    bestScore = score;
                    best = i;
                }
            }

            return best;
        }

        private GridPosition PickColumn(UnitData data)
        {
            int minCol = GetAIMinDeployCol();
            int maxCol = GetAIMaxDeployCol();
            if (minCol > maxCol) return new GridPosition(-1);

            bool isMelee = data.attackRange <= 1;

            int startCol = isMelee ? minCol : maxCol;
            int step = isMelee ? 1 : -1;

            for (int c = startCol; isMelee ? c <= maxCol : c >= minCol; c += step)
            {
                GridPosition pos = new GridPosition(c);
                if (CanAIPlaceUnit(data, pos))
                    return pos;
            }

            return new GridPosition(-1);
        }

        private int GetAIMinDeployCol()
        {
            int cols = GridManager.Instance.Cols;
            var units = GameManager.Instance.GetAllUnits()
                .Where(u => u != null && !u.IsDead)
                .ToList();

            int frontmostEnemyCol = cols;
            int backmostPlayerCol = -1;

            foreach (var u in units)
            {
                if (u.IsEnemy)
                {
                    if (u.GridPos.col < frontmostEnemyCol)
                        frontmostEnemyCol = u.GridPos.col;
                }
                else
                {
                    if (u.GridPos.col > backmostPlayerCol)
                        backmostPlayerCol = u.GridPos.col;
                }
            }

            int minAllowed;

            if (frontmostEnemyCol <= cols / 2 - 1)
                minAllowed = frontmostEnemyCol + 1;
            else
                minAllowed = cols / 2;

            if (backmostPlayerCol >= 0)
                minAllowed = Mathf.Max(minAllowed, backmostPlayerCol + 1);

            return Mathf.Max(minAllowed, _placeableColMin);
        }

        private int GetAIMaxDeployCol()
        {
            return GridManager.Instance.Cols - 1;
        }

        private bool CanAIPlaceUnit(UnitData data, GridPosition pos)
        {
            if (data == null) return false;
            if (_energy < data.deployCost) return false;
            if (GameManager.Instance.State != GameState.Deploy && GameManager.Instance.State != GameState.Battle) return false;
            if (pos.col < GetAIMinDeployCol() || pos.col > GetAIMaxDeployCol()) return false;
            if (!GridManager.Instance.IsInBounds(pos)) return false;
            if (GridManager.Instance.IsReserved(pos)) return false;
            if (GridManager.Instance.GetOccupants(pos).Count > 0) return false;
            return true;
        }

        private void ExecutePendingDeploys()
        {
            foreach (var pending in _pendingDeploys)
            {
                GridManager.Instance.Unreserve(pending.gridPos);

                float x = GridManager.Instance.GridToWorldX(pending.gridPos);
                Vector3 worldPos = new Vector3(x, _enemySpawnY, 0);
                GameObject go = Instantiate(pending.data.prefab, worldPos, Quaternion.identity);

                UnitBase unit = go.GetComponent<UnitBase>();
                if (unit != null)
                {
                    Vector3 scale = go.transform.localScale;
                    go.transform.localScale = new Vector3(-Mathf.Abs(scale.x), scale.y, scale.z);

                    SetField(unit, "isEnemy", true);
                    SetField(unit, "data", pending.data);
                    SetField(unit, "_gridPos", pending.gridPos);
                    unit.GridInitialized = true;

                    for (int i = 0; i < unit.OccupiedCols; i++)
                        GridManager.Instance.Occupy(pending.gridPos + i, unit);

                    GameManager.Instance.RegisterUnit(unit);
                }
            }
            _pendingDeploys.Clear();
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
