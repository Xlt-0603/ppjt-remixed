using UnityEngine;
using UnityEngine.SceneManagement;

namespace PPCorps
{
    public class StartScreen : MonoBehaviour
    {
        public Sprite backgroundImage;
        public string nextSceneName;

        private bool _isLoading;
        private GUIStyle _labelStyle;

        private void Awake()
        {
            _labelStyle = new GUIStyle
            {
                fontSize = 40,
                alignment = TextAnchor.MiddleCenter
            };
            _labelStyle.normal.textColor = Color.white;
        }

        private void Update()
        {
            if (_isLoading) return;
            if (string.IsNullOrEmpty(nextSceneName)) return;

            if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
            {
                _isLoading = true;
                Invoke(nameof(LoadNextScene), 0.15f);
            }
        }

        private void LoadNextScene()
        {
            SceneManager.LoadScene(nextSceneName);
        }

        private void OnGUI()
        {
            if (backgroundImage == null) return;

            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), backgroundImage.texture);

            if (_isLoading)
            {
                GUI.color = new Color(0, 0, 0, 0.6f);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
                GUI.color = Color.white;
                GUI.Label(new Rect(0, Screen.height / 2f - 30, Screen.width, 60), "Loading...", _labelStyle);
            }
        }
    }
}
