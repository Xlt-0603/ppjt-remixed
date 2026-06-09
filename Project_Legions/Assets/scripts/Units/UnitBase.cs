using System;
using UnityEngine;

namespace PPCorps
{
    public class UnitBase : MonoBehaviour
    {
        [SerializeField] protected UnitData data;
        [SerializeField] protected bool isEnemy;
        [SerializeField] protected Vector2 defaultMoveDirection = Vector2.zero;
        [SerializeField] protected Animator _animator;
        [SerializeField] private GameObject _hitEffectPrefab;
        [SerializeField] private GameObject _deathEffectPrefab;
        [SerializeField] private GameObject _deployEffectPrefab;
        [SerializeField] private GameObject _bulletPrefab;
        [SerializeField] private GameObject _hitSparkPrefab;

        protected int _currentHP;
        protected bool _justArrived;
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
        public bool GridInitialized { get; set; }
        public bool SuppressNormalBehavior { get; set; }
        public bool BlockMovement { get; set; }
        public UnitData Data => data;

        public void SetGridPosition(GridPosition pos)
        {
            for (int i = 0; i < OccupiedCols; i++)
                GridManager.Instance.Leave(_gridPos + i, this);
            _gridPos = pos;
            for (int i = 0; i < OccupiedCols; i++)
                GridManager.Instance.Occupy(_gridPos + i, this);
            transform.position = new Vector3(GridManager.Instance.GridToWorldX(_gridPos), transform.position.y, 0);
        }

        public void SetAction(UnitAction action)
        {
            _currentAction = action;
            SyncAnimator();
        }

        public void ForceMove(GridPosition dest)
        {
            for (int i = 0; i < OccupiedCols; i++)
                GridManager.Instance.Leave(_gridPos + i, this);
            _gridPos = dest;
            for (int i = 0; i < OccupiedCols; i++)
                GridManager.Instance.Occupy(_gridPos + i, this);

            _moveFrom = transform.position;
            _moveTo = new Vector3(GridManager.Instance.GridToWorldX(_gridPos), transform.position.y, 0);
            _moveStartBar = GameManager.Instance.Bar;
            _moveStartBeat = GameManager.Instance.Beat;
            _isMoving = true;
            _currentAction = UnitAction.Moving;
            SyncAnimator();
        }

        public event Action<UnitBase> OnUnitDeath;
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

            if (_animator == null)
                _animator = GetComponent<Animator>();

            if (!GridInitialized)
            {
                _gridPos = GridManager.Instance.WorldToGrid(transform.position);
                for (int i = 0; i < OccupiedCols; i++)
                    GridManager.Instance.Occupy(_gridPos + i, this);
                transform.position = new Vector3(GridManager.Instance.GridToWorldX(_gridPos), transform.position.y, 0);
            }
            SyncAnimator();
        }

        public virtual void OnBeat(int bar, int beat)
        {
            if (_isDead || data == null) return;

            _justArrived = false;

            if (_isMoving)
            {
                int elapsed = (GameManager.Instance.Bar - _moveStartBar) * 8
                            + (GameManager.Instance.Beat - _moveStartBeat);
                if (elapsed >= 8)
                {
                    _isMoving = false;
                    _justArrived = true;
                }
                else
                    return;
            }

            if (SuppressNormalBehavior)
            {
                SyncAnimator();
                return;
            }

            _currentTarget = data.preferFarthestTarget ? FindFarthestInRangeEnemy() : FindNearestEnemy();

            if (beat == 6)
            {
                bool inCombat = _currentTarget != null && InAttackRange(_currentTarget);
                if (!inCombat && !BlockMovement)
                {
                    TryMove();
                    return;
                }
                _isMoving = false;
            }

            if (ShouldAttackOnBeat(beat))
            {
                if (_currentTarget != null && InAttackRange(_currentTarget))
                {
                    if (_justArrived)
                    {
                        _currentAction = UnitAction.Idle;
                    }
                    else
                    {
                        _currentAction = UnitAction.Attacking;
                        if (_animator != null)
                            _animator.Play("\u5251\u58EB\u6345", 0, 0f);
                        SpawnBulletEffect(_currentTarget.transform.position);
                        _currentTarget.TakeDamage(data.attackPower);
                    }
                }
                else
                {
                    _currentAction = UnitAction.Idle;
                }
                SyncAnimator();
                return;
            }

            if (IsAnimatingOnBeat(beat))
            {
                _currentAction = UnitAction.Attacking;
                SyncAnimator();
                return;
            }

            _currentAction = UnitAction.Idle;
            SyncAnimator();
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

        private int AnimStartForBeat(int beat)
        {
            if (data == null) return 0;
            switch (beat)
            {
                case 1: return data.animStartBeat1;
                case 2: return data.animStartBeat2;
                case 3: return data.animStartBeat3;
                case 4: return data.animStartBeat4;
                case 5: return data.animStartBeat5;
                case 6: return data.animStartBeat6;
                case 7: return data.animStartBeat7;
                case 8: return data.animStartBeat8;
                default: return 0;
            }
        }

        private int AnimEndForBeat(int beat)
        {
            if (data == null) return 0;
            switch (beat)
            {
                case 1: return data.animEndBeat1;
                case 2: return data.animEndBeat2;
                case 3: return data.animEndBeat3;
                case 4: return data.animEndBeat4;
                case 5: return data.animEndBeat5;
                case 6: return data.animEndBeat6;
                case 7: return data.animEndBeat7;
                case 8: return data.animEndBeat8;
                default: return 0;
            }
        }

        private bool IsAnimatingOnBeat(int beat)
        {
            for (int b = 1; b <= 8; b++)
            {
                if (!ShouldAttackOnBeat(b)) continue;
                int start = AnimStartForBeat(b);
                int end = AnimEndForBeat(b);
                if (start == 0) continue;
                if (start <= end)
                {
                    if (beat >= start && beat <= end) return true;
                }
                else
                {
                    if (beat >= start || beat <= end) return true;
                }
            }
            return false;
        }

        protected void TryMove()
        {
            int dir = isEnemy ? -1 : 1;
            GridPosition dest = _gridPos + (dir * (int)Mathf.Max(1, data.moveSpeed));

            if (!GridManager.Instance.IsInBounds(dest))
            {
                _currentAction = UnitAction.Idle;
                SyncAnimator();
                return;
            }

            if (!GridManager.Instance.CanOccupy(dest, this))
            {
                var blockers = GridManager.Instance.GetOccupants(dest);
                if (blockers.Count > 0 && blockers[0].isEnemy != isEnemy)
                {
                    _currentTarget = blockers[0];
                    _currentAction = UnitAction.Attacking;
                    if (_animator != null)
                        _animator.Play("\u5251\u58EB\u6345", 0, 0f);
                    _currentTarget.TakeDamage(data.attackPower);
                    SyncAnimator();
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
            SyncAnimator();
        }

        public virtual void TakeDamage(int damage)
        {
            if (_isDead) return;
            _currentHP -= damage;
            SpawnHitEffect(damage);
            if (_currentHP <= 0) Die();
        }

        private void SpawnHitEffect(int damage)
        {
            if (_hitEffectPrefab == null) return;
            GameObject fx = Instantiate(_hitEffectPrefab, transform.position, Quaternion.identity);
            fx.transform.SetParent(null);

            // face particle toward the attacker
            // player unit (isEnemy=false): attacker from right → Y=0 (right)
            // enemy unit (isEnemy=true): attacker from left → Y=180 (left)
            float baseYRot = isEnemy ? 180f : 0f;
            fx.transform.rotation = Quaternion.Euler(0, baseYRot, UnityEngine.Random.Range(-15f, 15f));

            // more damage = bigger blood spray
            float scaleMult = Mathf.Clamp(1f + (damage - 1) * 0.15f, 0.5f, 3f);
            fx.transform.localScale = Vector3.one * scaleMult;

            ParticleSystem ps = fx.GetComponentInChildren<ParticleSystem>();
            if (ps != null)
            {
                ParticleSystem.MainModule main = ps.main;
                main.loop = false;
                float dur = ps.main.duration + ps.main.startLifetime.constantMax;
                Destroy(fx, dur);
            }
            else
            {
                Destroy(fx, 1f);
            }
        }

        private void SpawnDeathEffect()
        {
            if (_deathEffectPrefab == null) return;
            GameObject fx = Instantiate(_deathEffectPrefab, transform.position, Quaternion.identity);
            fx.transform.SetParent(null);
            float baseYRot = isEnemy ? 180f : 0f;
            fx.transform.rotation = Quaternion.Euler(0, baseYRot, 0);
            ParticleSystem ps = fx.GetComponentInChildren<ParticleSystem>();
            if (ps != null)
            {
                ParticleSystem.MainModule main = ps.main;
                main.loop = false;
                float dur = ps.main.duration + ps.main.startLifetime.constantMax;
                Destroy(fx, dur);
            }
            else
            {
                Destroy(fx, 1f);
            }
        }

        public void PlayDeployEffect()
        {
            if (_deployEffectPrefab == null) return;
            GameObject fx = Instantiate(_deployEffectPrefab, transform.position, Quaternion.identity);
            fx.transform.SetParent(null);
            ParticleSystem ps = fx.GetComponentInChildren<ParticleSystem>();
            if (ps != null)
            {
                ParticleSystem.MainModule main = ps.main;
                main.loop = false;
                float dur = ps.main.duration + ps.main.startLifetime.constantMax;
                Destroy(fx, dur);
            }
            else
            {
                Destroy(fx, 1f);
            }
        }

        private static Material _bulletMat;

        private static Material GetOrCreateParticleMat()
        {
            Shader shader = Shader.Find("Particles/Alpha Blended");
            if (shader == null) shader = Shader.Find("Particles/Additive");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            return shader != null ? new Material(shader) : null;
        }

        public void SpawnBulletEffect(Vector3 targetPos)
        {
            Vector3 fromPos = transform.position + new Vector3(isEnemy ? -0.3f : 0.3f, 0f, 0f);
            Vector3 dir = (targetPos - fromPos).normalized;
            float dist = Vector3.Distance(fromPos, targetPos);
            float speed = 25f;

            ParticleSystem ps = null;
            GameObject bulletGo = null;
            float lifeTime = 0.3f;

            if (_bulletPrefab != null)
            {
                bulletGo = Instantiate(_bulletPrefab, fromPos, Quaternion.identity);
                bulletGo.transform.SetParent(null);
                ps = bulletGo.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    ParticleSystem.MainModule m = ps.main;
                    lifeTime = Mathf.Max(dist / m.startSpeed.constant, 0.05f);
                    m.startLifetime = lifeTime;
                }
            }
            else
            {
                bulletGo = new GameObject("Bullet");
                bulletGo.transform.position = fromPos;
                bulletGo.transform.SetParent(null);
                ps = bulletGo.AddComponent<ParticleSystem>();

                ParticleSystem.MainModule m = ps.main;
                m.startSpeed = 0f;
                m.startLifetime = 0.3f;
                m.startSize = 0.15f;
                m.startColor = isEnemy ? new Color(1f, 0.3f, 0.2f) : new Color(0.3f, 0.6f, 1f);
                m.maxParticles = 15;
                m.loop = false;
                m.playOnAwake = false;
                m.simulationSpace = ParticleSystemSimulationSpace.World;
                m.duration = 0.3f;

                ParticleSystem.ShapeModule shape = ps.shape;
                shape.shapeType = ParticleSystemShapeType.Box;
                shape.scale = Vector3.one * 0.02f;

                ParticleSystem.EmissionModule emit = ps.emission;
                emit.rateOverTime = 0;

                ParticleSystemRenderer r = ps.GetComponent<ParticleSystemRenderer>();
                if (_bulletMat == null) _bulletMat = GetOrCreateParticleMat();
                if (_bulletMat != null) r.sharedMaterial = _bulletMat;
                r.sortingOrder = 32767;

                lifeTime = dist / speed;
                ps.Play();
                ParticleSystem.EmitParams ep = new ParticleSystem.EmitParams();
                ep.position = Vector3.zero;
                ep.velocity = dir * speed;
                ep.startSize = 0.15f;
                ep.startLifetime = lifeTime;
                ep.startColor = isEnemy ? new Color(1f, 0.3f, 0.2f) : new Color(0.3f, 0.6f, 1f);
                for (int i = 0; i < 3; i++)
                    ps.Emit(ep, 1);
            }

            if (ps != null)
            {
                if (_bulletPrefab != null) ps.Play();
                Destroy(bulletGo, lifeTime + 0.5f);
            }

            // hit spark on target position
            if (_hitSparkPrefab != null)
            {
                GameObject spark = Instantiate(_hitSparkPrefab, targetPos, Quaternion.identity);
                spark.transform.SetParent(null);
                ParticleSystem sp = spark.GetComponentInChildren<ParticleSystem>();
                if (sp != null)
                {
                    ParticleSystem.MainModule sm = sp.main;
                    sm.loop = false;
                    Destroy(spark, sm.duration + sm.startLifetime.constantMax);
                }
                else
                {
                    Destroy(spark, 0.5f);
                }
            }
            else
            {
                // auto spark
                GameObject spark = new GameObject("HitSpark");
                spark.transform.position = targetPos;
                spark.transform.SetParent(null);
                ParticleSystem sps = spark.AddComponent<ParticleSystem>();
                ParticleSystem.MainModule sm = sps.main;
                sm.startSpeed = 3f;
                sm.startLifetime = 0.2f;
                sm.startSize = 0.08f;
                sm.startColor = new Color(1f, 0.8f, 0.2f, 1f);
                sm.maxParticles = 10;
                sm.loop = false;
                sm.playOnAwake = true;
                sm.simulationSpace = ParticleSystemSimulationSpace.World;

                ParticleSystem.ShapeModule sshape = sps.shape;
                sshape.shapeType = ParticleSystemShapeType.Circle;
                sshape.radius = 0.05f;
                sshape.arc = 360f;

                ParticleSystem.EmissionModule semit = sps.emission;
                semit.rateOverTime = 0;
                semit.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0, (short)6) });

                ParticleSystemRenderer sr = sps.GetComponent<ParticleSystemRenderer>();
                if (_bulletMat == null) _bulletMat = GetOrCreateParticleMat();
                if (_bulletMat != null) sr.sharedMaterial = _bulletMat;
                sr.sortingOrder = 32767;

                sps.Play();
                Destroy(spark, 0.5f);
            }
        }

        protected virtual void Die()
        {
            _isDead = true;
            _currentAction = UnitAction.Dead;
            SyncAnimator();
            for (int i = 0; i < OccupiedCols; i++)
                GridManager.Instance.Leave(_gridPos + i, this);

            if (GameManager.Instance != null)
                GameManager.Instance.UnregisterUnit(this);

            SpawnDeathEffect();

            OnUnitDeath?.Invoke(this);

            Destroy(gameObject);
        }

        protected bool InAttackRange(UnitBase target)
        {
            if (target == null) return false;
            int cols = OccupiedCols > 0 ? OccupiedCols : 1;
            for (int i = 0; i < cols; i++)
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
                if (dist < minDist || (dist == minDist && nearest is Tower && !(unit is Tower)))
                {
                    minDist = dist;
                    nearest = unit;
                }
            }

            return nearest;
        }

        protected UnitBase FindFarthestInRangeEnemy()
        {
            UnitBase farthest = null;
            int maxDist = -1;

            var allUnits = GameManager.Instance.GetAllUnits();
            foreach (var unit in allUnits)
            {
                if (unit == null || unit == this || unit._isDead) continue;
                if (unit.isEnemy == isEnemy) continue;
                if (!InAttackRange(unit)) continue;

                int dist = GridPosition.Distance(_gridPos, unit._gridPos);
                if (dist > maxDist || (dist == maxDist && farthest is Tower && !(unit is Tower)))
                {
                    maxDist = dist;
                    farthest = unit;
                }
            }

            return farthest;
        }

        protected void SyncAnimator()
        {
            if (_animator == null) return;
            _animator.SetInteger("Action", (int)_currentAction);
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
