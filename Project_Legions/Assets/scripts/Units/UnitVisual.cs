using UnityEngine;

namespace PPCorps
{
    [RequireComponent(typeof(UnitBase))]
    public class UnitVisual : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private UnitBase _unit;

        private void Start()
        {
            _unit = GetComponent<UnitBase>();
        }

        private void Update()
        {
            if (_unit == null || _unit.IsDead) return;

            if (_unit.IsMoving)
            {
                float elapsed = Time.time - _unit.MoveStartTime;
                float barDuration = 60f / GameManager.Instance.BPM;
                float t = Mathf.Clamp01(elapsed / barDuration);
                t = Mathf.SmoothStep(0, 1, t);
                transform.position = Vector3.Lerp(_unit.MoveFrom, _unit.MoveTo, t);
            }

            if (_spriteRenderer != null)
                _spriteRenderer.flipX = _unit.IsEnemy;
        }
    }
}
