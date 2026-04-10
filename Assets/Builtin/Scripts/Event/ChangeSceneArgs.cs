//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework;
using GameFramework.Event;

namespace Builtin.Scripts.Event
{
    /// <summary>
    /// 加载字典成功事件。
    /// </summary>
    public sealed class ChangeSceneArgs : GameEventArgs
    {
        /// <summary>
        /// 加载字典成功事件编号。
        /// </summary>
        public static readonly int EventId = typeof(ChangeSceneArgs).GetHashCode();

        /// <summary>
        /// 初始化加载字典成功事件的新实例。
        /// </summary>
        public ChangeSceneArgs()
        {
            SceneName = null;
        }

        /// <summary>
        /// 获取加载字典成功事件编号。
        /// </summary>
        public override int Id
        {
            get
            {
                return EventId;
            }
        }

        /// <summary>
        /// 获取字典资源名称。
        /// </summary>
        public string SceneName
        {
            get;
            private set;
        }

        /// <summary>
        /// 创建加载字典成功事件。
        /// </summary>
        /// <param name="e">内部事件。</param>
        /// <returns>创建的加载字典成功事件。</returns>
        public static ChangeSceneArgs Create(string sceneName)
        {
            ChangeSceneArgs loadDictionarySuccessEventArgs = ReferencePool.Acquire<ChangeSceneArgs>();
            loadDictionarySuccessEventArgs.SceneName = sceneName;
            return loadDictionarySuccessEventArgs;
        }

        /// <summary>
        /// 清理加载字典成功事件。
        /// </summary>
        public override void Clear()
        {
            SceneName = null;
        }
    }
}
