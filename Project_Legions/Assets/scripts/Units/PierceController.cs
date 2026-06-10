using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PPCorps
{
    public class PierceController : MonoBehaviour
    {
        private UnitBase _unit;
        private int _moveCooldown;
        private int _attackCooldown;

        [SerializeField] private GameObject _laserBeamPrefab;
        [SerializeField] private GameObject _helixLinePrefab;
        [SerializeField] private float _laserDuration = 0.8f;
        [SerializeField] private float _helixRadius = 0.25f;
        [SerializeField] private float _helixTurns = 3f;

        private static Material _sharedMat;

        private void Awake()
        {
            _unit = GetComponent<UnitBase>();
            if (_unit == null)
            {
                enabled = false;
                return;
            }
        }

        private void Start()
        {
            GameManager.Instance.OnBeat += OnGameBeat;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnBeat -= OnGameBeat;
        }

        private void OnGameBeat(int bar, int beat)
        {
            if (_unit == null || _unit.IsDead) return;

            if (_moveCooldown > 0) _moveCooldown--;
            if (_attackCooldown > 0) _attackCooldown--;

            // handle movement
            if (beat == 1 && _moveCooldown == 0 && !HasEnemyInRange())
                TryMoveForward();

            // handle attack
            if (_attackCooldown == 0 && ShouldAttackOnBeat(beat) && HasEnemyInRange())
                DoPierceAttack();

            _unit.SuppressNormalBehavior = true;
        }

        private bool ShouldAttackOnBeat(int beat)
        {
            var data = _unit.Data;
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

        private bool HasEnemyInRange()
        {
            int dir = _unit.IsEnemy ? -1 : 1;
            int range = _unit.Data.attackRange;
            int myCol = _unit.GridPos.col;

            int startCol = myCol + dir;
            int endCol = myCol + dir * range;
            if (dir < 0) { int t = startCol; startCol = endCol; endCol = t; }
            startCol = Mathf.Clamp(startCol, 0, GridManager.Instance.Cols - 1);
            endCol = Mathf.Clamp(endCol, 0, GridManager.Instance.Cols - 1);

            foreach (var target in GameManager.Instance.GetAllUnits())
            {
                if (target == null || target == _unit || target.IsDead) continue;
                if (target.IsEnemy == _unit.IsEnemy) continue;
                int c = target.GridPos.col;
                if (c >= startCol && c <= endCol)
                    return true;
            }
            return false;
        }

        private void TryMoveForward()
        {
            int dir = _unit.IsEnemy ? -1 : 1;
            GridPosition dest = _unit.GridPos + dir;
            if (!GridManager.Instance.IsInBounds(dest)) return;
            if (!GridManager.Instance.CanOccupy(dest, _unit)) return;
            _unit.ForceMove(dest);
            _moveCooldown = 8;
        }

        private void DoPierceAttack()
        {
            _attackCooldown = 8;
            int dir = _unit.IsEnemy ? -1 : 1;
            int myCol = _unit.GridPos.col;

            int startCol = myCol + dir;
            int endCol = _unit.IsEnemy ? 0 : GridManager.Instance.Cols - 1;
            if (dir < 0) { int t = startCol; startCol = endCol; endCol = t; }

            var targets = new System.Collections.Generic.List<UnitBase>();
            foreach (var target in GameManager.Instance.GetAllUnits())
            {
                if (target == null || target == _unit || target.IsDead) continue;
                if (target.IsEnemy == _unit.IsEnemy) continue;
                int c = target.GridPos.col;
                if (c >= startCol && c <= endCol)
                    targets.Add(target);
            }

            foreach (var target in targets)
            {
                int damage = _unit.Data.attackPower;
                if (target is Tower)
                    damage = damage / 2;
                target.TakeDamage(damage);
            }

            SpawnLaserEffect();

            _unit.SetAction(UnitAction.Attacking);
        }

        private static Material GetSharedMat()
        {
            if (_sharedMat != null) return _sharedMat;
            Shader s = Shader.Find("Particles/Alpha Blended");
            if (s == null) s = Shader.Find("Particles/Additive");
            if (s == null) s = Shader.Find("Sprites/Default");
            if (s != null) _sharedMat = new Material(s);
            return _sharedMat;
        }

        private void SpawnLaserEffect()
        {
            if (_laserBeamPrefab == null && _helixLinePrefab == null) return;

            int dir = _unit.IsEnemy ? -1 : 1;
            Vector3 fromPos = _unit.transform.position + new Vector3(dir * 0.3f, 0f, -1f);
            float endX = _unit.IsEnemy
                ? GridManager.Instance.GridToWorldX(new GridPosition(0)) - 2f
                : GridManager.Instance.GridToWorldX(new GridPosition(GridManager.Instance.Cols - 1)) + 2f;
            Vector3 endPos = new Vector3(endX, fromPos.y, -1f);

            // main beam — prefab at cannon, local positions
            if (_laserBeamPrefab != null)
            {
                GameObject beam = Instantiate(_laserBeamPrefab, fromPos, Quaternion.identity);
                beam.transform.SetParent(null);
                LineRenderer lr = beam.GetComponentInChildren<LineRenderer>();
                if (lr != null)
                {
                    lr.positionCount = 2;
                    lr.SetPosition(0, Vector3.zero);
                    lr.SetPosition(1, endPos - fromPos);
                }
                Destroy(beam, _laserDuration + 0.5f);
            }

            // helix line — prefab at cannon, local positions
            if (_helixLinePrefab != null)
                StartCoroutine(AnimateHelixLine(fromPos, endPos));
        }

        private IEnumerator AnimateHelixLine(Vector3 start, Vector3 end)
        {
            float dist = Vector3.Distance(start, end);
            if (dist < 0.1f) yield break;

            GameObject go = Instantiate(_helixLinePrefab, start, Quaternion.identity);
            go.transform.SetParent(null);
            LineRenderer lr = go.GetComponentInChildren<LineRenderer>();
            if (lr == null) { Destroy(go); yield break; }

            int segments = Mathf.Max(lr.positionCount, 60);

            int helixDir = _unit.IsEnemy ? -1 : 1;
            float elapsed = 0f;

            while (elapsed < _laserDuration)
            {
                float scroll = elapsed / _laserDuration * 2f;

                lr.positionCount = segments;
                for (int i = 0; i < segments; i++)
                {
                    float t = (float)i / (segments - 1);
                    Vector3 worldPos = Vector3.Lerp(start, end, t);
                    float angle = t * Mathf.PI * 2 * _helixTurns + scroll * Mathf.PI * 2 * helixDir;
                    worldPos.y += Mathf.Sin(angle) * _helixRadius;
                    lr.SetPosition(i, go.transform.InverseTransformPoint(worldPos));
                }

                lr.widthCurve = new AnimationCurve(
                    new Keyframe(0f, 0f),
                    new Keyframe(0.1f, 1f),
                    new Keyframe(0.9f, 1f),
                    new Keyframe(1f, 0f)
                );

                elapsed += Time.deltaTime;
                yield return null;
            }

            Destroy(go, 0.1f);
        }
    }
}
