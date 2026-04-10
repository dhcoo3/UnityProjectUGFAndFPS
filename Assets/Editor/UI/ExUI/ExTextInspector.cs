using HotAssets.Scripts.UI.Tool.Component;
using HotAssets.Scripts.UI.Tool.OutLine;
using UnityEditor;
using UnityEngine;

namespace Editor.UI.ExUI
{
    [CustomEditor(typeof(ExText))]
    public class ExTextInspector : UnityEditor.Editor
    {
        private bool m_FuncShow = true;

        private ExText _target;
        public SerializedProperty m_UseLocalization;
        public SerializedProperty m_LocalizationKey;
        private ExUICreateConfig _createConfig;
        
        private void OnEnable()
        {
            _target = (ExText)serializedObject.targetObject;
            m_UseLocalization = serializedObject.FindProperty("UseLocalization");
            m_LocalizationKey = serializedObject.FindProperty("LocalizationKey");
            _createConfig = ExUIConfig.LoadAsset<ExUICreateConfig>("ExUICreateConfig");
            _target.CheckLocalization();
        }

        public override void OnInspectorGUI()
        {
            if (_createConfig && _createConfig.ScriptIcon != null)
            {
                EditorGUIUtility.SetIconForObject(target, _createConfig.ScriptIcon);
            }
            
            serializedObject.Update();

            base.OnInspectorGUI();
            m_FuncShow = EditorGUILayout.Foldout(m_FuncShow, "设置");

            if (m_FuncShow)
            {
                EditorGUILayout.PropertyField(m_UseLocalization, new GUIContent("自动本地化文本"));

                if (_target.UseLocalization)
                {
                    EditorGUILayout.PropertyField(m_LocalizationKey, new GUIContent("本地化Key"));
                }

                EditorGUILayout.Space();

                if (GUILayout.Button("添加描边和底纹组件"))
                {
                    AddTMPOutlineController();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// 添加 TMPOutline 组件
        /// </summary>
        private void AddTMPOutlineController()
        {
            if (_target.gameObject.GetComponent<TMPOutline>() == null)
            {
                Undo.AddComponent<TMPOutline>(_target.gameObject);
                EditorUtility.DisplayDialog("成功", "已添加 TMPOutline 组件", "确定");
            }
            else
            {
                EditorUtility.DisplayDialog("提示", "该对象已存在 TMPOutline 组件", "确定");
            }
        }

        [MenuItem("GameObject/UGF_UI/ExText", priority = 6)]
        public static ExText AddUGFText()
        {
            return CreateUGFText(Selection.activeTransform);
        }

        public static ExText CreateUGFText(Transform parent)
        {
            GameObject obj = new GameObject("UGFText");
            ExUICreateConfig config = ExUIConfig.LoadAsset<ExUICreateConfig>("ExUICreateConfig");

            if (parent)
            {
                obj.transform.SetParent(parent, false);
            }

            obj.transform.localPosition = Vector3.zero;
            ExText text = obj.AddComponent<ExText>();
            text.TMP.font = config.DefaultFont;
            text.TMP.color = Color.white;
            text.TMP.raycastTarget = false;
            text.TMP.fontSize = 30;
            text.TMP.text = "Text...";
            text.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 60);
            Selection.activeGameObject = obj;

            return text;
        }
    }
}