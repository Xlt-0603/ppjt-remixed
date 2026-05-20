using UnityEngine;

namespace PPCorps
{
    [RequireComponent(typeof(UnitBase))]
    public class UnitVisual : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private float _smoothTime = 0.04f;

        private UnitBase _unit;
        private Vector3 _velocity;

        private void Start()
        {
            _unit = GetComponent<UnitBase>();
        }

        private void Update()
        {
            if (_unit == null || _unit.IsDead) return;

            transform.position = Vector3.SmoothDamp(
                transform.position,
                _unit.TargetPosition,
                ref _velocity,
                _smoothTime
            );

            if (_spriteRenderer != null)
            {
                float facing = Mathf.Sign(_unit.TargetPosition.x - transform.position.x);
                if (Mathf.Abs(facing) > 0.01f)
                    _spriteRenderer.flipX = facing < 0;
            }
        }
    }
}
