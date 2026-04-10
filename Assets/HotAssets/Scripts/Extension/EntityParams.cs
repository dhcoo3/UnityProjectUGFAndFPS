using GameFramework;
using HotAssets.Scripts.Common;
using HotAssets.Scripts.GamePlay.Logic.Unit.Core;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.Extension
{
    public class EntityParams : RefParams
    {
        public IReference Unit { get; set; } = null;
        public fix3? Position { get; set; } = null;
        public fix3? LocalPosition { get; set; } = null;
        public fix3? LocalEulerAngles { get; set; } = null;
        public fix3? EulerAngles { get; set; } = null;
        public fix3? LocalScale { get; set; } = null;
        public int GameObjectLayer { get; set; } = -1;

        /// <summary>
        /// 绑定到父实体
        /// </summary>
        public Entity AttchToEntity { get; set; } = null;
        /// <summary>
        /// 指定绑定到父实体下的哪个节点
        /// </summary>
        public Transform ParentTransform { get; set; } = null;

        /// <summary>
        /// 实体显示时回调
        /// </summary>
        public GameFrameworkAction<EntityLogic> OnShowCallback { get; set; } = null;

        /// <summary>
        /// 实体隐藏时回调
        /// </summary>
        public GameFrameworkAction<EntityLogic> OnHideCallback { get; set; } = null;

        /// <summary>
        /// 创建一个实例(必须使用该接口创建)
        /// </summary>
        /// <param name="position"></param>
        /// <param name="eulerAngles"></param>
        /// <param name="localScale"></param>
        /// <returns></returns>
        public static EntityParams Create(fix3? position = null, fix3? eulerAngles = null, fix3? localScale = null)
        {
            var eParams = ReferencePool.Acquire<EntityParams>();
            eParams.CreateRoot();
            eParams.Position = position;
            eParams.EulerAngles = eulerAngles;
            eParams.LocalScale = localScale;
            return eParams;
        }
        
        protected override void ResetProperties()
        {
            base.ResetProperties();
            this.Position = null;
            this.LocalPosition = null;
            this.EulerAngles = null;
            this.LocalEulerAngles = null;
            this.LocalScale = null;
            this.GameObjectLayer = -1;
            this.AttchToEntity = null;
            this.ParentTransform = null;
            OnShowCallback = null;
            OnHideCallback = null;
            this.Unit = null;
        }
    }
}
#pragma warning restore IDE1006 // 命名样式
