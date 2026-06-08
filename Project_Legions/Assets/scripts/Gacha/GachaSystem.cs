using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PPCorps
{
    [System.Serializable]
    public class RarityCurrencyMapping
    {
        public Rarity rarity;
        public string currencyName;
        public Sprite currencyIcon;
        public int currencyPerCopy;
    }

    public struct GachaPullItem
    {
        public GachaItemData item;
        public CurrencyChangeInfo currencyChange;
        public bool hasCurrencyChange;
    }

    public class GachaSystem : MonoBehaviour
    {
        [Header("卡池")]
        public GachaBanner currentBanner;

        [Header("基础概率 (%)")]
        [Range(0, 100)] public float prob彩 = 0.6f;
        [Range(0, 100)] public float prob金 = 5f;
        [Range(0, 100)] public float prob蓝 = 30f;

        [Header("彩保底")]
        public int 小保底计数 = 40;
        [Range(0, 100)] public float 小保底up概率 = 30f;

        [Header("软保底")]
        public int 软保底起始 = 36;

        [Header("金保底")]
        public int 金保底计数 = 5;

        [Header("满突转化（品级→货币，留空待填）")]
        public RarityCurrencyMapping[] rarityCurrencyMappings;

        // ---- 运行时状态 ----
        private GachaSaveData _save;
        private const string SaveKey = "GachaSave";

        private void Awake()
        {
            LoadSave();
        }

        private void LoadSave()
        {
            string json = PlayerPrefs.GetString(SaveKey, "");
            if (string.IsNullOrEmpty(json))
                _save = new GachaSaveData();
            else
                _save = JsonUtility.FromJson<GachaSaveData>(json);
        }

        private void Save()
        {
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(_save));
            PlayerPrefs.Save();
        }

        public List<GachaPullItem> Pull(int count)
        {
            if (currentBanner == null)
            {
                Debug.LogError("GachaSystem: currentBanner 未设置");
                return new List<GachaPullItem>();
            }
            var results = new List<GachaPullItem>();
            for (int i = 0; i < count; i++)
            {
                var item = PullOnce();
                if (item.item != null) results.Add(item);
            }

            Save();
            return results;
        }

        public void SaveHistory(List<GachaItemData> items, bool isMulti)
        {
            _save.history.Add(new GachaRecord(items, isMulti));
            Save();
        }

        public List<GachaRecord> GetHistory()
        {
            return new List<GachaRecord>(_save.history);
        }

        [ContextMenu("清除全部抽卡进度（重置保底、副本、记录）")]
        private void ResetAllSave()
        {
            _save = new GachaSaveData();
            Save();
            Debug.Log("Gacha save data has been reset.");
        }

        private GachaPullItem PullOnce()
        {
            if (_save == null) LoadSave();
            _save.彩计数++;
            _save.金计数++;

            // ---- 确定稀有度（含保底） ----
            Rarity rarity = ResolveRarity();

            // ---- 取对应稀有度的物品（不再过滤 maxCopies） ----
            GachaItemData item = PickItemFromRarity(rarity);

            // ---- 重试：该稀有度无可用物品时重新抽 ----
            int retries = 30;
            while (item == null && retries > 0)
            {
                rarity = ResolveRarity();
                item = PickItemFromRarity(rarity);
                retries--;
            }
            if (item == null) return new GachaPullItem { item = null };

            // ---- 彩稀有度：UP/大保底处理 ----
            if (rarity == Rarity.彩)
            {
                _save.彩计数 = 0;
                _save.金计数 = 0;

                bool wonUp = _save.大保底状态 || Random.value * 100f < 小保底up概率;
                _save.大保底状态 = !wonUp;

                if (wonUp)
                {
                    var upPool = currentBanner.GetAllAvailable(Rarity.彩, true)
                        .Where(i => i != null)
                        .ToList();
                    if (upPool.Count > 0)
                        item = upPool[Random.Range(0, upPool.Count)];
                }
            }

            // ---- 金稀有度：重置金保底计数 ----
            if (rarity == Rarity.金)
                _save.金计数 = 0;

            TrackCopy(item);

            // ---- 满突转化 ----
            var result = new GachaPullItem { item = item };
            bool overMax = GetCopyCount(item) > item.maxCopies;
            if (overMax)
            {
                var mapping = GetCurrencyMapping(item.rarity);
                if (mapping != null)
                {
                    result.hasCurrencyChange = true;
                    result.currencyChange = new CurrencyChangeInfo
                    {
                        currencyName = mapping.currencyName,
                        amount = mapping.currencyPerCopy,
                        icon = mapping.currencyIcon
                    };
                }
            }

            return result;
        }

        private Rarity ResolveRarity()
        {
            if (_save.彩计数 >= 小保底计数)
            {
                _save.彩计数 = 0;
                return Rarity.彩;
            }

            Rarity rarity = RollRarity();

            if (_save.金计数 >= 金保底计数 && rarity < Rarity.金)
            {
                _save.金计数 = 0;
                return Rarity.金;
            }

            return rarity;
        }

        private Rarity RollRarity()
        {
            float adjusted彩 = prob彩;

            // 软保底
            if (_save.彩计数 >= 软保底起始)
            {
                int over = _save.彩计数 - 软保底起始;
                int range = 小保底计数 - 软保底起始;
                float t = Mathf.Clamp01((float)over / range);
                adjusted彩 = Mathf.Lerp(prob彩, 100f, t);
            }

            float total = adjusted彩 + prob金 + prob蓝;
            float roll = Random.value * Mathf.Max(total, 100f);

            if (roll < adjusted彩) return Rarity.彩;
            roll -= adjusted彩;
            if (roll < prob金) return Rarity.金;
            roll -= prob金;
            if (roll < prob蓝) return Rarity.蓝;
            return Rarity.白;
        }

        private GachaItemData PickItemFromRarity(Rarity rarity)
        {
            var pool = new List<GachaItemData>();
            pool.AddRange(currentBanner.GetAllAvailable(rarity, true));
            pool.AddRange(currentBanner.GetAllAvailable(rarity, false));

            var available = pool
                .Where(i => i != null)
                .ToList();

            if (available.Count == 0) return null;
            return available[Random.Range(0, available.Count)];
        }

        public RarityCurrencyMapping GetCurrencyMapping(Rarity rarity)
        {
            if (rarityCurrencyMappings == null) return null;
            foreach (var m in rarityCurrencyMappings)
                if (m.rarity == rarity) return m;
            return null;
        }

        public int GetCopyCount(GachaItemData item)
        {
            return _save.GetCopyCount(item.name);
        }

        private void TrackCopy(GachaItemData item)
        {
            _save.AddCopy(item.name);
        }

        public string GetPityInfo()
        {
            return $"彩={_save.彩计数}/{小保底计数} ({( _save.大保底状态 ? "大保底" : "小保底")}) 金={_save.金计数}/{金保底计数}";
        }
    }
}
