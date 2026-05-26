using UnityEngine;

namespace PPCorps
{
    public class LevelController : MonoBehaviour
    {
        [SerializeField] private LevelData _levelData;
        [SerializeField] private float _spawnY = -0.5f;

        private void Start()
        {
            if (_levelData == null)
            {
                Debug.LogWarning("LevelController: 未指定关卡数据");
                return;
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
                System.Reflection.FieldInfo field = null;
                var type = unit.GetType();
                while (type != null && field == null)
                {
                    field = type.GetField("isEnemy",
                        System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Public);
                    type = type.BaseType;
                }
                if (field != null)
                    field.SetValue(unit, true);
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnBeat -= OnGameBeat;
        }
    }
}
