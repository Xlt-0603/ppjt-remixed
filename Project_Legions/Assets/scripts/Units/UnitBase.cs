using UnityEngine;

namespace PPCorps
{
    public class UnitBase : MonoBehaviour
    {
        [SerializeField] protected UnitData data;
        [SerializeField] protected bool isEnemy;
        [SerializeField] protected Vector2 defaultMoveDirection = Vector2.zero;
        [SerializeField] private Animator _animator;

        protected int _currentHP;
        protected int _attackCooldown;
        protected UnitBase _currentTarget;
        protected bool _isDead;
        private UnitAction _currentAction = UnitAction.Idle;

        public bool IsEnemy => isEnemy;
        public bool IsDead => _isDead;
        public int CurrentHP => _currentHP;
        public int MaxHP => data != null ? data.maxHP : 1;
        public UnitAction CurrentAction => _currentAction;
        public UnitBase CurrentTarget => _currentTarget;
        public Vector3 LogicalPosition { get; protected set; }
        public float FacingDirection { get; protected set; }

        public event System.Action<UnitAction> OnActionChanged;

        private bool _hasVisual;

        private void Awake()
        {
            LogicalPosition = transform.position;
        }

        protected virtual void Start()
        {
            _currentHP = data != null ? data.maxHP : 1;
            FacingDirection = isEnemy ? -1f : 1f;

            if (defaultMoveDirection == Vector2.zero)
                defaultMoveDirection = isEnemy ? Vector2.left : Vector2.right;

            if (_animator == null)
                _animator = GetComponent<Animator>();

            _hasVisual = GetComponent<UnitVisual>() != null;

            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.flipX = isEnemy;

            if (GameManager.Instance != null)
                GameManager.Instance.RegisterUnit(this);

            if (GetComponent<UnitHPBar>() == null)
                gameObject.AddComponent<UnitHPBar>();

            SyncAnimator();
        }

        private void Update()
        {
            if (_isDead) return;
            if (!_hasVisual && transform.position != LogicalPosition)
                transform.position = LogicalPosition;
        }

        protected void SetAction(UnitAction action)
        {
            if (_currentAction == action) return;
            _currentAction = action;
            OnActionChanged?.Invoke(action);
            SyncAnimator();
        }

        private void SyncAnimator()
        {
            if (_animator != null && _animator.isActiveAndEnabled)
                _animator.SetInteger("Action", (int)_currentAction);
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
                    MoveOneStepTowards(_currentTarget.LogicalPosition);
                else
                {
                    LogicalPosition += (Vector3)defaultMoveDirection * data.moveSpeed;
                    FacingDirection = defaultMoveDirection.x;
                    SetAction(UnitAction.Moving);
                }
            }
            else
            {
                SetAction(UnitAction.Idle);
            }
        }

        protected virtual void TryAttack(UnitBase target)
        {
            if (_attackCooldown > 0)
            {
                _attackCooldown--;
                SetAction(UnitAction.Idle);
                return;
            }

            SetAction(UnitAction.Attacking);
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
            SetAction(UnitAction.Dead);
            if (GameManager.Instance != null)
                GameManager.Instance.UnregisterUnit(this);
            Destroy(gameObject);
        }

        protected bool InAttackRange(UnitBase target)
        {
            if (target == null) return false;
            float dist = Vector3.Distance(LogicalPosition, target.LogicalPosition);
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
                float dist = Vector3.Distance(LogicalPosition, unit.LogicalPosition);
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
            SetAction(UnitAction.Moving);
            Vector3 dir = (target - LogicalPosition).normalized;
            FacingDirection = dir.x;
            LogicalPosition += dir * data.moveSpeed;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.UnregisterUnit(this);
        }
    }
}
