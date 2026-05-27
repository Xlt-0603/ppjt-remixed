using UnityEngine;

namespace PPCorps
{
    public class Tower : UnitBase
    {
        public override int OccupiedCols => 0;

        public override void OnBeat(int bar, int beat)
        {
            if (_isDead || data == null) return;

            _currentTarget = FindNearestEnemy();

            if (_currentTarget != null && InAttackRange(_currentTarget))
            {
                if (ShouldAttackOnBeat(beat))
                {
                    _currentAction = UnitAction.Attacking;
                    _currentTarget.TakeDamage(data.attackPower);
                }
                else
                    _currentAction = UnitAction.Idle;
            }
            else
                _currentAction = UnitAction.Idle;
        }
    }
}
