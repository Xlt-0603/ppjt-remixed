using UnityEngine;
using UnityEngine.SceneManagement;

namespace PPCorps
{
    public class GameOverUI : MonoBehaviour
    {
        [Header("结算图片")]
        public Texture2D winImage;
        public Texture2D loseImage;

        [Header("动画参数")]
        public float fadeDuration = 1f;
        public float slideDuration = 0.6f;
        public float darkAlpha = 0.7f;
        public float holdBeforeReturn = 1f;

        private enum AnimState { Idle, FadeIn, SlideIn, Show }

        private AnimState _state = AnimState.Idle;
        private float _timer;
        private GameState _result;
        private float _slideStartY;
        private float _slideTargetY;
        private float _imageWidth;
        private float _imageHeight;
        private bool _canReturn;

        private void Start()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged += OnGameStateChanged;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged -= OnGameStateChanged;
        }

        private void OnGameStateChanged(GameState state)
        {
            if (state != GameState.Win && state != GameState.Lose)
                return;

            _result = state;
            _state = AnimState.FadeIn;
            _timer = 0f;
            _canReturn = false;

            Texture2D img = state == GameState.Win ? winImage : loseImage;
            if (img != null)
            {
                float screenW = Screen.width;
                float screenH = Screen.height;
                float imgAspect = (float)img.width / img.height;
                float screenAspect = screenW / screenH;

                if (imgAspect > screenAspect)
                {
                    _imageWidth = screenW * 0.8f;
                    _imageHeight = _imageWidth / imgAspect;
                }
                else
                {
                    _imageHeight = screenH * 0.6f;
                    _imageWidth = _imageHeight * imgAspect;
                }
            }

            _slideStartY = -_imageHeight;
            _slideTargetY = (Screen.height - _imageHeight) / 2f;
        }

        private void Update()
        {
            if (_state == AnimState.Idle)
                return;

            _timer += Time.deltaTime;

            if (_state == AnimState.FadeIn && _timer >= fadeDuration)
            {
                _state = AnimState.SlideIn;
                _timer = 0f;
            }
            else if (_state == AnimState.SlideIn && _timer >= slideDuration)
            {
                _state = AnimState.Show;
                _timer = 0f;
            }
            else if (_state == AnimState.Show && _timer >= holdBeforeReturn)
            {
                _canReturn = true;
            }

            if (_canReturn && Input.anyKeyDown)
                SceneManager.LoadScene("MatchScene");
        }

        private void OnGUI()
        {
            if (_state == AnimState.Idle)
                return;

            GUI.depth = -1;

            float fadeProgress = Mathf.Clamp01(_timer / fadeDuration);
            float alpha = _state == AnimState.FadeIn
                ? fadeProgress * darkAlpha
                : darkAlpha;

            GUI.color = new Color(0, 0, 0, alpha);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            Texture2D img = _result == GameState.Win ? winImage : loseImage;
            if (img == null)
            {
                DrawFallbackText();
                return;
            }

            float imgY;
            if (_state == AnimState.FadeIn)
            {
                imgY = _slideStartY;
            }
            else if (_state == AnimState.SlideIn)
            {
                float t = Mathf.Clamp01((_timer) / slideDuration);
                t = t * t * (3f - 2f * t);
                imgY = Mathf.Lerp(_slideStartY, _slideTargetY, t);
            }
            else
            {
                imgY = _slideTargetY;
            }

            float imgX = (Screen.width - _imageWidth) / 2f;
            GUI.DrawTexture(new Rect(imgX, imgY, _imageWidth, _imageHeight), img);

            if (_canReturn)
                DrawReturnHint();
        }

        private void DrawFallbackText()
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 50;
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleCenter;

            string text = _result == GameState.Win ? "胜利" : "失败";
            float y = _state == AnimState.Show ? Screen.height / 2f : _slideStartY + Screen.height / 2f;
            GUI.Label(new Rect(0, y, Screen.width, 80), text, style);
        }

        private void DrawReturnHint()
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 28;
            style.normal.textColor = new Color(1, 1, 1, 0.7f);
            style.alignment = TextAnchor.MiddleCenter;

            float y = _slideTargetY + _imageHeight + 40;
            GUI.Label(new Rect(0, y, Screen.width, 50), "点击任意键返回", style);
        }
    }
}
