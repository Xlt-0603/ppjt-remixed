using UnityEngine;

namespace PPCorps
{
    public class LevelController : MonoBehaviour
    {
        [SerializeField] private LevelData _levelData;
        [SerializeField] private float _spawnY = -0.5f;

        private int _validEntryCount;
        private int _spawnedCount;
        private int _aliveSpawnedCount;

        private void Start()
        {
            if (_levelData == null)
            {
                Debug.LogWarning("LevelController: 未指定关卡数据");
                return;
            }

            foreach (var entry in _levelData.entries)
            {
                if (entry.enemyData != null && entry.enemyData.prefab != null)
                    _validEntryCount++;
            }

            if (_levelData.bpmOverride > 0)
                GameManager.Instance.BPM = _levelData.bpmOverride;

            GameManager.Instance.OnBeat += OnGameBeat;
        }

        private void OnGameBeat(int bar, int beat)
        {
            if (_levelData == null) return;

            foreach (var entry in _levelData.entries)
            {
                if (entry.enemyData == null) continue;
                if (entry.enemyData.prefab == null) continue;
                if (entry.bar != bar || entry.beat != beat) continue;

                SpawnEnemy(entry);
            }
        }

        private void SpawnEnemy(SpawnEntry entry)
        {
            float x = GridManager.Instance.GridToWorldX(new GridPosition(entry.col));
            Vector3 pos = new Vector3(x, _spawnY, 0);

            GameObject go = Instantiate(entry.enemyData.prefab, pos, Quaternion.identity);

            UnitBase unit = go.GetComponent<UnitBase>();
            if (unit != null)
            {
                SetField(unit, "isEnemy", true);
                SetField(unit, "data", entry.enemyData);

                _spawnedCount++;
                _aliveSpawnedCount++;
                unit.OnUnitDeath += OnSpawnedUnitDeath;
            }
        }

        private void OnSpawnedUnitDeath(UnitBase unit)
        {
            unit.OnUnitDeath -= OnSpawnedUnitDeath;
            _aliveSpawnedCount--;
            TryWinByClear();
        }

        private void TryWinByClear()
        {
            if (!_levelData.clearAllEnemiesToWin) return;
            if (_spawnedCount < _validEntryCount) return;
            if (_aliveSpawnedCount > 0) return;

            GameManager.Instance.ForceWin();
        }

        private static void SetField(object obj, string fieldName, object value)
        {
            var type = obj.GetType();
            System.Reflection.FieldInfo field = null;
            while (type != null && field == null)
            {
                field = type.GetField(fieldName,
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Public);
                type = type.BaseType;
            }
            if (field != null)
                field.SetValue(obj, value);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnBeat -= OnGameBeat;
        }
    }
}
