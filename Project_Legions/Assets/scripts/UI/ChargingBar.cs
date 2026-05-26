using UnityEngine;

namespace PPCorps
{
    public class ChargingBar : MonoBehaviour
    {
        [Header("周期")]
        [SerializeField] private int _cycleBars = 15;

        [Header("尺寸")]
        [SerializeField] private int _width = 300;
        [SerializeField] private int _height = 24;

        [Header("颜色")]
        [SerializeField] private Color _emptyColor = new Color(0.6f, 0.6f, 0.6f);
        [SerializeField] private Color _fillColor = new Color(1f, 0.84f, 0f);

        [Header("边框")]
        [SerializeField] private int _borderWidth = 2;

        private float _progress;
        private Texture2D _whiteTex;

        private void Start()
        {
            _progress = 0f;
            _whiteTex = Texture2D.whiteTexture;

            if (GameManager.Instance != null)
                GameManager.Instance.OnBeat += OnGameBeat;
        }

        private void OnGameBeat(int bar, int beat)
        {
            if (GameManager.Instance == null || GameManager.Instance.State != GameState.Battle) return;

            float step = 1f / (_cycleBars * 8);
            _progress = Mathf.Clamp01(_progress + step);

            if (_progress >= 1f)
                _progress = 0f;
        }

        private void OnGUI()
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            float guiX = screenPos.x - _width * 0.5f;
            float guiY = Screen.height - screenPos.y - _height * 0.5f;

            Rect outerRect = new Rect(guiX, guiY, _width, _height);
            Color boarderColor = Color.Lerp(_emptyColor, _fillColor, _progress);

            DrawRect(outerRect, boarderColor);

            Rect bgRect = new Rect(
                guiX + _borderWidth,
                guiY + _borderWidth,
                _width - _borderWidth * 2,
                _height - _borderWidth * 2
            );
            DrawRect(bgRect, Color.black);

            float fillWidth = bgRect.width * _progress;
            if (fillWidth > 0f)
            {
                Rect fillRect = new Rect(bgRect.x, bgRect.y, fillWidth, bgRect.height);
                DrawRect(fillRect, _fillColor);
            }
        }

        private void DrawRect(Rect rect, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(rect, _whiteTex);
            GUI.color = Color.white;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnBeat -= OnGameBeat;
        }
    }
}
