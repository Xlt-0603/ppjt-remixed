using UnityEngine;

namespace PPCorps
{
    public class MetronomeUI : MonoBehaviour
    {
        [SerializeField] private bool _showDebugPanel = true;

        [Header("节拍可视化")]
        [SerializeField] private int _beatSquareSize = 80;
        [SerializeField] private int _beatGap = 16;
        [SerializeField] private Color _beatOn = new Color(0.3f, 1f, 0.3f);
        [SerializeField] private Color _beatOff = new Color(0.2f, 0.2f, 0.2f);
        [SerializeField] private Color _beatCurrent = new Color(0.1f, 0.6f, 1f);

        private GUIStyle _labelStyle;

        private void Awake()
        {
            _labelStyle = new GUIStyle();
            _labelStyle.fontSize = 32;
            _labelStyle.normal.textColor = Color.white;
            _labelStyle.alignment = TextAnchor.MiddleCenter;
        }

        private void OnGUI()
        {
            GameManager gm = GameManager.Instance;
            if (gm == null) return;

            DrawBeatVisualizer(gm);
            if (_showDebugPanel) DrawDebugPanel(gm);
        }

        private void DrawBeatVisualizer(GameManager gm)
        {
            int totalWidth = 8 * _beatSquareSize + 7 * _beatGap;
            float startX = (Screen.width - totalWidth) / 2f;
            float y = 16;

            for (int i = 0; i < 8; i++)
            {
                Rect rect = new Rect(startX + i * (_beatSquareSize + _beatGap), y, _beatSquareSize, _beatSquareSize);

                bool isCurrentBeat = (i + 1) == gm.Beat;
                bool isPastBeat = (i + 1) < gm.Beat;

                if (isCurrentBeat)
                    GUI.color = _beatCurrent;
                else if (isPastBeat)
                    GUI.color = _beatOn;
                else
                    GUI.color = _beatOff;

                GUI.DrawTexture(rect, Texture2D.whiteTexture);
                GUI.color = Color.white;

                GUI.Label(rect, $"{i + 1}", _labelStyle);
            }

            float barCenterX = startX + totalWidth / 2f;
            GUI.Label(new Rect(barCenterX - 120, y + _beatSquareSize + 6, 240, 36),
                $"小节 {gm.Bar}", _labelStyle);
        }

        private void DrawDebugPanel(GameManager gm)
        {
            float x = 10;
            float y = 10 + _beatSquareSize + 50;
            float w = 760;
            float h = 800;

            int origLabelSize = GUI.skin.label.fontSize;
            int origButtonSize = GUI.skin.button.fontSize;
            int origBoxSize = GUI.skin.box.fontSize;
            float origLabelH = GUI.skin.label.fixedHeight;
            GUI.skin.label.fontSize = 30;
            GUI.skin.label.fixedHeight = 36;
            GUI.skin.button.fontSize = 26;
            GUI.skin.box.fontSize = 30;

            GUI.Box(new Rect(x, y, w, h), "♩ 节拍器");

            GUILayout.BeginArea(new Rect(x + 10, y + 40, w - 20, h - 50));
            GUILayout.Label($"BPM: {(int)gm.BPM}", GUILayout.Height(36));
            gm.SetBPM(GUILayout.HorizontalSlider(gm.BPM, 10f, 240f));

            GUILayout.Space(8);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("-10", GUILayout.Height(40))) gm.SetBPM(gm.BPM - 10);
            if (GUILayout.Button("-1", GUILayout.Height(40))) gm.SetBPM(gm.BPM - 1);
            if (GUILayout.Button("+1", GUILayout.Height(40))) gm.SetBPM(gm.BPM + 1);
            if (GUILayout.Button("+10", GUILayout.Height(40))) gm.SetBPM(gm.BPM + 10);
            GUILayout.EndHorizontal();

            GUILayout.Space(14);
            GUILayout.Label($"状态: {gm.State}", GUILayout.Height(36));

            GUILayout.Space(14);
            GUILayout.BeginHorizontal();
            if (gm.State == GameState.Deploy)
            {
                if (GUILayout.Button("⚔ 开战", GUILayout.Height(48)))
                    gm.StartBattle();
            }
            else if (gm.State == GameState.Battle)
            {
                if (GUILayout.Button(gm.IsPaused ? "▶ 继续" : "⏸ 暂停", GUILayout.Height(48)))
                    gm.TogglePause();
                if (gm.IsPaused && GUILayout.Button("⏭ 步进", GUILayout.Height(48)))
                    gm.StepBeat();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(14);
            GUILayout.Label("--- 单位列表 ---", GUILayout.Height(36));

            var units = gm.GetAllUnits();
            foreach (var unit in units)
            {
                if (unit == null) continue;
                string prefix = unit.IsEnemy ? "🔴" : "🔵";
                string hpBar = HPToBar(unit.CurrentHP, unit.MaxHP);
                string action = unit.CurrentAction.ToString();
                string targetName = unit.CurrentTarget != null
                    ? $"{unit.CurrentTarget.name}"
                    : "-";
                GUILayout.Label($"{prefix} {unit.name}  {hpBar}  {action}→{targetName}", GUILayout.Height(36));
            }

            GUILayout.EndArea();

            GUI.skin.label.fontSize = origLabelSize;
            GUI.skin.label.fixedHeight = origLabelH;
            GUI.skin.button.fontSize = origButtonSize;
            GUI.skin.box.fontSize = origBoxSize;
        }

        private string HPToBar(int current, int max)
        {
            if (max <= 0) return "";
            int filled = Mathf.RoundToInt((float)current / max * 5);
            string bar = "";
            for (int i = 0; i < 5; i++)
                bar += i < filled ? "■" : "□";
            return $"[{bar}] {current}/{max}";
        }
    }
}
