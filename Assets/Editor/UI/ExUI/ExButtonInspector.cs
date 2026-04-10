using HotAssets.Scripts.UI.Tool.Component;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Editor.UI.ExUI
{
    [CustomEditor(typeof(ExButton))]
    public class ExButtonInspector : UnityEditor.Editor
    {

        private bool m_ButtonInspector = false;
        private bool m_Extend = true;
    
        private enum ButtonTypeDes
        {
            点击,
            间隔,
            长按,
            点击加长按,
            长按循环,
            点击加长按循环
        }

        private ButtonTypeDes m_ButtonTypeDes = ButtonTypeDes.点击;
    
        private ExButton _target;
        private SerializedProperty m_ClickInterva;
        private SerializedProperty m_IsGray;
        private SerializedProperty m_LongPressOnTime;
        private SerializedProperty m_Sound;
        private SerializedProperty m_CustomPolygon;
        private SerializedProperty m_StateObjList;
        private SerializedProperty m_SoundId;

        private ExUICreateConfig _createConfig;
        
        private void OnEnable()
        {
            _target = (ExButton)serializedObject.targetObject;
            m_ClickInterva = serializedObject.FindProperty("ClickInterva");
            m_IsGray = serializedObject.FindProperty("IsGray");
            m_LongPressOnTime = serializedObject.FindProperty("LongPressOnTime");
            m_CustomPolygon = serializedObject.FindProperty("CustomPolygon");
            m_StateObjList = serializedObject.FindProperty("StateObjList");
            m_SoundId = serializedObject.FindProperty("SoundId");
            _createConfig = ExUIConfig.LoadAsset<ExUICreateConfig>("ExUICreateConfig");
            m_ButtonTypeDes = (ButtonTypeDes)_target.m_ButtonType;
        }

        public override void OnInspectorGUI()
        {
            if (_createConfig && _createConfig.ScriptIcon != null)
            {
                EditorGUIUtility.SetIconForObject(target, _createConfig.ScriptIcon);
            }
            
            serializedObject.Update();
        
            m_ButtonInspector = EditorGUILayout.Foldout(m_ButtonInspector, "按钮");
        
            if (m_ButtonInspector)
            {
                base.OnInspectorGUI();
            }
        

            m_Extend = EditorGUILayout.Foldout(m_Extend, new GUIContent("扩展", "按钮功能扩展"));

            if (m_Extend)
            {
                EditorGUILayout.BeginVertical("box");

                m_ButtonTypeDes =
                    (ButtonTypeDes)EditorGUILayout.EnumPopup(new GUIContent("类型", "请选择按钮类型"), m_ButtonTypeDes);
            
                _target.m_ButtonType = (ExButton.ButtonType)m_ButtonTypeDes;
            
                if (_target.m_ButtonType == ExButton.ButtonType.Interva)
                {
                    EditorGUILayout.PropertyField(m_ClickInterva, new GUIContent("间隔时间"));
                    EditorGUILayout.PropertyField(m_IsGray, new GUIContent("置灰效果","在按钮无法点击的间隔时间内使用置灰效果"));
                }
                else if (_target.m_ButtonType == ExButton.ButtonType.LongPressOn 
                         || _target.m_ButtonType == ExButton.ButtonType.ClickAndLongPressOn 
                         || _target.m_ButtonType == ExButton.ButtonType.LongPressOnLoop
                         || _target.m_ButtonType == ExButton.ButtonType.ClickAndLongPressOnLoop)
                {
                    EditorGUILayout.PropertyField(m_LongPressOnTime, new GUIContent("长按响应时间"));
                }
     
                EditorGUILayout.PropertyField(m_CustomPolygon, new GUIContent("自定义点击区域","不规则点击时使用该功能"));
                EditorGUILayout.PropertyField(m_SoundId, new GUIContent("点击音效"));   
                EditorGUILayout.PropertyField(m_StateObjList, new GUIContent("显示样式","按钮需要使用多种样式切换时使用"));
                EditorGUILayout.EndVertical();
            }            

            //修改序列化对象的属性
            serializedObject.ApplyModifiedProperties();   
        }

        [MenuItem("GameObject/UGF_UI/ExButton", priority = 3)]
        static void AddUGFButton()
        {
            CreateUGFButton(Selection.activeTransform);
        }

        static public ExButton CreateUGFButton(Transform parent)
        {
            GameObject obj = new GameObject("UGFButton");
            ExUICreateConfig uGFUICreateConfig = ExUIConfig.LoadAsset<ExUICreateConfig>("ExUICreateConfig");

            if (parent)
            {
                obj.transform.SetParent(parent, false);
            }

            obj.transform.localPosition = Vector3.zero;

            ExButton exButton = obj.AddComponent<ExButton>();
            exButton.GetComponent<RectTransform>().sizeDelta = new Vector2(218, 138);
            exButton.ButtonText = ExTextInspector.CreateUGFText(obj.transform);
      
            HotAssets.Scripts.UI.Tool.Component.ExImage img = obj.GetComponent<HotAssets.Scripts.UI.Tool.Component.ExImage>();
            img.maskable = false;
            exButton.targetGraphic = img;

            if (uGFUICreateConfig.ButtonDefaultImage2 != null)
            {
                exButton.image.sprite = uGFUICreateConfig.ButtonDefaultImage2;
            }

            if (uGFUICreateConfig.ButtonOnClickId != null)
            {
                exButton.SoundId = uGFUICreateConfig.ButtonOnClickId;
            }

            if (uGFUICreateConfig.ButtonTransition != null)
            {
                exButton.transition = uGFUICreateConfig.ButtonTransition;
            
                if (uGFUICreateConfig.ButtonTransition == Selectable.Transition.Animation)
                {
                    Animator animator = exButton.gameObject.AddComponent<Animator>();
                    animator.runtimeAnimatorController = uGFUICreateConfig.ButtonAnimatorController;
                }
            }

            return exButton;
        }
    }
}
