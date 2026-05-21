using UnityEngine;
using UnityEngine.UI;

namespace PPCorps
{
    public class ScrollButton : MonoBehaviour
    {
        public enum Direction { Left, Right }

        public Direction direction;

        private void Start()
        {
            var btn = GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(OnClick);
        }

        public void OnClick()
        {
            var ctrl = FindObjectOfType<VillageCameraController>();
            if (direction == Direction.Left)
                ctrl?.ScrollLeft();
            else
                ctrl?.ScrollRight();
        }
    }
}
