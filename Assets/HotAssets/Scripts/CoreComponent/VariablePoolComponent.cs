using System.Collections.Generic;
using System.Linq;
using GameFramework;
using UnityEngine.Pool;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.CoreComponent
{
    /// <summary>
    /// 用于通过引用池管理变量, 避免频繁new对象, 用于Entity和UI参数传递
    /// </summary>
    public class VariablePoolComponent : GameFrameworkComponent
    {
        private Dictionary<int, Dictionary<string, Variable>> m_Variables;
        private DictionaryPool<string, Variable> m_ValuesPool;
#if UNITY_EDITOR
        public Dictionary<int, Dictionary<string, Variable>> Variables => m_Variables;
#endif
        protected override void Awake()
        {
            base.Awake();
            m_Variables = new Dictionary<int, Dictionary<string, Variable>>(1024);
        }


        private void OnDestroy()
        {
            var keys = m_Variables.Keys.ToArray();
            foreach (var key in keys)
            {
                ClearVariables(key);
            }

            m_Variables.Clear();
        }

        public bool TryGetVariable<T>(int rootId, string key, out T value) where T : Variable
        {
            value = null;
            if (m_Variables.TryGetValue(rootId, out var values) && values.TryGetValue(key, out Variable v))
            {
                value = v as T;
                return true;
            }

            return false;
        }

        public T GetVariable<T>(int rootId, string key) where T : Variable
        {
            if (m_Variables.TryGetValue(rootId, out var values) && values.TryGetValue(key, out var value))
            {
                return value as T;
            }

            return null;
        }

        public void SetVariable<T>(int rootId, string key, T value) where T : Variable
        {
            if (m_Variables.TryGetValue(rootId, out var values))
            {
                values[key] = value;
            }
            else
            {
                values = DictionaryPool<string, Variable>.Get();
                values[key] = value;
                m_Variables.Add(rootId, values);
            }
        }

        public bool HasVariable(int rootId, string key)
        {
            return m_Variables.TryGetValue(rootId, out var values) && values.ContainsKey(key);
        }

        public void ClearVariables(int rootId)
        {
            if (m_Variables.TryGetValue(rootId, out var values))
            {
                foreach (var item in values)
                {
                    ReferencePool.Release(item.Value);
                }

                DictionaryPool<string, Variable>.Release(values);
                m_Variables.Remove(rootId);
            }
        }
    }
}