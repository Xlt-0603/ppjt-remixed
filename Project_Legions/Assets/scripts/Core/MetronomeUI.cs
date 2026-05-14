using UnityEngine;

namespace PPCorps
{
    public class MetronomeUI : MonoBehaviour
    {
        [SerializeField] private bool _showDebugPanel = true;

        private void OnGUI()
        {
            if (!_showDebugPanel) return;

            GameManager gm = GameManager.Instance;
            if (gm == null) return;

            float x = 10;
            float y = 10;
            float w = 420;
            float h = 350;

            GUI.Box(new Rect(x, y, w, h), "♩ 节拍器");

            GUILayout.BeginArea(new Rect(x + 5, y + 25, w - 10, h - 30));
            GUILayout.Label($"BPM: {(int)gm.BPM}");
            gm.SetBPM(GUILayout.HorizontalSlider(gm.BPM, 10f, 240f));

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("-10")) gm.SetBPM(gm.BPM - 10);
            if (GUILayout.Button("-1")) gm.SetBPM(gm.BPM - 1);
            if (GUILayout.Button("+1")) gm.SetBPM(gm.BPM + 1);
            if (GUILayout.Button("+10")) gm.SetBPM(gm.BPM + 10);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            GUILayout.Label($"小节: {gm.Bar}   拍: {gm.Beat}");
            GUILayout.Label($"状态: {gm.State}");

            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            if (gm.State == GameState.Deploy)
            {
                if (GUILayout.Button("⚔ 开战", GUILayout.Height(30)))
                    gm.StartBattle();
            }
            else if (gm.State == GameState.Battle)
            {
                if (GUILayout.Button(gm.IsPaused ? "▶ 继续" : "⏸ 暂停"))
                    gm.TogglePause();
                if (gm.IsPaused && GUILayout.Button("⏭ 步进"))
                    gm.StepBeat();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            GUILayout.Label("--- 单位列表 ---");

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
                GUILayout.Label($"{prefix} {unit.name}  {hpBar}  {action}→{targetName}");
            }

            GUILayout.EndArea();
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
