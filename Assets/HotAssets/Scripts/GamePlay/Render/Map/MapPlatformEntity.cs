using HotAssets.Scripts.Extension;
using HotAssets.Scripts.GamePlay.Logic.Map;
using HotAssets.Scripts.GamePlay.Render.Entity;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.GamePlay.Render.Map
{
    /// <summary>
    /// 移动平台渲染实体（属于地图渲染系统）。
    /// 每帧将逻辑层 PlatformUnit.Position 同步到 transform.position。
    /// </summary>
    public class MapPlatformEntity : EntityRender
    {
        public int PlatformId;

        private MapPlatformUnit _mapPlatformUnit;

        private Vector3 _tmpVector3 = Vector3.zero;

        protected override void OnShow(object userData)
        {
            EntityParams entityParams = (EntityParams)userData;
            _mapPlatformUnit = entityParams.Unit as MapPlatformUnit;
           
            if (_mapPlatformUnit == null)
            {
                Log.Error("_mapPlatformUnit is invalid.");
                return;
            }
            
            PlatformId = _mapPlatformUnit.PlatformId;

            SyncPosition();

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = (int)MapRenderLayer.Grid;
            }
            
            entityParams.OnShowCallback.Invoke(this);
            base.OnShow(userData);
        }

        public override void LogicUpdate(fix deltaTime)
        {
            if (_mapPlatformUnit == null) return;
            SyncPosition();
            base.LogicUpdate(deltaTime);
        }

        protected override void OnHide(bool isShutdown, object userData)
        {
            _mapPlatformUnit = null;
            base.OnHide(isShutdown, userData);
        }

        protected override void OnRecycle()
        {
            _mapPlatformUnit = null;
            base.OnRecycle();
        }

        private void SyncPosition()
        {
            _tmpVector3.x = _mapPlatformUnit.Position.x;
            _tmpVector3.y = _mapPlatformUnit.Position.y;
            _tmpVector3.z = _mapPlatformUnit.Position.z;
            transform.position = _tmpVector3;
        }

        /// <summary>
        /// 编辑器 Gizmos 可视化：黄色矩形=碰撞范围，青色线=移动路径，白色球=起点/终点
        /// 仅运行时有效（_mapPlatformUnit 需已初始化）
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            if (_mapPlatformUnit == null) return;

            float cx = _mapPlatformUnit.Position.x;
            float cy = _mapPlatformUnit.Position.y;
            float hw = _mapPlatformUnit.HalfWidth;
            float hh = _mapPlatformUnit.HalfHeight;

            // 碰撞矩形（黄色半透明），DrawWireCube 第二参数为全尺寸，需乘2
            Gizmos.color = new Color(1f, 1f, 0f, 0.9f);
            Gizmos.DrawWireCube(new Vector3(cx, cy, 0f), new Vector3(hw * 2f, hh * 2f, 0.05f));

            // 移动路径（青色线）
            Vector3 startV = new Vector3(
                (float)_mapPlatformUnit.StartPos.x,
                (float)_mapPlatformUnit.StartPos.y, 0f);
            Vector3 endV = new Vector3(
                (float)_mapPlatformUnit.EndPos.x,
                (float)_mapPlatformUnit.EndPos.y, 0f);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(startV, endV);

            // 起点（绿色小球）终点（红色小球）
            float r = Mathf.Max(hw, hh) * 0.25f;
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(startV, r);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(endV, r);
        }
    }
}