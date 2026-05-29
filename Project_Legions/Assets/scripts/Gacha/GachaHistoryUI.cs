using System.Collections.Generic;
using UnityEngine;

namespace PPCorps
{
    public class GachaHistoryUI : MonoBehaviour
    {
        private GachaSystem _gachaSystem;

        private bool _show;
        private Vector2 _scrollPos;

        private void Awake()
        {
            _gachaSystem = GetComponent<GachaSystem>();
        }

        private void OnGUI()
        {
            if (!_show) return;

            float w = Screen.width * 0.8f;
            float x = (Screen.width - w) / 2f;
            float h = Screen.height * 0.7f;
            float y = 30;

            GUI.Box(new Rect(x, y, w, h), "抽卡记录");

            Rect viewRect = new Rect(0, 0, w - 20, GetContentHeight());
            _scrollPos = GUI.BeginScrollView(new Rect(x, y + 25, w, h - 30), _scrollPos, viewRect);

            float lineY = 0;
            var history = _gachaSystem.GetHistory();
            foreach (var record in history)
            {
                string[] names = record.GetItemNames();
                if (names.Length == 0) continue;

                GUI.Label(new Rect(10, lineY, w - 40, 20), record.timestamp);
                lineY += 22;

                float itemX = 10;
                foreach (var name in names)
                {
                    GUI.Label(new Rect(itemX, lineY, 120, 20), name);
                    itemX += 125;
                }
                lineY += 22;
            }

            GUI.EndScrollView();

            if (GUI.Button(new Rect(Screen.width / 2f - 50, Screen.height - 60, 100, 30), "关闭"))
                _show = false;
        }

        private float GetContentHeight()
        {
            var history = _gachaSystem.GetHistory();
            float h = 0;
            foreach (var record in history)
                h += 44;
            return h + 40;
        }

        public void Toggle()
        {
            _show = !_show;
        }
    }
}
