using UnityEngine;

namespace PPCorps
{
    [RequireComponent(typeof(UnitBase))]
    public class UnitVisual : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Animator _animator;
        [SerializeField][Range(1f, 30f)] private float _smoothSpeed = 12f;

        private UnitBase _unit;

        private void Start()
        {
            _unit = GetComponent<UnitBase>();
            transform.position = _unit.LogicalPosition;
            _unit.OnActionChanged += OnUnitActionChanged;
        }

        private void OnDestroy()
        {
            if (_unit != null)
                _unit.OnActionChanged -= OnUnitActionChanged;
        }

        private void Update()
        {
            if (_unit == null || _unit.IsDead) return;

            transform.position = Vector3.Lerp(
                transform.position,
                _unit.LogicalPosition,
                _smoothSpeed * Time.deltaTime
            );

            UpdateFacing();
        }

        private void OnUnitActionChanged(UnitAction action)
        {
            if (_animator == null) return;

            switch (action)
            {
                case UnitAction.Moving:
                    _animator.SetBool("IsMoving", true);
                    break;
                case UnitAction.Attacking:
                    _animator.SetBool("IsMoving", false);
                    _animator.SetTrigger("Attack");
                    break;
                case UnitAction.Dead:
                    _animator.SetTrigger("Die");
                    break;
                default:
                    _animator.SetBool("IsMoving", false);
                    break;
            }
        }

        private void UpdateFacing()
        {
            if (_spriteRenderer == null) return;
            float facing = _unit.FacingDirection;
            if (Mathf.Abs(facing) > 0.01f)
                _spriteRenderer.flipX = facing < 0;
        }
    }
}
