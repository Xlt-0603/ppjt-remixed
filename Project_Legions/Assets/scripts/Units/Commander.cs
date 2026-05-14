using System;
using UnityEngine;

namespace PPCorps
{
    public class Commander : MonoBehaviour
    {
        [SerializeField] private int _maxHP = 20;

        public int maxHP => _maxHP;
        public int currentHP { get; private set; }

        public event Action<int, int> OnHPChanged;
        public event Action OnDeath;

        private void Start()
        {
            currentHP = _maxHP;
        }

        public void TakeDamage(int damage)
        {
            if (currentHP <= 0) return;

            currentHP = Mathf.Max(0, currentHP - damage);
            OnHPChanged?.Invoke(currentHP, _maxHP);

            if (currentHP <= 0)
                OnDeath?.Invoke();
        }
    }
}
