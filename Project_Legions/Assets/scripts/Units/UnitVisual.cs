using UnityEngine;

namespace PPCorps
{
    [RequireComponent(typeof(UnitBase))]
    public class UnitVisual : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField][Range(1f, 30f)] private float _smoothSpeed = 12f;

        private UnitBase _unit;

        private void Start()
        {
            _unit = GetComponent<UnitBase>();
        }

        private void Update()
        {
            if (_unit == null || _unit.IsDead) return;

            transform.position = Vector3.Lerp(
                transform.position,
                _unit.LogicalPosition,
                _smoothSpeed * Time.deltaTime
            );

            if (_spriteRenderer != null)
            {
                float facing = _unit.FacingDirection;
                if (Mathf.Abs(facing) > 0.01f)
                    _spriteRenderer.flipX = facing < 0;
            }
        }
    }
}
