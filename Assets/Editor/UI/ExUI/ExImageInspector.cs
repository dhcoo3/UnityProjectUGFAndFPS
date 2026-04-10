
#if UNITY_EDITOR
using HotAssets.Scripts.Common;
using HotAssets.Scripts.UI.Tool.Component;
using UnityEditor;
using UnityEngine;

namespace Editor.UI.ExUI
{
    [CustomEditor(typeof(HotAssets.Scripts.UI.Tool.Component.ExImage))]
    public class ExImageInspector : UnityEditor.Editor
    {
        private bool m_ImageInspector = false;
        private bool m_Extend = true;
        private bool m_ClearLoadPath = false;

        private HotAssets.Scripts.UI.Tool.Component.ExImage _target;     

        public SerializedProperty DynamicLoad;
        public SerializedProperty DynamicLoadPath;
        public SerializedProperty SlicedClipMode;
        public SerializedProperty DynamicLoadAlphaVal;
    
        private Sprite _spriteObj;
        private ExUICreateConfig _createConfig;
        
        private void OnEnable()
        {
            _target = (HotAssets.Scripts.UI.Tool.Component.ExImage)serializedObject.targetObject;
            DynamicLoad = serializedObject.FindProperty("DynamicLoad");
            DynamicLoadPath = serializedObject.FindProperty("DynamicLoadPath");
            SlicedClipMode = serializedObject.FindProperty("m_SlicedClipMode");
            DynamicLoadAlphaVal = serializedObject.FindProperty("DynamicLoadAlphaVal");
            _createConfig = ExUIConfig.LoadAsset<ExUICreateConfig>("ExUICreateConfig");
        }
    

        public void OnDestroy()
        {
            if (!Application.isPlaying&&_target &&_target.DynamicLoad)
            {
                _target.color = new Color(_target.color.r, _target.color.g, _target.color.b, 0);
                EditorUtility.SetDirty(_target);
                AssetDatabase.SaveAssets();
            }
        }

        public override void OnInspectorGUI()
        {                 
            if (_createConfig && _createConfig.ScriptIcon != null)
            {
                EditorGUIUtility.SetIconForObject(target, _createConfig.ScriptIcon);
            }
            
            serializedObject.Update();
            DrawImageInspector();
            DrawExtend();
            //修改序列化对象的属性
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawImageInspector()
        {
            m_ImageInspector = EditorGUILayout.Foldout(m_ImageInspector, "图片");
        
            if (m_ImageInspector)
            {
                base.OnInspectorGUI();
            
                if (GUILayout.Button("Set Native Size", GUILayout.Width(200)))
                {
                    SetNativeSiz();
                }
            }
        }
        string spritePath = "";
        private void DrawExtend()
        {
            m_Extend = EditorGUILayout.Foldout(m_Extend, "扩展");

            if (m_Extend)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent("路径复制","复制图片的资源全路径"))){ CopyPath(); }
                EditorGUILayout.EndHorizontal();
            
                EditorGUILayout.BeginHorizontal();
                _spriteObj = EditorGUILayout.ObjectField(new GUIContent("Sprite图片"),_spriteObj,typeof(Sprite),true) as Sprite;
            
                EditorGUILayout.BeginVertical();
                if (GUILayout.Button("Set Native Size")) { SetNativeSiz(); }

                if (GUILayout.Button("删除图片"))
                {
                    _spriteObj = null;
                    _target.sprite = null;
                    EditorUtility.SetDirty(_target);
                }
            
                EditorGUILayout.EndVertical();
        
                EditorGUILayout.EndHorizontal();
            
                EditorGUILayout.PropertyField(DynamicLoad, new GUIContent("动态加载","游戏运行时才加载图片,编辑器模式下只显示预览,预览结束后会将透明度调为0"));
            
                if (_target.DynamicLoad)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(DynamicLoadPath, new GUIContent("加载路径","运行时按此路径加载,若要使用代码加载,请点击清除路径"));
                
                    if (GUILayout.Button("清除路径",GUILayout.Width(100)))
                    {
                        m_ClearLoadPath = true;
                        _target.DynamicLoadPath = "";
                        EditorUtility.SetDirty(_target);
                    }

                    if (GUILayout.Button("定位"))
                    {
                        if (!string.IsNullOrEmpty(_target.DynamicLoadPath))
                        {
                            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(_target.DynamicLoadPath);
                        }
                    }
                
                    EditorGUILayout.EndHorizontal();
                
                    EditorGUILayout.PropertyField(DynamicLoadAlphaVal, new GUIContent("透明度","加载成功后设置的透明度"));
                
                    if (_spriteObj)
                    {
                        spritePath = AssetDatabase.GetAssetPath(_spriteObj);
                    }
                    else if (!string.IsNullOrEmpty(_target.DynamicLoadPath))
                    {
                        spritePath = _target.DynamicLoadPath;
                    } else if (_target.sprite)
                    {
                        spritePath = AssetDatabase.GetAssetPath(_target.sprite);
                    }
                
                    if (!string.IsNullOrEmpty(spritePath))
                    {
                        if (!m_ClearLoadPath && !Application.isPlaying &&!_target.DynamicLoadPath.Equals(spritePath))
                        {
                            _target.DynamicLoadPath = spritePath;
                            EditorUtility.SetDirty(_target);
                        }
                    }
                
                    if (!Application.isPlaying && !string.IsNullOrEmpty(spritePath))
                    {
                        Sprite tmpSprite = GetTmpSprite(spritePath);
                        _target.sprite = tmpSprite;
                        _spriteObj = tmpSprite;
                    }

                    if (!Application.isPlaying)
                    {
                        _target.color = new Color(_target.color.r, _target.color.g, _target.color.b, _target.DynamicLoadAlphaVal);
                    }
                }
                else
                {
                    if (_target.sprite)
                    {
                        spritePath = AssetDatabase.GetAssetPath(_target.sprite);

                        if (_spriteObj != null)
                        {
                            string newPath = AssetDatabase.GetAssetPath(_spriteObj);
                            if (newPath != spritePath)
                            {
                                _target.sprite = null;
                            }
                            else
                            {
                                _spriteObj = _target.sprite;
                            }
                        }
                        else
                        {
                            _spriteObj = _target.sprite;
                        }
                    
                        if (string.IsNullOrEmpty(spritePath))
                        {
                            _target.sprite = null;
                            _spriteObj = null;
                        }
                    }
                    else
                    {
                        spritePath = AssetDatabase.GetAssetPath(_spriteObj);
                        _target.sprite = _spriteObj;
                    }
                }
            
                EditorGUILayout.LabelField(new GUIContent("图片名称:"+spritePath));
                EditorGUILayout.PropertyField(SlicedClipMode, new GUIContent("Sliced+Filled","九宫格带裁剪模式"));
            }
        }
    
        private void CopyPath()
        {
            if (_target.sprite != null)
            {
                Utils.CopyToClipboard(AssetDatabase.GetAssetPath(_target.sprite));
            }
        }
    
        private void SetNativeSiz()
        {
            _target.SetNativeSize();

            ExButton uGF_Button = _target.GetComponent<ExButton>();
           
            if (uGF_Button && !uGF_Button.CustomPolygon)
            {              
                if (uGF_Button.PolygonCollider2D)
                {
                    float width = uGF_Button.GetComponent<RectTransform>().rect.width;
                    float height = uGF_Button.GetComponent<RectTransform>().rect.height;
                    Vector2 vector1 = new Vector2((width / 2) * -1, height / 2);
                    Vector2 vector2 = new Vector2(width / 2, height / 2);
                    Vector2 vector3 = new Vector2(width / 2, (height / 2) * -1);
                    Vector2 vector4 = new Vector2((width / 2) * -1, (height / 2) * -1);
                    uGF_Button.PolygonCollider2D.points = new Vector2[] { vector1, vector2, vector3, vector4 };
                }                
            }
            EditorUtility.SetDirty(_target);
        }

        [MenuItem("GameObject/UGF_UI/ExImage", priority = 2)]
        static void AddUGFImage()
        {
            CreateUGFImage(Selection.activeTransform);
        }

        public static HotAssets.Scripts.UI.Tool.Component.ExImage CreateUGFImage(Transform parent)
        {
            GameObject obj = new GameObject("UGFImage");

            if (parent)
            {
                obj.transform.SetParent(parent, false);
            }
            obj.transform.localPosition = Vector3.zero;
            HotAssets.Scripts.UI.Tool.Component.ExImage exImage = obj.AddComponent<HotAssets.Scripts.UI.Tool.Component.ExImage>();
            exImage.raycastTarget = false;
            exImage.maskable = false;
            Selection.activeGameObject = obj;
            return exImage;
        }

        private Sprite GetTmpSprite(string spritePath)
        {
            Sprite rtSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (rtSprite == null) return null;
            return Sprite.Create(rtSprite.texture, rtSprite.rect, rtSprite.pivot,
                rtSprite.pixelsPerUnit, 0U, SpriteMeshType.Tight, rtSprite.border);
        }
    }
}
#endif