using UnityEngine;

namespace PPCorps
{
    [RequireComponent(typeof(UnitBase))]
    public class UnitVisual : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private UnitBase _unit;
        private Vector3 _prevFrom;
        private Vector3 _prevTo;
        private float _animStartTime;

        private void Start()
        {
            _unit = GetComponent<UnitBase>();
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

            if (_spriteRenderer != null)
                _spriteRenderer.flipX = _unit.IsEnemy;
        }
    }
}
