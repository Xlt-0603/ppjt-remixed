using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PPCorps
{
    public class DeckManager : MonoBehaviour
    {
        [SerializeField] private UnitData[] _deckCards;
        [SerializeField] private bool _isEnemy;
        [SerializeField] private int _handSize = 4;

        private List<UnitData> _drawPile;
        private List<UnitData> _hand;
        private List<UnitData> _inPlay;
        private bool _initialized;

        public IReadOnlyList<UnitData> Hand => _hand;
        public bool IsEnemy => _isEnemy;

        public event System.Action OnHandChanged;

        private void Start()
        {
            if (_deckCards == null || _deckCards.Length == 0)
            {
                Debug.LogError($"[DeckManager {(IsEnemy ? "AI" : "Player")}] _deckCards is empty");
                return;
            }
            _drawPile = new List<UnitData>(_deckCards);
            _inPlay = new List<UnitData>();
            Shuffle();
            _hand = new List<UnitData>(_handSize);
            FillHand();
            _initialized = true;
            OnHandChanged?.Invoke();
            GameManager.Instance.OnBeat += OnGameBeat;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnBeat -= OnGameBeat;
        }

        private void OnGameBeat(int bar, int beat)
        {
            if (!_initialized) return;

            // return cards whose unit has died
            bool returned = false;
            for (int i = _inPlay.Count - 1; i >= 0; i--)
            {
                if (!IsUnitAlive(_inPlay[i]))
                {
                    _drawPile.Add(_inPlay[i]);
                    _inPlay.RemoveAt(i);
                    returned = true;
                }
            }
            if (returned)
            {
                FillHand();
                OnHandChanged?.Invoke();
            }
        }

        private bool IsUnitAlive(UnitData data)
        {
            foreach (var unit in GameManager.Instance.GetAllUnits())
            {
                if (unit == null || unit.IsDead) continue;
                if (unit.IsEnemy != _isEnemy) continue;
                if (unit.Data == data) return true;
            }
            return false;
        }

        private void Shuffle()
        {
            for (int i = _drawPile.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                UnitData tmp = _drawPile[i];
                _drawPile[i] = _drawPile[j];
                _drawPile[j] = tmp;
            }
        }

        private void FillHand()
        {
            while (_hand.Count < _handSize && _drawPile.Count > 0)
                _hand.Add(DrawFromPile());
        }

        private UnitData DrawFromPile()
        {
            if (_drawPile == null || _drawPile.Count == 0) return null;
            UnitData top = _drawPile[0];
            _drawPile.RemoveAt(0);
            return top;
        }

        public UnitData UseCard(int handIndex)
        {
            if (!_initialized) return null;
            if (handIndex < 0 || handIndex >= _hand.Count) return null;
            UnitData card = _hand[handIndex];

            _hand.RemoveAt(handIndex);
            _inPlay.Add(card);
            UnitData next = DrawFromPile();
            if (next != null)
                _hand.Add(next);
            OnHandChanged?.Invoke();
            return card;
        }
    }
}
