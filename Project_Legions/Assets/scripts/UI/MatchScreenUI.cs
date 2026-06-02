using UnityEngine;

namespace PPCorps
{
    public class MatchScreenUI : MonoBehaviour
    {
        public Sprite matchImage;

        private GUIStyle _labelStyle;

        private void Awake()
        {
            _labelStyle = new GUIStyle();
            _labelStyle.fontSize = 40;
            _labelStyle.normal.textColor = Color.white;
            _labelStyle.alignment = TextAnchor.MiddleCenter;
        }

        private void OnGUI()
        {
            if (matchImage == null)
            {
                Debug.LogWarning("MatchScreenUI: matchImage is null");
                return;
            }

            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), matchImage.texture);
        }
    }
}
