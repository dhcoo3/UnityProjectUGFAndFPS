using HotAssets.Scripts.CoreComponent;
using UnityEditor;
using UnityEngine;

namespace Editor.ComponentInspector
{
    [CustomEditor(typeof(VariablePoolComponent))]
    public class VariablePoolComponentInspector : UnityEditor.Editor
    {
        VariablePoolComponent m_Target;
        int m_UnfoldId;
        int m_TotalVariableCount;
        bool m_Debug;

        private void OnEnable()
        {
            m_Target = target as VariablePoolComponent;
            m_UnfoldId = -1;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (null != m_Target.Variables)
            {
                m_Debug = EditorGUILayout.Toggle("Enable Debug", m_Debug);
                EditorGUILayout.LabelField($"Variables Count:{m_TotalVariableCount}");
                m_TotalVariableCount = 0;
                foreach (var item in m_Target.Variables)
                {
                    m_TotalVariableCount += item.Value.Count;
                    if (m_Debug)
                    {
                        bool unfold = item.Key == m_UnfoldId;
                        if (GUILayout.Button(unfold ? $"▼ ID:{item.Key}" : $"▶ ID:{item.Key}", EditorStyles.label))
                        {
                            m_UnfoldId = unfold ? -1 : item.Key;
                            ;
                        }

                        if (unfold)
                        {
                            EditorGUILayout.BeginVertical("box");
                            {
                                foreach (var element in item.Value)
                                {
                                    EditorGUILayout.LabelField($"{element.Key} : {element.Value}");
                                }

                                EditorGUILayout.EndVertical();
                            }
                        }
                    }

                    Repaint();
                }
            }
        }
    }
}