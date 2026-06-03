using UnityEditor;
using UnityEngine;

namespace PPCorps
{
    [CustomEditor(typeof(UnitData))]
    public class UnitDataEditor : Editor
    {
        private SerializedProperty _unitName;
        private SerializedProperty _prefab;
        private SerializedProperty _icon;
        private SerializedProperty _unitType;
        private SerializedProperty _unitClass;
        private SerializedProperty _maxHP;
        private SerializedProperty _attackPower;
        private SerializedProperty _attackRange;
        private SerializedProperty _moveSpeed;
        private SerializedProperty _deployCost;
        private SerializedProperty _preferFarthestTarget;
        private SerializedProperty _isVanguard;
        private SerializedProperty _vanguardScanMin;
        private SerializedProperty _vanguardScanMax;
        private SerializedProperty _vanguardDamage;
        private SerializedProperty _vanguardChargeBeats;

        private SerializedProperty[] _attackOnBeat = new SerializedProperty[8];
        private SerializedProperty[] _animStartBeat = new SerializedProperty[8];
        private SerializedProperty[] _animEndBeat = new SerializedProperty[8];

        private void OnEnable()
        {
            _unitName = serializedObject.FindProperty("unitName");
            _prefab = serializedObject.FindProperty("prefab");
            _icon = serializedObject.FindProperty("icon");
            _unitType = serializedObject.FindProperty("unitType");
            _unitClass = serializedObject.FindProperty("unitClass");
            _maxHP = serializedObject.FindProperty("maxHP");
            _attackPower = serializedObject.FindProperty("attackPower");
            _attackRange = serializedObject.FindProperty("attackRange");
            _moveSpeed = serializedObject.FindProperty("moveSpeed");
            _deployCost = serializedObject.FindProperty("deployCost");
            _preferFarthestTarget = serializedObject.FindProperty("preferFarthestTarget");
            _isVanguard = serializedObject.FindProperty("isVanguard");
            _vanguardScanMin = serializedObject.FindProperty("vanguardScanMin");
            _vanguardScanMax = serializedObject.FindProperty("vanguardScanMax");
            _vanguardDamage = serializedObject.FindProperty("vanguardDamage");
            _vanguardChargeBeats = serializedObject.FindProperty("vanguardChargeBeats");

            for (int i = 0; i < 8; i++)
            {
                int n = i + 1;
                _attackOnBeat[i] = serializedObject.FindProperty($"attackOnBeat{n}");
                _animStartBeat[i] = serializedObject.FindProperty($"animStartBeat{n}");
                _animEndBeat[i] = serializedObject.FindProperty($"animEndBeat{n}");
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawCommonFields();
            EditorGUILayout.Space(6);
            DrawRhythmSection();
            EditorGUILayout.Space(6);
            DrawVanguardSection();

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
                EditorUtility.SetDirty(target);
        }

        private void DrawCommonFields()
        {
            EditorGUILayout.PropertyField(_unitName);
            EditorGUILayout.PropertyField(_prefab);
            EditorGUILayout.PropertyField(_icon);
            EditorGUILayout.PropertyField(_unitType);
            EditorGUILayout.PropertyField(_unitClass);
            EditorGUILayout.PropertyField(_maxHP);
            EditorGUILayout.PropertyField(_attackPower);
            EditorGUILayout.PropertyField(_attackRange);
            EditorGUILayout.PropertyField(_moveSpeed);
            EditorGUILayout.PropertyField(_deployCost);
            EditorGUILayout.PropertyField(_preferFarthestTarget);
        }

        private void DrawRhythmSection()
        {
            EditorGUILayout.LabelField("攻击节奏（8拍）", EditorStyles.boldLabel);

            for (int i = 0; i < 8; i++)
            {
                int beatNum = i + 1;
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField($"第{beatNum}拍", GUILayout.Width(50));

                _attackOnBeat[i].boolValue = EditorGUILayout.Toggle(_attackOnBeat[i].boolValue, GUILayout.Width(20));

                if (_attackOnBeat[i].boolValue)
                {
                    EditorGUILayout.LabelField("动画起止", GUILayout.Width(60));
                    _animStartBeat[i].intValue = EditorGUILayout.IntField(_animStartBeat[i].intValue, GUILayout.Width(40));
                    EditorGUILayout.LabelField("~", GUILayout.Width(12));
                    _animEndBeat[i].intValue = EditorGUILayout.IntField(_animEndBeat[i].intValue, GUILayout.Width(40));
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawVanguardSection()
        {
            EditorGUILayout.PropertyField(_isVanguard);
            if (_isVanguard.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_vanguardScanMin);
                EditorGUILayout.PropertyField(_vanguardScanMax);
                EditorGUILayout.PropertyField(_vanguardDamage);
                EditorGUILayout.PropertyField(_vanguardChargeBeats);
                EditorGUI.indentLevel--;
            }
        }
    }
}
