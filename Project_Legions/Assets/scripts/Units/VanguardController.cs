using UnityEngine;

namespace PPCorps
{
    public class VanguardController : MonoBehaviour
    {
        private UnitBase _unit;
        private bool _isCharging;
        private int _chargeRemainingBeats;
        private UnitBase _chargeTarget;

        private void Awake()
        {
            _unit = GetComponent<UnitBase>();
            if (_unit == null)
            {
                enabled = false;
                return;
            }
            if (GameManager.Instance != null)
                GameManager.Instance.OnBeat += OnGameBeat;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnBeat -= OnGameBeat;
        }

        private void OnGameBeat(int bar, int beat)
        {
            if (_unit == null || _unit.IsDead || _unit.Data == null || !_unit.Data.isVanguard)
                return;

            if (!_isCharging)
                TryStartCharge();

            if (_isCharging)
                TickCharge();
        }

        private void TryStartCharge()
        {
            UnitBase target = FindChargeTarget();
            if (target == null || !PathIsClear(target)) return;

            _isCharging = true;
            _chargeTarget = target;
            _chargeRemainingBeats = _unit.Data.vanguardChargeBeats;
            _unit.SuppressNormalBehavior = true;
        }

        private UnitBase FindChargeTarget()
        {
            int dir = _unit.IsEnemy ? -1 : 1;
            int effectiveMin = Mathf.Max(_unit.Data.attackRange + 1, _unit.Data.vanguardScanMin);
            int minCol = _unit.GridPos.col + dir * effectiveMin;
            int maxCol = _unit.GridPos.col + dir * _unit.Data.vanguardScanMax;
            if (dir < 0)
            {
                int t = minCol; minCol = maxCol; maxCol = t;
            }

            UnitBase nearest = null;
            int nearestDist = int.MaxValue;

            var allUnits = GameManager.Instance.GetAllUnits();
            foreach (var unit in allUnits)
            {
                if (unit == null || unit == _unit || unit.IsDead) continue;
                if (unit.IsEnemy == _unit.IsEnemy) continue;

                int c = unit.GridPos.col;
                if (c >= minCol && c <= maxCol)
                {
                    int d = GridPosition.Distance(_unit.GridPos, unit.GridPos);
                    if (d < nearestDist)
                    {
                        nearestDist = d;
                        nearest = unit;
                    }
                }
            }
            return nearest;
        }

        private bool PathIsClear(UnitBase target)
        {
            int start = Mathf.Min(_unit.GridPos.col, target.GridPos.col) + 1;
            int end = Mathf.Max(_unit.GridPos.col, target.GridPos.col) - 1;

            for (int c = start; c <= end; c++)
            {
                var occs = GridManager.Instance.GetOccupants(new GridPosition(c));
                foreach (var o in occs)
                {
                    if (!(o is Tower))
                        return false;
                }
            }
            return true;
        }

        private void TickCharge()
        {
            _chargeRemainingBeats--;

            if (_chargeTarget == null || _chargeTarget.IsDead)
            {
                CancelCharge();
                return;
            }

            if (_chargeRemainingBeats <= 0)
                ExecuteCharge();
        }

        private void ExecuteCharge()
        {
            if (_chargeTarget == null || _chargeTarget.IsDead)
            {
                CancelCharge();
                return;
            }

            int dir = _unit.IsEnemy ? -1 : 1;
            GridPosition targetPos = _chargeTarget.GridPos;
            GridPosition behindPos = targetPos + dir;

            bool canKnockback = !(_chargeTarget is Tower)
                             && GridManager.Instance.IsInBounds(behindPos)
                             && GridManager.Instance.CanOccupy(behindPos, _chargeTarget);

            if (canKnockback)
            {
                _chargeTarget.ForceMove(behindPos);
                _unit.ForceMove(targetPos);
            }
            else
            {
                GridPosition frontPos = targetPos - dir;
                frontPos.col = Mathf.Clamp(frontPos.col, 0, GridManager.Instance.Cols - 1);
                _unit.ForceMove(frontPos);
            }

            _unit.SpawnBulletEffect(_chargeTarget.transform.position);
            _chargeTarget.TakeDamage(_unit.Data.vanguardDamage);

            _isCharging = false;
            _chargeTarget = null;
            _unit.SuppressNormalBehavior = false;
        }

        private void CancelCharge()
        {
            _isCharging = false;
            _chargeTarget = null;
            _unit.SuppressNormalBehavior = false;
        }
    }
}
