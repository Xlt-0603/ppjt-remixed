using UnityEngine;

namespace PPCorps
{
    public class UnitBase : MonoBehaviour
    {
        [SerializeField] protected UnitData data;
        [SerializeField] protected bool isEnemy;
        [SerializeField] protected Vector2 defaultMoveDirection = Vector2.zero;

        protected int _currentHP;
        protected int _attackCooldown;
        protected UnitBase _currentTarget;
        protected bool _isDead;
        protected UnitAction _currentAction = UnitAction.Idle;

        public bool IsEnemy => isEnemy;
        public bool IsDead => _isDead;
        public int CurrentHP => _currentHP;
        public int MaxHP => data != null ? data.maxHP : 1;
        public UnitAction CurrentAction => _currentAction;
        public UnitBase CurrentTarget => _currentTarget;
        public Vector3 TargetPosition { get; set; }

        protected virtual void Start()
        {
            _currentHP = data != null ? data.maxHP : 1;

            if (defaultMoveDirection == Vector2.zero)
                defaultMoveDirection = isEnemy ? Vector2.left : Vector2.right;

            if (GameManager.Instance != null)
                GameManager.Instance.RegisterUnit(this);

            if (GetComponent<UnitHPBar>() == null)
                gameObject.AddComponent<UnitHPBar>();

            TargetPosition = transform.position;
        }

        public virtual void OnBeat(int bar, int beat)
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
                    MoveOneStepTowards(_currentTarget.TargetPosition);
                else
                {
                    TargetPosition += (Vector3)defaultMoveDirection * data.moveSpeed;
                    _currentAction = UnitAction.Moving;
                }
            }
            else
            {
                _currentAction = UnitAction.Idle;
            }
        }

        protected virtual void TryAttack(UnitBase target)
        {
            if (_attackCooldown > 0)
            {
                _attackCooldown--;
                _currentAction = UnitAction.Idle;
                return;
            }

            _currentAction = UnitAction.Attacking;
            target.TakeDamage(data.attackPower);
            _attackCooldown = data.attackIntervalInBeats;
        }

        public virtual void TakeDamage(int damage)
        {
            if (_isDead) return;
            _currentHP -= damage;
            if (_currentHP <= 0) Die();
        }

        protected virtual void Die()
        {
            _isDead = true;
            _currentAction = UnitAction.Dead;

            if (GameManager.Instance != null)
                GameManager.Instance.UnregisterUnit(this);

            Destroy(gameObject);
        }

        protected bool InAttackRange(UnitBase target)
        {
            if (target == null) return false;
            float dist = Vector3.Distance(TargetPosition, target.TargetPosition);
            return dist <= data.attackRange;
        }

        protected UnitBase FindNearestEnemy()
        {
            UnitBase nearest = null;
            float minDist = float.MaxValue;

            var allUnits = GameManager.Instance.GetAllUnits();
            foreach (var unit in allUnits)
            {
                if (unit == null || unit == this || unit.IsDead) continue;
                if (unit.IsEnemy == isEnemy) continue;

                float dist = Vector3.Distance(TargetPosition, unit.TargetPosition);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = unit;
                }
            }

            return nearest;
        }

        protected void MoveOneStepTowards(Vector3 target)
        {
            if (data == null) return;

            _currentAction = UnitAction.Moving;
            Vector3 dir = (target - TargetPosition).normalized;
            TargetPosition += dir * data.moveSpeed;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.UnregisterUnit(this);
        }
    }
}
