using UnityEngine;

namespace PPCorps
{
    public class GuardController : MonoBehaviour
    {
        [SerializeField] private int _pauseBeats = 8;

        private UnitBase _unit;
        private bool _wasMoving;
        private int _moveCountdown;
        private int _pauseCountdown;
        private bool _inPause;

        private void Awake()
        {
            _unit = GetComponent<UnitBase>();
            if (_unit == null)
            {
                enabled = false;
                return;
            }
            GameManager.Instance.OnBeat += OnGameBeat;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnBeat -= OnGameBeat;
        }

        private void OnGameBeat(int bar, int beat)
        {
            if (_unit == null || _unit.IsDead) return;

            bool nowMoving = _unit.IsMoving;

            // detect movement START (event fires BEFORE UnitBase.OnBeat, so nowMoving is stale)
            if (!_wasMoving && nowMoving)
            {
                _moveCountdown = 8;
            }
            _wasMoving = nowMoving;

            // count down movement
            if (_moveCountdown > 0)
            {
                _moveCountdown--;
                if (_moveCountdown <= 0)
                {
                    // movement finished → start guard pause
                    _inPause = true;
                    _pauseCountdown = _pauseBeats;
                    _unit.BlockMovement = true;
                }
            }

            // count down pause
            if (_inPause && _moveCountdown <= 0)
            {
                _pauseCountdown--;
                if (_pauseCountdown <= 0)
                {
                    _inPause = false;
                    _unit.BlockMovement = false;
                }
            }
        }
    }
}
