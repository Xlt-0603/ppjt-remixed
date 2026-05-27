using System.Linq;
using System.Reflection;
using UnityEngine;

namespace PPCorps
{
    public class DeploySystem : MonoBehaviour
    {
        public static DeploySystem Instance { get; private set; }

        [Header("费用")]
        [SerializeField] private int _energyPerBar = 1;
        [SerializeField] private int _startEnergy = 3;

        [Header("部署范围")]
        [SerializeField] private float _playerSpawnY = -0.5f;
        [SerializeField] private int _placeableColMin = 0;

        public int Energy { get; private set; }
        public float PlayerSpawnY => _playerSpawnY;

        private void Awake()
        {
            Instance = this;
            Energy = _startEnergy;
        }

        private void Start()
        {
            GameManager.Instance.OnBeat += OnGameBeat;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnBeat -= OnGameBeat;
        }

        private void OnGameBeat(int bar, int beat)
        {
            if (GameManager.Instance.State != GameState.Battle) return;
            if (beat == 1)
                Energy += _energyPerBar;
        }

        public int GetMaxDeployCol()
        {
            var units = GameManager.Instance.GetAllUnits()
                .Where(u => u != null && !u.IsDead)
                .ToList();

            int frontmostPlayerCol = -1;
            int backmostEnemyCol = 24;

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

            if (frontmostPlayerCol >= 12)
                maxAllowed = frontmostPlayerCol - 1;
            else
                maxAllowed = 11;

            if (backmostEnemyCol < 24)
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
            if (GridManager.Instance.GetOccupants(pos).Count > 0) return false;
            return true;
        }

        public void PlaceUnit(UnitData data, GridPosition pos)
        {
            if (!CanPlaceUnit(data, pos)) return;

            Energy -= data.deployCost;

            float x = GridManager.Instance.GridToWorldX(pos);
            Vector3 worldPos = new Vector3(x, _playerSpawnY, 0);
            GameObject go = Instantiate(data.prefab, worldPos, Quaternion.identity);

            UnitBase unit = go.GetComponent<UnitBase>();
            if (unit != null)
            {
                SetField(unit, "isEnemy", false);
                SetField(unit, "data", data);
            }
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
