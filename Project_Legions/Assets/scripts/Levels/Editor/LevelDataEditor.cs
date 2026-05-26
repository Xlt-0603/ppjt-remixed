using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace PPCorps
{
    [CustomEditor(typeof(LevelData))]
    public class LevelDataEditor : Editor
    {
        private SerializedProperty _entries;
        private SerializedProperty _bpmOverride;
        private SerializedProperty _levelName;
        private GUIStyle _headerStyle;
        private GUIStyle _cellStyle;
        private GUIStyle _enemyLabelStyle;

        private void OnEnable()
        {
            _levelName = serializedObject.FindProperty("levelName");
            _bpmOverride = serializedObject.FindProperty("bpmOverride");
            _entries = serializedObject.FindProperty("entries");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawLevelHeader();
            DrawBasicSettings();
            EditorGUILayout.Space(10);
            DrawSpawnTable();
            EditorGUILayout.Space(6);
            DrawTimelinePreview();

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
                EditorUtility.SetDirty(target);
        }

        private void DrawLevelHeader()
        {
            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 14,
                    alignment = TextAnchor.MiddleCenter
                };
            }
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("关卡调试面板", _headerStyle);
            EditorGUILayout.Space(2);
        }

        private void DrawBasicSettings()
        {
            EditorGUILayout.PropertyField(_levelName, new GUIContent("关卡名称"));
            EditorGUILayout.PropertyField(_bpmOverride, new GUIContent("BPM 覆盖（0=默认）"));

            var totalEnemies = 0;
            for (int i = 0; i < _entries.arraySize; i++)
                if (_entries.GetArrayElementAtIndex(i).FindPropertyRelative("enemyData").objectReferenceValue != null)
                    totalEnemies++;
            EditorGUILayout.LabelField($"出场敌人总数: {totalEnemies}", EditorStyles.miniLabel);
        }

        private void DrawSpawnTable()
        {
            if (_cellStyle == null)
            {
                _cellStyle = new GUIStyle(EditorStyles.miniButton)
                {
                    fontSize = 10,
                    alignment = TextAnchor.MiddleCenter,
                    fixedHeight = 22
                };
                _enemyLabelStyle = new GUIStyle(EditorStyles.label)
                {
                    fontSize = 11,
                    alignment = TextAnchor.MiddleLeft
                };
            }

            EditorGUILayout.LabelField("出场表", EditorStyles.boldLabel);

            if (_entries.arraySize == 0)
            {
                EditorGUILayout.HelpBox("尚未添加出场条目，点击下方 [+] 添加", MessageType.Info);
            }
            else
            {
                DrawTableHeader();
                for (int i = 0; i < _entries.arraySize; i++)
                    DrawEntryRow(i);
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("+", GUILayout.Width(40), GUILayout.Height(22)))
            {
                _entries.InsertArrayElementAtIndex(_entries.arraySize);
                var newEntry = _entries.GetArrayElementAtIndex(_entries.arraySize - 1);
                newEntry.FindPropertyRelative("bar").intValue = 1;
                newEntry.FindPropertyRelative("beat").intValue = 1;
                newEntry.FindPropertyRelative("col").intValue = 0;
                newEntry.FindPropertyRelative("enemyData").objectReferenceValue = null;
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTableHeader()
        {
            Rect r = EditorGUILayout.GetControlRect(false, 20);
            float x = r.x;
            float w = r.width;

            float[] widths = { w * 0.22f, w * 0.12f, w * 0.10f, w * 0.42f, w * 0.14f };
            string[] headers = { "敌人", "小节", "拍", "列位置", "删除" };

            for (int i = 0; i < 5; i++)
            {
                GUI.Label(new Rect(x, r.y, widths[i], 20), headers[i], EditorStyles.boldLabel);
                x += widths[i];
            }
        }

        private void DrawEntryRow(int index)
        {
            SerializedProperty entry = _entries.GetArrayElementAtIndex(index);
            SerializedProperty enemyDataProp = entry.FindPropertyRelative("enemyData");
            SerializedProperty barProp = entry.FindPropertyRelative("bar");
            SerializedProperty beatProp = entry.FindPropertyRelative("beat");
            SerializedProperty colProp = entry.FindPropertyRelative("col");

            EditorGUILayout.BeginHorizontal();

            float totalW = EditorGUIUtility.currentViewWidth - 30;
            float[] widths = { totalW * 0.22f, totalW * 0.12f, totalW * 0.10f, totalW * 0.42f, totalW * 0.14f };

            EditorGUILayout.PropertyField(enemyDataProp, GUIContent.none, GUILayout.Width(widths[0]));

            barProp.intValue = EditorGUILayout.IntField(barProp.intValue, GUILayout.Width(widths[1]));
            barProp.intValue = Mathf.Max(1, barProp.intValue);

            beatProp.intValue = EditorGUILayout.IntField(beatProp.intValue, GUILayout.Width(widths[2]));
            beatProp.intValue = Mathf.Clamp(beatProp.intValue, 1, 8);

            int col = EditorGUILayout.IntSlider(colProp.intValue, 0, 23, GUILayout.Width(widths[3]));
            colProp.intValue = col;

            if (GUILayout.Button("×", GUILayout.Width(widths[4]), GUILayout.Height(18)))
            {
                _entries.DeleteArrayElementAtIndex(index);
                return;
            }

            EditorGUILayout.EndHorizontal();

            var dataObj = enemyDataProp.objectReferenceValue as UnitData;
            if (dataObj != null && dataObj.icon != null)
            {
                string info = $"{dataObj.unitName}  HP:{dataObj.maxHP}  ATK:{dataObj.attackPower}";
                EditorGUILayout.LabelField(info, EditorStyles.miniLabel);
            }
        }

        private void DrawTimelinePreview()
        {
            EditorGUILayout.LabelField("时间轴预览（节拍 → 列位置）", EditorStyles.boldLabel);

            if (_entries.arraySize == 0) return;

            int maxBar = 1;
            for (int i = 0; i < _entries.arraySize; i++)
            {
                int b = _entries.GetArrayElementAtIndex(i).FindPropertyRelative("bar").intValue;
                if (b > maxBar) maxBar = b;
            }

            int totalBeats = maxBar * 8;
            int previewHeight = 80;

            Rect area = EditorGUILayout.GetControlRect(false, previewHeight + 20);

            float areaX = area.x;
            float areaY = area.y;
            float areaW = area.width;
            float areaH = previewHeight;

            EditorGUI.DrawRect(new Rect(areaX, areaY, areaW, areaH), new Color(0.15f, 0.15f, 0.15f));

            float beatW = areaW / totalBeats;
            float colH = areaH / 24f;

            for (int i = 0; i < _entries.arraySize; i++)
            {
                var entry = _entries.GetArrayElementAtIndex(i);
                var dataObj = entry.FindPropertyRelative("enemyData").objectReferenceValue as UnitData;
                if (dataObj == null) continue;

                int bar = entry.FindPropertyRelative("bar").intValue;
                int beat = entry.FindPropertyRelative("beat").intValue;
                int col = entry.FindPropertyRelative("col").intValue;

                float x = areaX + ((bar - 1) * 8 + (beat - 1)) * beatW;
                float y = areaY + areaH - (col + 1) * colH;

                Color c = dataObj.unitType == UnitType.Ranged ? new Color(0.3f, 0.8f, 1f) : new Color(1f, 0.4f, 0.4f);
                EditorGUI.DrawRect(new Rect(x, y, Mathf.Max(beatW - 1, 2), Mathf.Max(colH - 1, 2)), c);
            }

            for (int b = 0; b < totalBeats; b++)
            {
                float x = areaX + b * beatW;
                if (b % 8 == 0)
                    EditorGUI.DrawRect(new Rect(x, areaY, 1, areaH), new Color(1, 1, 1, 0.5f));
                else if (b % 4 == 0)
                    EditorGUI.DrawRect(new Rect(x, areaY, 1, areaH), new Color(1, 1, 1, 0.2f));
            }

            EditorGUI.DrawRect(new Rect(areaX, areaY + areaH, areaW, 1), new Color(0.5f, 0.5f, 0.5f));
            EditorGUI.DrawRect(new Rect(areaX, areaY, 1, areaH), new Color(0.5f, 0.5f, 0.5f));

            GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel) { fontSize = 8 };
            for (int b = 0; b <= maxBar; b++)
            {
                float x = areaX + b * 8 * beatW;
                if (x < areaX + areaW - 10)
                    GUI.Label(new Rect(x - 5, areaY + areaH + 2, 20, 14), $"第{b + 1}节", labelStyle);
            }

            EditorGUILayout.LabelField("色块: 敌人   |   横向=时间  纵向=列  白线=每节开头", EditorStyles.miniLabel);
        }

        [MenuItem("GameObject/砰砰军团/关卡控制器", false, 10)]
        private static void CreateLevelController(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("Level Controller");
            go.AddComponent<LevelController>();
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "创建关卡控制器");
            Selection.activeObject = go;
        }
    }
}
