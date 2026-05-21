using UnityEngine;

namespace PPCorps
{
    public class ChatBubbleUI : MonoBehaviour
    {
        [SerializeField] private GameObject _bubbleObject;
        [SerializeField] private float _displayDuration = 3f;

        private float _hideTimer;

        private void Start()
        {
            Hide();
        }

        public void Show(string text)
        {
            if (_bubbleObject != null)
            {
                _bubbleObject.SetActive(true);
                var textComp = _bubbleObject.GetComponentInChildren<UnityEngine.UI.Text>();
                if (textComp != null)
                    textComp.text = text;
            }
            _hideTimer = _displayDuration;
        }

        public void Hide()
        {
            if (_bubbleObject != null)
                _bubbleObject.SetActive(false);
        }

        private void Update()
        {
            if (_bubbleObject != null && _bubbleObject.activeSelf)
            {
                _hideTimer -= Time.deltaTime;
                if (_hideTimer <= 0)
                    Hide();
            }
        }
    }
}
