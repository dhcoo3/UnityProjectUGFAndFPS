using HotAssets.Scripts.UI.Tool.OutLine;
using UnityEditor;
using UnityEngine;

namespace Editor.UI
{
    /// <summary>
    /// TMPOutline 自定义编辑器
    /// 提供描边和底纹的实时预览和便捷操作
    /// </summary>
    [CustomEditor(typeof(TMPOutline))]
    public class TMPOutlineEditor : UnityEditor.Editor
    {
        private SerializedProperty enableOutlineProp;
        private SerializedProperty outlineWidthProp;
        private SerializedProperty outlineColorProp;
        private SerializedProperty enableUnderlayProp;
        private SerializedProperty underlayColorProp;
        private SerializedProperty underlayOffsetXProp;
        private SerializedProperty underlayOffsetYProp;
        private SerializedProperty underlayDilateProp;
        private SerializedProperty underlaySoftnessProp;

        private void OnEnable()
        {
            enableOutlineProp    = serializedObject.FindProperty("enableOutline");
            outlineWidthProp     = serializedObject.FindProperty("outlineWidth");
            outlineColorProp     = serializedObject.FindProperty("outlineColor");
            enableUnderlayProp   = serializedObject.FindProperty("enableUnderlay");
            underlayColorProp    = serializedObject.FindProperty("underlayColor");
            underlayOffsetXProp  = serializedObject.FindProperty("underlayOffsetX");
            underlayOffsetYProp  = serializedObject.FindProperty("underlayOffsetY");
            underlayDilateProp   = serializedObject.FindProperty("underlayDilate");
            underlaySoftnessProp = serializedObject.FindProperty("underlaySoftness");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            // ---- 描边设置 ----
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("描边设置", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(enableOutlineProp, new GUIContent("启用描边"));
            if (enableOutlineProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(outlineWidthProp, new GUIContent("描边宽度"));
                EditorGUILayout.PropertyField(outlineColorProp, new GUIContent("描边颜色"));

                EditorGUILayout.Space();

                // 描边快捷预设
                EditorGUILayout.LabelField("描边预设", EditorStyles.miniLabel);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("无")) outlineWidthProp.floatValue = 0f;
                if (GUILayout.Button("细")) outlineWidthProp.floatValue = 0.1f;
                if (GUILayout.Button("中")) outlineWidthProp.floatValue = 0.2f;
                if (GUILayout.Button("粗")) outlineWidthProp.floatValue = 0.4f;
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }

            // ---- 底纹设置 ----
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("底纹设置", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(enableUnderlayProp, new GUIContent("启用底纹"));
            if (enableUnderlayProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(underlayColorProp,    new GUIContent("底纹颜色"));
                EditorGUILayout.PropertyField(underlayOffsetXProp,  new GUIContent("X 偏移"));
                EditorGUILayout.PropertyField(underlayOffsetYProp,  new GUIContent("Y 偏移"));
                EditorGUILayout.PropertyField(underlayDilateProp,   new GUIContent("扩散"));
                EditorGUILayout.PropertyField(underlaySoftnessProp, new GUIContent("柔软度"));

                EditorGUILayout.Space();

                // 底纹快捷预设
                EditorGUILayout.LabelField("底纹预设", EditorStyles.miniLabel);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("无底纹"))
                {
                    underlayColorProp.colorValue = new Color(0, 0, 0, 0);
                }
                if (GUILayout.Button("阴影"))
                {
                    underlayColorProp.colorValue    = new Color(0, 0, 0, 0.5f);
                    underlayOffsetXProp.floatValue  = 0.5f;
                    underlayOffsetYProp.floatValue  = -0.5f;
                    underlayDilateProp.floatValue   = 0f;
                    underlaySoftnessProp.floatValue = 0.05f;
                }
                if (GUILayout.Button("模糊阴影"))
                {
                    underlayColorProp.colorValue    = new Color(0, 0, 0, 0.4f);
                    underlayOffsetXProp.floatValue  = 0.5f;
                    underlayOffsetYProp.floatValue  = -0.5f;
                    underlayDilateProp.floatValue   = 0.2f;
                    underlaySoftnessProp.floatValue = 0.3f;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                SceneView.RepaintAll();
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "此组件为每个 TextMeshPro 创建独立材质实例，修改不会影响其他文字。",
                MessageType.Info
            );
        }
    }
}