using System.Collections.Generic;
using UnityEngine;

namespace PPCorps
{
    public class CurrencyPopup : MonoBehaviour
    {
        private List<CurrencyChangeInfo> _currentChanges;
        private bool _show;

        [Header("弹窗布局")]
        [SerializeField] private float _iconSize = 96f;
        [SerializeField] private float _gapBetweenItems = 60f;
        [SerializeField] private float _nameOffsetY = 10f;
        [SerializeField] private float _badgeSize = 28f;

        private void Awake()
        {
            if (CurrencyManager.Instance != null)
                CurrencyManager.Instance.OnCurrencyChanged += OnCurrencyChanged;
        }

        private void OnDestroy()
        {
            if (CurrencyManager.Instance != null)
                CurrencyManager.Instance.OnCurrencyChanged -= OnCurrencyChanged;
        }

        private void OnCurrencyChanged(List<CurrencyChangeInfo> changes)
        {
            _currentChanges = changes;
            _show = true;
        }

        private void OnGUI()
        {
            if (!_show || _currentChanges == null || _currentChanges.Count == 0) return;

            // dark overlay
            GUI.color = new Color(0, 0, 0, 0.6f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // calculate total width
            int count = _currentChanges.Count;
            float totalW = count * _iconSize + (count - 1) * _gapBetweenItems;
            float startX = (Screen.width - totalW) / 2f;
            float centerY = (Screen.height - _iconSize) / 2f;

            for (int i = 0; i < count; i++)
            {
                var change = _currentChanges[i];
                float x = startX + i * (_iconSize + _gapBetweenItems);

                // icon
                Rect iconRect = new Rect(x, centerY, _iconSize, _iconSize);
                if (change.icon != null)
                    GUI.DrawTexture(iconRect, change.icon.texture);
                else
                {
                    GUI.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
                    GUI.DrawTexture(iconRect, Texture2D.whiteTexture);
                    GUI.color = Color.white;
                }

                // amount badge (右下角)
                string amountText = $"+{change.amount}";
                float badgeW = _badgeSize;
                float badgeH = _badgeSize;
                float badgeX = iconRect.xMax - badgeW;
                float badgeY = iconRect.yMax - badgeH;

                GUI.color = new Color(0, 0, 0, 0.8f);
                GUI.DrawTexture(new Rect(badgeX, badgeY, badgeW, badgeH), Texture2D.whiteTexture);
                GUI.color = Color.white;
                GUI.Label(new Rect(badgeX, badgeY, badgeW, badgeH), amountText, new GUIStyle
                {
                    fontSize = Mathf.RoundToInt(badgeW * 0.45f),
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleRight,
                    normal = new GUIStyleState { textColor = Color.white }
                });

                // name below icon
                float nameY = iconRect.yMax + _nameOffsetY;
                float nameW = _iconSize + 40;
                GUI.Label(new Rect(x - 20, nameY, nameW, 24), change.currencyName, new GUIStyle
                {
                    fontSize = 16,
                    alignment = TextAnchor.UpperCenter,
                    normal = new GUIStyleState { textColor = Color.white }
                });
            }

            // click anywhere to dismiss
            if (Event.current.type == EventType.MouseDown)
            {
                _show = false;
                _currentChanges = null;
            }
        }
    }
}
