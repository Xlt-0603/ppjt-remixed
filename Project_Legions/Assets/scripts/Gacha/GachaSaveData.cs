using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PPCorps
{
    [Serializable]
    public class GachaRecord
    {
        public string timestamp;
        public string itemNames; // comma-separated, since GachaItemData can't serialize directly
        public bool isMulti;

        public GachaRecord(List<GachaItemData> items, bool isMulti)
        {
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            itemNames = string.Join(",", items.Select(i => i != null ? i.name : "null"));
            this.isMulti = isMulti;
        }

        public string[] GetItemNames()
        {
            return itemNames.Split(',');
        }
    }

    [Serializable]
    public class CopyCountEntry
    {
        public string itemName;
        public int count;
    }

    [Serializable]
    public class GachaSaveData
    {
        public int 彩计数;
        public bool 大保底状态;
        public int 金计数;
        public List<CopyCountEntry> copyCounts = new List<CopyCountEntry>();
        public List<GachaRecord> history = new List<GachaRecord>();

        public int GetCopyCount(string itemName)
        {
            var entry = copyCounts.Find(e => e.itemName == itemName);
            return entry != null ? entry.count : 0;
        }

        public void AddCopy(string itemName)
        {
            var entry = copyCounts.Find(e => e.itemName == itemName);
            if (entry != null)
                entry.count++;
            else
                copyCounts.Add(new CopyCountEntry { itemName = itemName, count = 1 });
        }
    }
}
