using GameFramework;
using HotAssets.Scripts.Common;
using HotAssets.Scripts.GamePlay.Render.Aoe;
using HotAssets.Scripts.GamePlay.Render.Bullet;
using HotAssets.Scripts.GamePlay.Render.Camera;
using HotAssets.Scripts.GamePlay.Render.Effect;
using HotAssets.Scripts.GamePlay.Render.Joystick;
using HotAssets.Scripts.GamePlay.Render.Layer;
using HotAssets.Scripts.GamePlay.Render.Map;
using HotAssets.Scripts.GamePlay.Render.Role;

namespace HotAssets.Scripts.GamePlay.Render.RenderManager
{
    public class GameRenderManager:Singleton<GameRenderManager>
    {
        private GameRender[] _gameRenders;
        
        /// <summary>
        /// 通用
        /// </summary>
        public void Register()
        {
            _gameRenders = new GameRender[]
            {
                ReferencePool.Acquire<MapRender>(),
                ReferencePool.Acquire<RoleRender>(),
                ReferencePool.Acquire<BulletRender>(),
                ReferencePool.Acquire<CameraRender>(),
                ReferencePool.Acquire<LayerRender>(),
                ReferencePool.Acquire<JoystickRender>(),
                ReferencePool.Acquire<EffectRender>(),
                ReferencePool.Acquire<AoeRender>()
            };
        }

        public void Initialize()
        {
            for(int i=0;i<_gameRenders.Length;i++)
            {
                _gameRenders[i].Initialize();
            }
        }

        public T GetRender<T>() where T : GameRender
        {
            foreach (var data in _gameRenders)
            {
                if (data is T)
                {
                    return (T)data;
                }
            }

            return null;
        }
        
        public void LogicUpdate(fix deltaTime)
        {
            for(int i=0;i<_gameRenders.Length;i++)
            {
                _gameRenders[i].LogicUpdate(deltaTime);
            }
        }

        public void Clear()
        {
            for(int i=0;i<_gameRenders.Length;i++)
            {
                ReferencePool.Release(_gameRenders[i]);
            }

            _gameRenders = null;
        }
    }
}