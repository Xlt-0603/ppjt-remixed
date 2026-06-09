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

        [Header("满突角色自动跳转时间（秒）")]
        [SerializeField] private float _autoTransitionTime = 1f;

        [Header("逐一展示 - 角色图标大小")]
        [SerializeField] private float _charIconSize = 100f;
        [Header("逐一展示 - 货币图标大小")]
        [SerializeField] private float _currencyIconSize = 96f;
        [SerializeField] private float _currencyBadgeSize = 28f;

        [Header("统一展示 - 卡槽大小")]
        [SerializeField] private float _slotWidth = 80f;
        [SerializeField] private float _slotHeight = 140f;
        [SerializeField] private float _slotIconSize = 60f;
        [SerializeField] private float _currencyMiniIconSize = 24f;
        [SerializeField] private float _currencyMiniBadgeSize = 16f;

        [Header("布局偏移")]
        [SerializeField] private float _nameOffsetY = 15f;
        [SerializeField] private float _fullLabelOffsetY = 50f;
        [SerializeField] private float _buttonOffsetY = 120f;

        [Header("抽卡结果过渡页面（可拖入，空=跳过）")]
        [SerializeField] private GachaResultPage _resultPage;

        private bool _showingResultPage;
        private List<GachaPullItem> _currentPull;
        private int _displayIndex;

        private enum Phase { Idle, ShowRarity, ShowOneByOne, ShowAll }
        private Phase _phase = Phase.Idle;
        private Rarity _highestRarity;
        private bool _isLoading;

        private enum CardSubPhase { ShowChar, ShowCurrency }
        private CardSubPhase _cardSubPhase;
        private float _autoTimer;

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

            if (_isLoading || _showingResultPage)
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
            _gachaSystem.SaveHistory(ExtractItems(result), isMulti);
            _isLoading = false;

            Rarity highest = GetHighestRarity(result);
            if (_resultPage != null)
            {
                _showingResultPage = true;
                _resultPage.Play(highest, () =>
                {
                    _showingResultPage = false;
                    ShowPull(result);
                });
            }
            else
                ShowPull(result);
        }

        private Rarity GetHighestRarity(List<GachaPullItem> items)
        {
            Rarity h = Rarity.白;
            foreach (var p in items)
                if (p.item != null && (int)p.item.rarity > (int)h)
                    h = p.item.rarity;
            return h;
        }

        private List<GachaItemData> ExtractItems(List<GachaPullItem> pullItems)
        {
            var list = new List<GachaItemData>();
            foreach (var p in pullItems)
                if (p.item != null) list.Add(p.item);
            return list;
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
                _cardSubPhase = CardSubPhase.ShowChar;
                _phase = Phase.ShowOneByOne;
            }
        }

        private void DrawOneByOnePhase()
        {
            if (_displayIndex >= _currentPull.Count) return;
            var pullItem = _currentPull[_displayIndex];
            if (pullItem.item == null) return;
            var item = pullItem.item;

            float cx = Screen.width / 2f;
            float cy = Screen.height / 2f;

            if (!pullItem.hasCurrencyChange)
            {
                DrawCharDisplay(item, cx, cy);
                DrawOneByOneButtons();
            }
            else
            {
                if (_cardSubPhase == CardSubPhase.ShowChar)
                {
                    DrawCharDisplay(item, cx, cy);

                    GUI.color = new Color(1, 0.8f, 0, 1);
                    GUI.Label(new Rect(cx - 50, cy + _fullLabelOffsetY, 100, 24), "已满突", GetSmallCenterStyle());
                    GUI.color = Color.white;

                    _autoTimer -= Time.unscaledDeltaTime;
                    if (_autoTimer <= 0f)
                        _cardSubPhase = CardSubPhase.ShowCurrency;
                }
                else
                {
                    DrawCurrencyDisplay(pullItem.currencyChange, cx, cy);
                    DrawOneByOneButtons();
                }
            }
            _autoTimer = Mathf.Max(0f, _autoTimer);
        }

        private void DrawCharDisplay(GachaItemData item, float cx, float cy)
        {
            float half = _charIconSize * 0.5f;
            if (item.icon != null)
                GUI.DrawTexture(new Rect(cx - half, cy - _charIconSize, _charIconSize, _charIconSize), item.icon.texture);

            Color c = GetRarityColor(item.rarity);
            GUI.color = c;
            GUI.Label(new Rect(cx - 100, cy + _nameOffsetY, 200, 30), $"{item.itemName} ({item.rarity})", GetBigStyle());
            GUI.color = Color.white;
        }

        private void DrawCurrencyDisplay(CurrencyChangeInfo change, float cx, float cy)
        {
            float half = _currencyIconSize * 0.5f;
            float iconX = cx - half;
            float iconY = cy - half;

            if (change.icon != null)
                GUI.DrawTexture(new Rect(iconX, iconY, _currencyIconSize, _currencyIconSize), change.icon.texture);
            else
            {
                GUI.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
                GUI.DrawTexture(new Rect(iconX, iconY, _currencyIconSize, _currencyIconSize), Texture2D.whiteTexture);
                GUI.color = Color.white;
            }

            float badgeX = iconX + _currencyIconSize - _currencyBadgeSize;
            float badgeY = iconY + _currencyIconSize - _currencyBadgeSize;
            GUI.color = new Color(0, 0, 0, 0.8f);
            GUI.DrawTexture(new Rect(badgeX, badgeY, _currencyBadgeSize, _currencyBadgeSize), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(badgeX, badgeY, _currencyBadgeSize, _currencyBadgeSize), $"+{change.amount}", new GUIStyle
            {
                fontSize = Mathf.RoundToInt(_currencyBadgeSize * 0.45f),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight,
                normal = new GUIStyleState { textColor = Color.white }
            });

            GUI.Label(new Rect(cx - 60, iconY + _currencyIconSize + 10, 120, 24), change.currencyName, new GUIStyle
            {
                fontSize = 16,
                alignment = TextAnchor.UpperCenter,
                normal = new GUIStyleState { textColor = Color.white }
            });
        }

        private void DrawOneByOneButtons()
        {
            float cx = Screen.width / 2f;
            float btnY = Screen.height / 2f + _buttonOffsetY;

            if (_displayIndex < _currentPull.Count - 1)
            {
                if (GUI.Button(new Rect(cx - 50, btnY, 100, 30), "继续"))
                {
                    _displayIndex++;
                    _cardSubPhase = CardSubPhase.ShowChar;
                    _autoTimer = _autoTransitionTime;
                }
            }
            else
            {
                if (GUI.Button(new Rect(cx - 50, btnY, 100, 30), "全部展示"))
                    _phase = Phase.ShowAll;
            }
        }

        private void DrawAllPhase()
        {
            float startX = Screen.width / 2f - (_currentPull.Count * _slotWidth) / 2f;
            float y = Screen.height / 2f - _slotHeight / 2f;

            for (int i = 0; i < _currentPull.Count; i++)
            {
                var pullItem = _currentPull[i];
                if (pullItem.item == null) continue;
                var item = pullItem.item;
                float x = startX + i * _slotWidth;

                float iconOffsetX = (_slotWidth - _slotIconSize) * 0.5f;
                if (item.icon != null)
                    GUI.DrawTexture(new Rect(x + iconOffsetX, y, _slotIconSize, _slotIconSize), item.icon.texture);

                Color c = GetRarityColor(item.rarity);
                GUI.color = c;
                GUI.Label(new Rect(x, y + _slotIconSize + 5, _slotWidth, 20), item.itemName, GetMiniStyle());
                GUI.color = Color.white;

                if (pullItem.hasCurrencyChange)
                {
                    float currencyY = y + _slotIconSize + 28;
                    float ciX = x + (_slotWidth - _currencyMiniIconSize) * 0.5f;

                    if (pullItem.currencyChange.icon != null)
                        GUI.DrawTexture(new Rect(ciX, currencyY, _currencyMiniIconSize, _currencyMiniIconSize), pullItem.currencyChange.icon.texture);

                    string badgeText = $"×{pullItem.currencyChange.amount}";
                    float badgeX = ciX + _currencyMiniIconSize - _currencyMiniBadgeSize;
                    float badgeY2 = currencyY + _currencyMiniIconSize - _currencyMiniBadgeSize;
                    GUI.color = new Color(0, 0, 0, 0.8f);
                    GUI.DrawTexture(new Rect(badgeX, badgeY2, _currencyMiniBadgeSize, _currencyMiniBadgeSize), Texture2D.whiteTexture);
                    GUI.color = Color.white;
                    GUI.Label(new Rect(badgeX, badgeY2, _currencyMiniBadgeSize, _currencyMiniBadgeSize), badgeText, new GUIStyle
                    {
                        fontSize = Mathf.RoundToInt(_currencyMiniBadgeSize * 0.45f),
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleRight,
                        normal = new GUIStyleState { textColor = Color.white }
                    });
                }
            }

            if (GUI.Button(new Rect(Screen.width / 2f - 50, Screen.height / 2f + _slotHeight / 2f + 20, 100, 30), "完成"))
            {
                FlushCurrencyChanges();
                _phase = Phase.Idle;
            }
        }

        private void FlushCurrencyChanges()
        {
            if (CurrencyManager.Instance == null) return;
            var changes = new List<CurrencyChangeInfo>();
            foreach (var p in _currentPull)
                if (p.hasCurrencyChange)
                    changes.Add(p.currencyChange);
            if (changes.Count > 0)
                CurrencyManager.Instance.AddCurrencies(changes);
        }

        private void DrawBadge(float x, float y, float w, float h, string text)
        {
            GUI.color = new Color(0, 0, 0, 0.75f);
            GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(x, y, w, h), text, new GUIStyle
            {
                fontSize = Mathf.RoundToInt(w * 0.55f),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight,
                normal = new GUIStyleState { textColor = Color.white }
            });
        }

        [ContextMenu("测试展示")]
        private void TestShow()
        {
            var list = new List<GachaPullItem>();
            for (int i = 0; i < 5; i++)
            {
                var item = ScriptableObject.CreateInstance<GachaItemData>();
                item.name = $"测试{i + 1}";
                item.itemName = $"测试{i + 1}";
                item.rarity = (Rarity)Random.Range(0, 4);
                item.maxCopies = 1;
                var pullItem = new GachaPullItem { item = item };
                list.Add(pullItem);
            }
            ShowPull(list);
        }

        public void ShowPull(List<GachaPullItem> items)
        {
            _currentPull = items;
            _displayIndex = 0;
            _cardSubPhase = CardSubPhase.ShowChar;
            _autoTimer = _autoTransitionTime;
            _highestRarity = Rarity.白;
            foreach (var p in items)
                if (p.item != null && (int)p.item.rarity > (int)_highestRarity)
                    _highestRarity = p.item.rarity;
            _phase = Phase.ShowRarity;
        }

        private Color GetRarityColor(Rarity r)
        {
            return r switch
            {
                Rarity.彩 => new Color(1, 0.5f, 0, 1),
                Rarity.金 => new Color(1, 0.84f, 0, 1),
                Rarity.蓝 => new Color(0.3f, 0.6f, 1, 1),
                _ => Color.white
            };
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

        private GUIStyle GetSmallStyle()
        {
            return new GUIStyle
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleLeft,
                normal = new GUIStyleState { textColor = Color.white }
            };
        }

        private GUIStyle GetSmallCenterStyle()
        {
            return new GUIStyle
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter,
                normal = new GUIStyleState { textColor = Color.white }
            };
        }

        private GUIStyle GetMiniStyle()
        {
            return new GUIStyle
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                normal = new GUIStyleState { textColor = Color.white }
            };
        }
    }
}
