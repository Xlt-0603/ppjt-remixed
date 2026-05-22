using UnityEngine;

namespace PPCorps
{
    public class EnergyUI : MonoBehaviour
    {
        [Header("费用图片（按费用值对应）")]
        [SerializeField] private Texture2D _icon3;
        [SerializeField] private Texture2D _icon4;
        [SerializeField] private Texture2D _icon5;
        [SerializeField] private Texture2D _icon6;

        [Header("布局")]
        [SerializeField] private float _iconSize = 80f;
        [SerializeField] private float _barWidth = 300f;
        [SerializeField] private float _barHeight = 20f;
        [SerializeField] private float _bottomMargin = 40f;

        [Header("颜色")]
        [SerializeField] private Color _barBgColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        [SerializeField] private Color _barFillColor = new Color(0.3f, 0.8f, 1f, 0.9f);
        [SerializeField] private Color _iconBgColor = new Color(0.15f, 0.15f, 0.15f, 0.85f);

        private EnergyManager _energy;
        private GUIStyle _numberStyle;

        private void Start()
        {
            _energy = EnergyManager.Instance;
            _numberStyle = new GUIStyle();
            _numberStyle.fontSize = 48;
            _numberStyle.normal.textColor = Color.white;
            _numberStyle.alignment = TextAnchor.MiddleCenter;
            _numberStyle.fontStyle = FontStyle.Bold;
        }

        private void OnGUI()
        {
            if (_energy == null) return;

            DrawCostArea();
            DrawProgressBar();
        }

        private void DrawCostArea()
        {
            float screenCenterX = Screen.width / 2f;
            float iconX = screenCenterX - _iconSize / 2f;
            float iconY = Screen.height - _bottomMargin - _barHeight - _iconSize - 12f;

            Rect bgRect = new Rect(iconX - 8, iconY - 8, _iconSize + 16, _iconSize + 16);
            GUI.color = _iconBgColor;
            GUI.DrawTexture(bgRect, Texture2D.whiteTexture);
            GUI.color = Color.white;

            Rect iconRect = new Rect(iconX, iconY, _iconSize, _iconSize);
            Texture2D costIcon = GetCostIcon(_energy.CurrentMaxEnergy);

            if (costIcon != null)
            {
                GUI.DrawTexture(iconRect, costIcon);
            }
            else
            {
                GUI.Box(iconRect, "");
                GUI.Label(iconRect, $"{_energy.CurrentEnergy}", _numberStyle);
            }
        }

        private void DrawProgressBar()
        {
            float screenCenterX = Screen.width / 2f;
            float barX = screenCenterX - _barWidth / 2f;
            float barY = Screen.height - _bottomMargin - _barHeight;

            Rect bgRect = new Rect(barX, barY, _barWidth, _barHeight);
            GUI.color = _barBgColor;
            GUI.DrawTexture(bgRect, Texture2D.whiteTexture);
            GUI.color = Color.white;

            float fillRatio = Mathf.Clamp01(_energy.RefillTimer / _energy.RefillInterval);
            Rect fillRect = new Rect(barX, barY, _barWidth * fillRatio, _barHeight);
            GUI.color = _barFillColor;
            GUI.DrawTexture(fillRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        private Texture2D GetCostIcon(int cost)
        {
            switch (cost)
            {
                case 3: return _icon3;
                case 4: return _icon4;
                case 5: return _icon5;
                case 6: return _icon6;
                default: return null;
            }
        }
    }
}
