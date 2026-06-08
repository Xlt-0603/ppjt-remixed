using System;
using System.Collections.Generic;
using UnityEngine;

namespace PPCorps
{
    public struct CurrencyChangeInfo
    {
        public string currencyName;
        public int amount;
        public Sprite icon;
    }

    public class CurrencyManager : MonoBehaviour
    {
        public static CurrencyManager Instance { get; private set; }

        [Serializable]
        public class CurrencyDefinition
        {
            public string currencyName;
            public Sprite icon;
            public int initialAmount;
        }

        [Header("货币定义（在 Inspector 中配置名称、图标、初始数量）")]
        public CurrencyDefinition[] currencyDefinitions;

        private Dictionary<string, int> _amounts = new Dictionary<string, int>();
        private Dictionary<string, Sprite> _icons = new Dictionary<string, Sprite>();

        public event Action<List<CurrencyChangeInfo>> OnCurrencyChanged;

        private const string SaveKey = "CurrencySave";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadSave();
        }

        private void Start()
        {
            if (currencyDefinitions == null) return;
            foreach (var def in currencyDefinitions)
            {
                if (def == null || string.IsNullOrEmpty(def.currencyName)) continue;
                if (!_amounts.ContainsKey(def.currencyName))
                    _amounts[def.currencyName] = def.initialAmount;
                if (!_icons.ContainsKey(def.currencyName))
                    _icons[def.currencyName] = def.icon;
            }
            Save();
        }

        public void AddCurrency(string name, int amount)
        {
            if (string.IsNullOrEmpty(name) || amount <= 0) return;
            if (!_amounts.ContainsKey(name))
                _amounts[name] = 0;
            _amounts[name] += amount;
            Save();

            TriggerEvent(new List<CurrencyChangeInfo>
            {
                new CurrencyChangeInfo
                {
                    currencyName = name,
                    amount = amount,
                    icon = _icons.ContainsKey(name) ? _icons[name] : null
                }
            });
        }

        public void AddCurrencies(List<CurrencyChangeInfo> changes)
        {
            if (changes == null || changes.Count == 0) return;
            foreach (var c in changes)
            {
                if (string.IsNullOrEmpty(c.currencyName) || c.amount <= 0) continue;
                if (!_amounts.ContainsKey(c.currencyName))
                    _amounts[c.currencyName] = 0;
                _amounts[c.currencyName] += c.amount;
                if (!_icons.ContainsKey(c.currencyName) && c.icon != null)
                    _icons[c.currencyName] = c.icon;
            }
            Save();
            TriggerEvent(changes);
        }

        public bool SpendCurrency(string name, int amount)
        {
            if (string.IsNullOrEmpty(name) || amount <= 0) return false;
            if (!_amounts.ContainsKey(name) || _amounts[name] < amount) return false;
            _amounts[name] -= amount;
            Save();
            return true;
        }

        public int GetCurrency(string name)
        {
            return _amounts.ContainsKey(name) ? _amounts[name] : 0;
        }

        public Sprite GetIcon(string name)
        {
            return _icons.ContainsKey(name) ? _icons[name] : null;
        }

        private void TriggerEvent(List<CurrencyChangeInfo> changes)
        {
            OnCurrencyChanged?.Invoke(changes);
        }

        private void LoadSave()
        {
            string json = PlayerPrefs.GetString(SaveKey, "");
            if (string.IsNullOrEmpty(json)) return;
            var save = JsonUtility.FromJson<CurrencySaveData>(json);
            if (save == null || save.entries == null) return;
            foreach (var entry in save.entries)
            {
                if (entry != null && !string.IsNullOrEmpty(entry.currencyName))
                    _amounts[entry.currencyName] = entry.amount;
            }
        }

        private void Save()
        {
            var save = new CurrencySaveData();
            foreach (var kv in _amounts)
                save.entries.Add(new CurrencySaveEntry { currencyName = kv.Key, amount = kv.Value });
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(save));
            PlayerPrefs.Save();
        }

        [ContextMenu("重置所有货币")]
        private void ResetAllCurrency()
        {
            _amounts.Clear();
            _icons.Clear();
            PlayerPrefs.DeleteKey(SaveKey);
            PlayerPrefs.Save();
            Debug.Log("所有货币已重置");
        }

        [Serializable]
        private class CurrencySaveData
        {
            public List<CurrencySaveEntry> entries = new List<CurrencySaveEntry>();
        }

        [Serializable]
        private class CurrencySaveEntry
        {
            public string currencyName;
            public int amount;
        }
    }
}
