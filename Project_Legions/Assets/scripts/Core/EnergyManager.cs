using System;
using UnityEngine;

namespace PPCorps
{
    public class EnergyManager : MonoBehaviour
    {
        public static EnergyManager Instance { get; private set; }

        [Header("回费设置")]
        [SerializeField] private float _refillInterval = 15f;
        [SerializeField] private int[] _refillSchedule = new int[]
            { 3, 3, 3, 4, 4, 4, 4, 4, 4, 5, 5, 6 };

        public int CurrentEnergy { get; private set; }
        public int CurrentMaxEnergy { get; private set; }
        public float RefillTimer { get; private set; }
        public float RefillInterval => _refillInterval;
        public int RefillCount { get; private set; }

        public event Action<int, int> OnEnergyChanged;
        public event Action<float> OnRefillTimerChanged;

        private void Awake()
        {
            Instance = this;
            CurrentEnergy = _refillSchedule.Length > 0 ? _refillSchedule[0] : 3;
            CurrentMaxEnergy = CurrentEnergy;
            RefillCount = 0;
            RefillTimer = 0f;
        }

        private void Update()
        {
            RefillTimer += Time.deltaTime;
            OnRefillTimerChanged?.Invoke(RefillTimer);

            if (RefillTimer >= _refillInterval)
            {
                RefillTimer -= _refillInterval;
                DoRefill();
            }
        }

        private void DoRefill()
        {
            RefillCount++;
            int target = GetRefillAmount();

            CurrentMaxEnergy = target;
            int oldEnergy = CurrentEnergy;
            CurrentEnergy = target;

            if (oldEnergy != CurrentEnergy)
                OnEnergyChanged?.Invoke(CurrentEnergy, CurrentMaxEnergy);
        }

        public int GetRefillAmount()
        {
            int index = Mathf.Min(RefillCount, _refillSchedule.Length - 1);
            return _refillSchedule[index];
        }

        public bool Spend(int amount)
        {
            if (amount > CurrentEnergy) return false;
            CurrentEnergy -= amount;
            OnEnergyChanged?.Invoke(CurrentEnergy, CurrentMaxEnergy);
            return true;
        }

        public void AddEnergy(int amount)
        {
            CurrentEnergy = Mathf.Min(CurrentMaxEnergy, CurrentEnergy + amount);
            OnEnergyChanged?.Invoke(CurrentEnergy, CurrentMaxEnergy);
        }

        public void ResetRefillTimer()
        {
            RefillTimer = 0f;
            OnRefillTimerChanged?.Invoke(RefillTimer);
        }
    }
}
