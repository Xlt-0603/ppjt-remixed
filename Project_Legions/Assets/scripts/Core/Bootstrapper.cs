using System.Linq;
using UnityEngine;

namespace PPCorps
{
    public class Bootstrapper : MonoBehaviour
    {
        [Header("网格设置")]
        [SerializeField] private float _cellSize = 1.5234375f;
        [SerializeField] private Vector2 _gridOrigin = new Vector2(-18.28125f, -4.5f);
        [SerializeField] private int _cols = 24;

        [Header("部署设置")]
        [SerializeField] private float _playerSpawnY = -0.5f;

        [Header("AI卡组")]
        [SerializeField] private AIDeckEntry[] _aiDeck;

        [Header("塔引用（拖入场景中的塔）")]
        [SerializeField] private UnitBase _playerTower;
        [SerializeField] private UnitBase _enemyTower;

        private void Awake()
        {
            SetupGridManager();
            SetupGameManager();
            SetupDeploySystem();
            SetupPlayerDeck();
            SetupDeployUI();
            SetupAIDeck();
            SetupAICommander();
        }

        private void SetupGridManager()
        {
            var gm = FindObjectOfType<GridManager>();
            if (gm == null)
            {
                var go = new GameObject("GridManager");
                gm = go.AddComponent<GridManager>();
            }

            SetField(gm, "_cellSize", _cellSize);
            SetField(gm, "_gridOrigin", _gridOrigin);
            SetField(gm, "_cols", _cols);
        }

        private void SetupGameManager()
        {
            var gm = FindObjectOfType<GameManager>();
            if (gm == null)
            {
                var go = new GameObject("GameManager");
                gm = go.AddComponent<GameManager>();
            }

            if (_playerTower != null)
            {
                var field = typeof(GameManager).GetField("_playerTower",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (field != null) field.SetValue(gm, _playerTower);
            }

            if (_enemyTower != null)
            {
                var field = typeof(GameManager).GetField("_enemyTower",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (field != null) field.SetValue(gm, _enemyTower);
            }
        }

        private void SetupDeploySystem()
        {
            var ds = FindObjectOfType<DeploySystem>();
            if (ds == null)
            {
                var go = new GameObject("DeploySystem");
                ds = go.AddComponent<DeploySystem>();
            }

            SetField(ds, "_playerSpawnY", _playerSpawnY);
        }

        private void SetupPlayerDeck()
        {
            if (FindObjectsOfType<DeckManager>().Any(d => d != null && !d.IsEnemy)) return;

            // auto-create player deck from DeployUI._cards
            var ui = FindObjectOfType<DeployUI>();
            if (ui == null) return;

            var cardsField = typeof(DeployUI).GetField("_cards",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var cards = cardsField?.GetValue(ui) as DeployCardEntry[];
            if (cards == null || cards.Length == 0) return;

            var deckData = cards
                .Where(e => e != null && e.unitData != null)
                .Select(e => e.unitData)
                .ToArray();
            if (deckData.Length == 0) return;

            var go = new GameObject("PlayerDeck");
            var dm = go.AddComponent<DeckManager>();
            SetField(dm, "_deckCards", deckData);
            SetField(dm, "_isEnemy", false);
            SetField(dm, "_handSize", 4);
        }

        private void SetupDeployUI()
        {
            var ui = FindObjectOfType<DeployUI>();
            if (ui == null)
            {
                var go = new GameObject("DeployUI");
                ui = go.AddComponent<DeployUI>();
            }

            if (ui == null) return;

            var playerDeck = FindObjectsOfType<DeckManager>().FirstOrDefault(d => d != null && !d.IsEnemy);
            if (playerDeck != null)
                SetField(ui, "_deckManager", playerDeck);
        }

        private void SetupAIDeck()
        {
            if (_aiDeck == null || _aiDeck.Length == 0) return;
            if (FindObjectsOfType<DeckManager>().Any(d => d != null && d.IsEnemy)) return;
            var unitDataArray = _aiDeck.Select(e => e.unitData).Where(d => d != null).ToArray();
            if (unitDataArray.Length == 0) return;
            var go = new GameObject("AIDeck");
            var dm = go.AddComponent<DeckManager>();
            SetField(dm, "_deckCards", unitDataArray);
            SetField(dm, "_isEnemy", true);
            SetField(dm, "_handSize", 4);
        }

        private void SetupAICommander()
        {
            if (FindObjectOfType<AICommander>() != null) return;
            if (_aiDeck == null || _aiDeck.Length == 0) return;
            var go = new GameObject("AICommander");
            var ai = go.AddComponent<AICommander>();
            SetField(ai, "_deck", _aiDeck);

            var aiDeck = FindObjectsOfType<DeckManager>().FirstOrDefault(d => d != null && d.IsEnemy);
            if (aiDeck != null)
                SetField(ai, "_deckManager", aiDeck);
        }

        private static void SetField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public);
            if (field != null)
                field.SetValue(obj, value);
        }
    }
}
