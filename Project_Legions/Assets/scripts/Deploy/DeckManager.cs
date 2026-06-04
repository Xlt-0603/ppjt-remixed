using System.Collections.Generic;
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
            Shuffle();
            _hand = new List<UnitData>(_handSize);
            FillHand();
            _initialized = true;
            OnHandChanged?.Invoke();
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

        public bool CanUseCard(int handIndex)
        {
            if (!_initialized) return false;
            if (handIndex < 0 || handIndex >= _hand.Count) return false;
            return CanUseUnit(_hand[handIndex]);
        }

        public bool CanUseUnit(UnitData data)
        {
            if (data == null || !_initialized) return false;
            foreach (var unit in GameManager.Instance.GetAllUnits())
            {
                if (unit == null || unit.IsDead) continue;
                if (unit.IsEnemy != _isEnemy) continue;
                if (unit.Data == data) return false;
            }
            return true;
        }

        public UnitData UseCard(int handIndex)
        {
            if (!_initialized) return null;
            if (handIndex < 0 || handIndex >= _hand.Count) return null;
            UnitData card = _hand[handIndex];
            if (!CanUseUnit(card)) return null;

            _hand.RemoveAt(handIndex);
            _drawPile.Add(card);
            _hand.Add(DrawFromPile());
            OnHandChanged?.Invoke();
            return card;
        }
    }
}
