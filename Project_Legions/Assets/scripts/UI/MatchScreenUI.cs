using UnityEngine;
using UnityEngine.SceneManagement;

namespace PPCorps
{
    public class MatchScreenUI : MonoBehaviour
    {
        public Sprite matchImage;

        [SerializeField] private Rect _clickRect = new Rect(0.75f, 0.3f, 0.2f, 0.2f);

        private bool _isLoading;
        private GUIStyle _labelStyle;
        private GUIStyle _invisibleBtn;

        private void Awake()
        {
            _labelStyle = new GUIStyle();
            _labelStyle.fontSize = 40;
            _labelStyle.normal.textColor = Color.white;
            _labelStyle.alignment = TextAnchor.MiddleCenter;

            _invisibleBtn = new GUIStyle();
            _invisibleBtn.normal.background = null;
        }

        private void OnGUI()
        {
            if (matchImage == null)
            {
                Debug.LogWarning("MatchScreenUI: matchImage is null");
                return;
            }

            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), matchImage.texture);

            if (_isLoading)
            {
                GUI.color = new Color(0, 0, 0, 0.6f);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
                GUI.color = Color.white;
                GUI.Label(new Rect(0, Screen.height / 2f - 30, Screen.width, 60), "Loading...", _labelStyle);
                return;
            }

            float bx = _clickRect.x * Screen.width;
            float by = _clickRect.y * Screen.height;
            float bw = _clickRect.width * Screen.width;
            float bh = _clickRect.height * Screen.height;

            if (GUI.Button(new Rect(bx, by, bw, bh), "", _invisibleBtn))
            {
                _isLoading = true;
                Invoke(nameof(LoadBattle), 0.15f);
            }
        }

        private void LoadBattle()
        {
            Debug.Log("MatchScreenUI: loading battlescene...");
            SceneManager.LoadScene("battlescene");
        }
    }
}
