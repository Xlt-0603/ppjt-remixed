using System.Collections.Generic;
using UnityEngine;

namespace PPCorps
{
    public class GachaDisplay : MonoBehaviour
    {
        private GachaSystem _gachaSystem;
        private GachaHistoryUI _historyUI;

        [Header("按钮位置 (屏幕百分比)")]
        public Rect 单抽按钮区域 = new Rect(0.85f, 0.85f, 0.1f, 0.06f);
        public Rect 五连按钮区域 = new Rect(0.85f, 0.92f, 0.1f, 0.06f);
        public Rect 历史按钮区域 = new Rect(0.02f, 0.02f, 0.08f, 0.04f);

        private List<GachaItemData> _currentPull;
        private int _displayIndex;
        private enum Phase { Idle, ShowRarity, ShowOneByOne, ShowAll }
        private Phase _phase = Phase.Idle;
        private Rarity _highestRarity;
        private bool _isLoading;

        private void Awake()
        {
            _gachaSystem = GetComponent<GachaSystem>();
            _historyUI = GetComponent<GachaHistoryUI>();
        }

        private void OnGUI()
        {
            if (_phase == Phase.Idle)
            {
                DrawPullButtons();
                return;
            }

            if (_isLoading)
            {
                GUI.color = new Color(0, 0, 0, 0.6f);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
                GUI.color = Color.white;
                GUI.Label(new Rect(0, Screen.height / 2f - 30, Screen.width, 60), "Loading...", GetBigStyle());
                return;
            }

            switch (_phase)
            {
                case Phase.ShowRarity:
                    DrawRarityPhase();
                    break;
                case Phase.ShowOneByOne:
                    DrawOneByOnePhase();
                    break;
                case Phase.ShowAll:
                    DrawAllPhase();
                    break;
            }
        }

        private void DrawPullButtons()
        {
            DrawRectButton(单抽按钮区域, "单抽", () => DoPull(1, false));
            DrawRectButton(五连按钮区域, "五连", () => DoPull(5, true));
            DrawRectButton(历史按钮区域, "记录", () => { if (_historyUI != null) _historyUI.Toggle(); });
        }

        private void DrawRectButton(Rect rect, string label, System.Action onClick)
        {
            float x = rect.x * Screen.width;
            float y = rect.y * Screen.height;
            float w = rect.width * Screen.width;
            float h = rect.height * Screen.height;
            if (GUI.Button(new Rect(x, y, w, h), label))
                onClick();
        }

        private void DoPull(int count, bool isMulti)
        {
            _isLoading = true;
            var result = _gachaSystem.Pull(count);
            _gachaSystem.SaveHistory(result, isMulti);
            _isLoading = false;
            ShowPull(result);
        }

        private void DrawRarityPhase()
        {
            string label = _highestRarity switch
            {
                Rarity.彩 => "★★★★★ 彩色 ★★★★★",
                Rarity.金 => "★★★★ 金色 ★★★★",
                Rarity.蓝 => "★★★ 蓝色 ★★★",
                Rarity.白 => "★★ 白色 ★★",
                _ => "抽卡结果"
            };

            Color c = _highestRarity switch
            {
                Rarity.彩 => new Color(1, 0.5f, 0, 1),
                Rarity.金 => new Color(1, 0.84f, 0, 1),
                Rarity.蓝 => new Color(0.3f, 0.6f, 1, 1),
                _ => Color.white
            };

            GUI.color = c;
            GUI.Label(new Rect(Screen.width / 2f - 150, Screen.height / 2f - 30, 300, 60), label, GetBigStyle());
            GUI.color = Color.white;

            if (GUI.Button(new Rect(Screen.width / 2f - 50, Screen.height / 2f + 60, 100, 30), "继续"))
            {
                _displayIndex = 0;
                _phase = Phase.ShowOneByOne;
            }
        }

        private void DrawOneByOnePhase()
        {
            if (_displayIndex >= _currentPull.Count) return;
            var item = _currentPull[_displayIndex];

            string text = $"{item.itemName} ({(Rarity)item.rarity})";
            GUI.Label(new Rect(Screen.width / 2f - 100, Screen.height / 2f - 20, 200, 40), text, GetBigStyle());

            if (_displayIndex < _currentPull.Count - 1)
            {
                if (GUI.Button(new Rect(Screen.width / 2f - 50, Screen.height / 2f + 60, 100, 30), "继续"))
                    _displayIndex++;
            }
            else
            {
                if (GUI.Button(new Rect(Screen.width / 2f - 50, Screen.height / 2f + 60, 100, 30), "全部展示"))
                    _phase = Phase.ShowAll;
            }
        }

        private void DrawAllPhase()
        {
            float x = Screen.width / 2f - (_currentPull.Count * 60f) / 2f;
            for (int i = 0; i < _currentPull.Count; i++)
            {
                Color c = _currentPull[i].rarity switch
                {
                    Rarity.彩 => new Color(1, 0.5f, 0, 1),
                    Rarity.金 => new Color(1, 0.84f, 0, 1),
                    Rarity.蓝 => new Color(0.3f, 0.6f, 1, 1),
                    _ => Color.white
                };
                GUI.color = c;
                GUI.Label(new Rect(x + i * 60, Screen.height / 2f - 10, 60, 20), _currentPull[i].itemName);
            }
            GUI.color = Color.white;

            if (GUI.Button(new Rect(Screen.width / 2f - 50, Screen.height / 2f + 60, 100, 30), "完成"))
                _phase = Phase.Idle;
        }

        [ContextMenu("测试展示")]
        private void TestShow()
        {
            var list = new List<GachaItemData>();
            for (int i = 0; i < 5; i++)
            {
                var item = ScriptableObject.CreateInstance<GachaItemData>();
                item.name = $"测试{i + 1}";
                item.itemName = $"测试{i + 1}";
                item.rarity = (Rarity)Random.Range(0, 4);
                list.Add(item);
            }
            ShowPull(list);
        }

        public void ShowPull(List<GachaItemData> items)
        {
            _currentPull = items;
            _displayIndex = 0;
            _highestRarity = Rarity.白;
            foreach (var item in items)
                if ((int)item.rarity > (int)_highestRarity)
                    _highestRarity = item.rarity;
            _phase = Phase.ShowRarity;
        }

        private GUIStyle GetBigStyle()
        {
            return new GUIStyle
            {
                fontSize = 24,
                alignment = TextAnchor.MiddleCenter,
                normal = new GUIStyleState { textColor = Color.white }
            };
        }
    }
}
