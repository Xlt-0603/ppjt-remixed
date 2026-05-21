using UnityEngine;

namespace PPCorps
{
    [RequireComponent(typeof(UnitBase))]
    public class UnitVisual : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private UnitBase _unit;
        private Animator _animator;
        private Vector3 _prevFrom;
        private Vector3 _prevTo;
        private float _animStartTime;
        private bool _wasMoving;
        private Sprite _defaultSprite;
        private int _lastAction = -1;

        private static readonly int ActionParam = Animator.StringToHash("Action");

        private void Start()
        {
            _unit = GetComponent<UnitBase>();
            _animator = GetComponent<Animator>();
            if (_spriteRenderer != null)
                _defaultSprite = _spriteRenderer.sprite;
        }

        private void Update()
        {
            if (_unit == null || _unit.IsDead) return;

            if (_unit.IsMoving)
            {
                if (_unit.MoveFrom != _prevFrom || _unit.MoveTo != _prevTo)
                {
                    _prevFrom = _unit.MoveFrom;
                    _prevTo = _unit.MoveTo;
                    _animStartTime = Time.time;
                }

                float barDuration = 60f / GameManager.Instance.BPM;
                float elapsed = Time.time - _animStartTime;
                float t = Mathf.Clamp01(elapsed / barDuration);
                t = Mathf.SmoothStep(0, 1, t);
                transform.position = Vector3.Lerp(_unit.MoveFrom, _unit.MoveTo, t);
            }
            else if (_wasMoving)
            {
                transform.position = _unit.MoveTo;
            }

            _wasMoving = _unit.IsMoving;

            int currentAction = (int)_unit.CurrentAction;
            if (_animator != null && _animator.runtimeAnimatorController != null)
            {
                if (currentAction != _lastAction)
                {
                    _animator.SetInteger(ActionParam, currentAction);
                    _lastAction = currentAction;
                }
            }

            if (_spriteRenderer != null)
            {
                _spriteRenderer.flipX = _unit.IsEnemy;
                if (_spriteRenderer.sprite == null && _defaultSprite != null)
                    _spriteRenderer.sprite = _defaultSprite;
            }
        }
    }
}
