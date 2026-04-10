using System.Text.RegularExpressions;
using Cysharp.Text;
using UnityEngine;

namespace Editor.UI
{
    public class UIContainerAutoCreate
    {
        private const string TEMPLATE = @"
using HotAssets.Scripts.UI.Component;
public class <Name> : ExPanel
{
    #region Auto Create
    #endregion

    protected override void OnInit(object userData)
    {
        #region Auto Bind
        #endregion

        #region Auto Event
        #endregion

        base.OnInit(userData);
    }

    #region Auto Write Event
    #endregion
}";
    
    
        public static void AutoCreate(GameObject obj)
        {
        
        }
    
        /// <summary>
        /// 基于模板生成完整的 Panel 脚本内容（脚本不含 region 时使用）
        /// </summary>
        public static string GetTemplateText(string className, UIContainer uiContainer, string mgrName)
        {
            string defineText = GetDefineText(uiContainer, true);
            string bindText = GetBindText(uiContainer, mgrName, true);
            string eventText = GetEventText(uiContainer, true);
            string writeEventText = GetWriteEventText(uiContainer, true);

            string result = TEMPLATE.Replace("<Name>", className);
            result = Regex.Replace(result, @"#region\s+Auto\s+Create(.+?)#endregion", defineText, RegexOptions.Singleline);
            result = Regex.Replace(result, @"#region\s+Auto\s+Bind(.+?)#endregion", bindText, RegexOptions.Singleline);
            result = Regex.Replace(result, @"#region\s+Auto\s+Event(.+?)#endregion", eventText, RegexOptions.Singleline);
            result = Regex.Replace(result, @"#region\s+Auto\s+Write\s+Event(.+?)#endregion", writeEventText, RegexOptions.Singleline);
            return result;
        }

        /// <summary>
        /// 生成UI组件定义代码
        /// </summary>
        public static string GetDefineText(UIContainer uiContainer, bool isReplace = false)
        {
            if (uiContainer == null || uiContainer.UIContainerDict == null)
            {
                return string.Empty;
            }

            using (var builder = ZString.CreateStringBuilder())
            {
                if (isReplace)
                {
                    builder.Append("#region Auto Create\n");
                }

                foreach (var data in uiContainer.UIContainerDict)
                {
                    builder.AppendFormat("    {0} {1} {2};\n", data.Value.Domain, data.Value.NodeType, data.Value.NodeName);
                }
                builder.Append("    #endregion");

                return builder.ToString();
            }
        }

        /// <summary>
        /// 生成UI组件绑定代码
        /// </summary>
        public static string GetBindText(UIContainer uiContainer, string mgrName, bool isReplace = false)
        {
            if (uiContainer == null || uiContainer.UIContainerDict == null)
            {
                return string.Empty;
            }

            using (var builder = ZString.CreateStringBuilder())
            {
                if (isReplace)
                {
                    builder.Append("#region Auto Bind\n");
                }

                foreach (var data in uiContainer.UIContainerDict)
                {
                    builder.AppendFormat("        {0} = {1}.Get<{2}>(\"{3}\");\n", data.Value.NodeName, mgrName, data.Value.NodeType, data.Value.NodeName);
                }
                builder.Append("        #endregion");

                return builder.ToString();
            }
        }

        /// <summary>
        /// 生成按钮事件注册代码（OnInit 内 Auto Event 区域）
        /// </summary>
        public static string GetEventText(UIContainer uiContainer, bool isReplace = false)
        {
            if (uiContainer == null || uiContainer.UIContainerDict == null)
            {
                return string.Empty;
            }

            using (var builder = ZString.CreateStringBuilder())
            {
                if (isReplace)
                {
                    builder.Append("#region Auto Event\n");
                }

                foreach (var data in uiContainer.UIContainerDict)
                {
                    if (data.Value.NodeType != "ExButton") continue;
                    string nodeName = data.Value.NodeName;
                    string listenerName = nodeName.StartsWith("m_")
                        ? nodeName.Substring(2) + "ClickListener"
                        : nodeName + "ClickListener";
                    builder.AppendFormat("        {0}.SetClickListener({1});\n", nodeName, listenerName);
                }
                builder.Append("        #endregion");

                return builder.ToString();
            }
        }

        /// <summary>
        /// 生成按钮事件方法体代码（Auto Write Event 区域）
        /// </summary>
        public static string GetWriteEventText(UIContainer uiContainer, bool isReplace = false)
        {
            if (uiContainer == null || uiContainer.UIContainerDict == null)
            {
                return string.Empty;
            }

            using (var builder = ZString.CreateStringBuilder())
            {
                if (isReplace)
                {
                    builder.Append("#region Auto Write Event\n");
                }

                foreach (var data in uiContainer.UIContainerDict)
                {
                    if (data.Value.NodeType != "ExButton") continue;
                    string nodeName = data.Value.NodeName;
                    string listenerName = nodeName.StartsWith("m_")
                        ? nodeName.Substring(2) + "ClickListener"
                        : nodeName + "ClickListener";
                    builder.AppendFormat("    private void {0}()\n    {{\n        \n    }}\n", listenerName);
                }
                builder.Append("    #endregion");

                return builder.ToString();
            }
        }
    }
}