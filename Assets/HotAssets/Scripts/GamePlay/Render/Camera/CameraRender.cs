using HotAssets.Scripts.Common.Event;
using HotAssets.Scripts.GamePlay.Logic.Common;
using HotAssets.Scripts.GamePlay.Logic.Map;
using HotAssets.Scripts.GamePlay.Logic.ProxyManager;
using HotAssets.Scripts.GamePlay.Logic.Unit.Role;
using HotAssets.Scripts.GamePlay.Render.RenderManager;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.GamePlay.Render.Camera
{
    public class CameraRender:GameRender
    {
        private UnityEngine.Camera _fightCamera;

        public UnityEngine.Camera FightCamera
        {
            get
            {
                return _fightCamera;
            }
        }

        private RoleUnit _focus;

        private Vector3 _targetPosition = new Vector3(0,0,-50);

        private MapProxy _mapProxy;

        public override void Initialize()
        {
            _fightCamera = UnityEngine.Camera.main;
            _mapProxy = GameProxyManger.Instance.GetProxy<MapProxy>();
            Subscribe(GamePlayEvent.ECameraFocus,CameraFocus);
        }

        private void CameraFocus(object sender, GameEvent e)
        {
            RoleUnit unit = e.GetParam1<RoleUnit>();
            if (unit == null)
            {
                Log.Info("Camera Focus Error");
                return;
            }
            _focus = unit;
        }

        /// <summary>
        /// 每帧跟随目标并将相机限制在地图边界内。
        /// 根据相机正交半高/半宽计算可视范围，确保视口不超出 MapInfo.border。
        /// 若地图尺寸小于相机视口，则居中显示。
        /// </summary>
        public override void LogicUpdate(fix deltaTime)
        {
            if (_focus == null || _focus.IsDeath())
            {
                return;
            }

            _targetPosition.x = _focus.Behaviour.Position.x;
            _targetPosition.y = _focus.Behaviour.Position.y;

            // 地图边界限制
            MapInfo mapInfo = _mapProxy?.MapInfo;
            if (mapInfo != null)
            {
                Rect border = mapInfo.border;
                float halfH = _fightCamera.orthographicSize;
                float halfW = halfH * _fightCamera.aspect;

                // 地图比视口小时居中，否则限制范围
                float minX = border.xMin + halfW;
                float maxX = border.xMax - halfW;
                float minY = border.yMin + halfH;
                float maxY = border.yMax - halfH;

                _targetPosition.x = minX < maxX
                    ? Mathf.Clamp(_targetPosition.x, minX, maxX)
                    : (border.xMin + border.xMax) * 0.5f;

                _targetPosition.y = minY < maxY
                    ? Mathf.Clamp(_targetPosition.y, minY, maxY)
                    : (border.yMin + border.yMax) * 0.5f;
            }

            FightCamera.transform.position = _targetPosition;

            base.LogicUpdate(deltaTime);
        }
    }
}
