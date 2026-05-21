using UnityEngine;

namespace PPCorps
{
    public class UnitBase : MonoBehaviour
    {
        [SerializeField] protected UnitData data;
        [SerializeField] protected bool isEnemy;
        [SerializeField] protected Vector2 defaultMoveDirection = Vector2.zero;

        protected int _currentHP;
        protected UnitBase _currentTarget;
        protected bool _isDead;
        protected UnitAction _currentAction = UnitAction.Idle;
        protected bool _isMoving;
        protected GridPosition _gridPos;
        protected Vector3 _moveFrom;
        protected Vector3 _moveTo;
        protected int _moveStartBar;
        protected int _moveStartBeat;

        public bool IsEnemy => isEnemy;
        public bool IsDead => _isDead;
        public int CurrentHP => _currentHP;
        public int MaxHP => data != null ? data.maxHP : 1;
        public UnitAction CurrentAction => _currentAction;
        public UnitBase CurrentTarget => _currentTarget;
        public GridPosition GridPos => _gridPos;
        public bool IsMoving => _isMoving;
        public Vector3 MoveFrom => _moveFrom;
        public Vector3 MoveTo => _moveTo;
        public int MoveStartBar => _moveStartBar;
        public int MoveStartBeat => _moveStartBeat;
        public virtual int OccupiedCols => 1;

        protected virtual void Start()
        {
            _currentHP = data != null ? data.maxHP : 1;

            if (defaultMoveDirection == Vector2.zero)
                defaultMoveDirection = isEnemy ? Vector2.left : Vector2.right;

            if (GameManager.Instance != null)
                GameManager.Instance.RegisterUnit(this);

            if (GetComponent<UnitHPBar>() == null)
                gameObject.AddComponent<UnitHPBar>();

            _gridPos = GridManager.Instance.WorldToGrid(transform.position);
            for (int i = 0; i < OccupiedCols; i++)
                GridManager.Instance.Occupy(_gridPos + i, this);
        }

        public virtual void OnBeat(int bar, int beat)
        {
            if (_isDead || data == null) return;

            if (_isMoving)
            {
                int elapsed = (GameManager.Instance.Bar - _moveStartBar) * 8
                            + (GameManager.Instance.Beat - _moveStartBeat);
                if (elapsed >= 8)
                {
                    _isMoving = false;
                }
                else
                    return;
            }

            if (beat == 1)
            {
                _currentTarget = FindNearestEnemy();

                if (_currentTarget != null && InAttackRange(_currentTarget))
                    _isMoving = false;
                else
                {
                    TryMove();
                    return;
                }
            }

            if (!ShouldAttackOnBeat(beat))
            {
                _currentAction = UnitAction.Idle;
                return;
            }

            if (_currentTarget == null || _currentTarget._isDead || !InAttackRange(_currentTarget))
                _currentTarget = FindNearestEnemy();

            if (_currentTarget != null && InAttackRange(_currentTarget))
            {
                _currentAction = UnitAction.Attacking;
                _currentTarget.TakeDamage(data.attackPower);
            }
            else
                _currentAction = UnitAction.Idle;
        }

        protected bool ShouldAttackOnBeat(int beat)
        {
            if (data == null) return false;
            switch (beat)
            {
                case 1: return data.attackOnBeat1;
                case 2: return data.attackOnBeat2;
                case 3: return data.attackOnBeat3;
                case 4: return data.attackOnBeat4;
                case 5: return data.attackOnBeat5;
                case 6: return data.attackOnBeat6;
                case 7: return data.attackOnBeat7;
                case 8: return data.attackOnBeat8;
                default: return false;
            }
        }

        protected void TryMove()
        {
            int dir = isEnemy ? -1 : 1;
            GridPosition dest = _gridPos + (dir * (int)Mathf.Max(1, data.moveSpeed));

            if (!GridManager.Instance.IsInBounds(dest))
            {
                _currentAction = UnitAction.Idle;
                return;
            }

            if (!GridManager.Instance.CanOccupy(dest, this))
            {
                var blockers = GridManager.Instance.GetOccupants(dest);
                if (blockers.Count > 0 && blockers[0].isEnemy != isEnemy)
                {
                    _currentTarget = blockers[0];
                    _currentAction = UnitAction.Attacking;
                    _currentTarget.TakeDamage(data.attackPower);
                }
                return;
            }

            for (int i = 0; i < OccupiedCols; i++)
                GridManager.Instance.Leave(_gridPos + i, this);
            _gridPos = dest;
            for (int i = 0; i < OccupiedCols; i++)
                GridManager.Instance.Occupy(_gridPos + i, this);

            _moveFrom = transform.position;
            _moveTo = new Vector3(
                GridManager.Instance.GridToWorldX(_gridPos),
                transform.position.y, 0
            );
            _moveStartBar = GameManager.Instance.Bar;
            _moveStartBeat = GameManager.Instance.Beat;
            _isMoving = true;
            _currentAction = UnitAction.Moving;
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
            for (int i = 0; i < OccupiedCols; i++)
                GridManager.Instance.Leave(_gridPos + i, this);

            if (GameManager.Instance != null)
                GameManager.Instance.UnregisterUnit(this);

            Destroy(gameObject);
        }

        protected bool InAttackRange(UnitBase target)
        {
            if (target == null) return false;
            for (int i = 0; i < OccupiedCols; i++)
            {
                if (GridPosition.Distance(_gridPos + i, target._gridPos) <= data.attackRange)
                    return true;
            }
            return false;
        }

        protected UnitBase FindNearestEnemy()
        {
            UnitBase nearest = null;
            int minDist = int.MaxValue;

            var allUnits = GameManager.Instance.GetAllUnits();
            foreach (var unit in allUnits)
            {
                if (unit == null || unit == this || unit._isDead) continue;
                if (unit.isEnemy == isEnemy) continue;

                int dist = GridPosition.Distance(_gridPos, unit._gridPos);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = unit;
                }
            }

            return nearest;
        }

        private void OnDestroy()
        {
            if (!_isDead)
                for (int i = 0; i < OccupiedCols; i++)
                    GridManager.Instance?.Leave(_gridPos + i, this);
            if (GameManager.Instance != null)
                GameManager.Instance.UnregisterUnit(this);
        }
    }
}
