using UnityEngine;

namespace PPCorps
{
    public class EnemyBase : UnitBase
    {
        [SerializeField] private Vector2 _moveDirection = Vector2.left;

        protected override void Start()
        {
            base.Start();
        }

        public override void OnBeat(int bar, int beat)
        {
            if (_isDead || data == null) return;

            _currentTarget = FindNearestEnemy();

            if (_currentTarget != null && InAttackRange(_currentTarget))
            {
                TryAttack(_currentTarget);
            }
            else if (beat == 1)
            {
                if (_currentTarget != null)
                    MoveOneStepTowards(_currentTarget.transform.position);
                else
                {
                    transform.position += (Vector3)_moveDirection * data.moveSpeed;
                    _currentAction = UnitAction.Moving;
                }
            }
            else
            {
                _currentAction = UnitAction.Idle;
            }
        }
    }
}
