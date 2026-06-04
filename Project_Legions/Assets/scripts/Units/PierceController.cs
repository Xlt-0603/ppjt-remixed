using UnityEngine;

namespace PPCorps
{
    public class PierceController : MonoBehaviour
    {
        private UnitBase _unit;
        private int _moveCooldown;

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

            // handle movement
            if (beat == 1 && _moveCooldown == 0 && !HasEnemyInRange())
                TryMoveForward();

            // handle attack
            if (ShouldAttackOnBeat(beat) && HasEnemyInRange())
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

            _unit.SetAction(UnitAction.Attacking);
        }
    }
}
