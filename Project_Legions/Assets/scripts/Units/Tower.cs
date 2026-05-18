using UnityEngine;

namespace PPCorps
{
    public class Tower : UnitBase
    {
        public override void OnBeat(int bar, int beat)
        {
            if (_isDead || data == null) return;

            _currentTarget = FindNearestEnemy();

            if (_currentTarget != null && InAttackRange(_currentTarget))
            {
                TryAttack(_currentTarget);
            }
            else
            {
                SetAction(UnitAction.Idle);
            }
        }
    }
}
