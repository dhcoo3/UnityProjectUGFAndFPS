using System.Collections.Generic;
using Builtin.Scripts.Extension;
using Builtin.Scripts.Game;
using Cysharp.Text;
using HotAssets.Scripts.Common.Event;
using HotAssets.Scripts.Extension;
using HotAssets.Scripts.GamePlay.Logic.Common;
using HotAssets.Scripts.GamePlay.Logic.Unit.Bullet;
using HotAssets.Scripts.GamePlay.Render.RenderManager;
using HotAssets.Scripts.GamePlay.Render.Role;
using TuanjieAI.Assistant.Schema;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.GamePlay.Render.Bullet
{
    public class BulletRender:GameRender
    {
        private readonly Dictionary<int, BulletEntity> _bulletEntities = new Dictionary<int, BulletEntity>();
        
        public override void Initialize()
        {
            Subscribe(GamePlayEvent.ERenderBullet,RenderBullet);
            Subscribe(GamePlayEvent.EStopRenderBullet,StopRenderBullet);
        }
        
        public override void LogicUpdate(fix deltaTime)
        {
            if(_bulletEntities.Count == 0) return;

            foreach (var (key,data) in _bulletEntities)
            {
                data.LogicUpdate(deltaTime);
            }
            
            base.LogicUpdate(deltaTime);
        }
        
        /// <summary>
        /// 渲染子弹
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RenderBullet(object sender, GameEvent e)
        {
            BulletUnit bulletUnit = e.GetParam1<BulletUnit>();

            EntityParams param = EntityParams.Create(bulletUnit.Behaviour.Position);
            param.OnShowCallback += ShowBulletFinish;
            param.Unit = bulletUnit;
            AppEntry.Entity.ShowEntity<BulletEntity>( AssetPathUtil.GetBullet(bulletUnit.Data.model.prefab),
                GamePlayDefine.EntityGroup.BulletEntity,
                GamePlayDefine.LoadPriority.Bullet, param);
        }
        
        private void ShowBulletFinish(EntityLogic logic)
        {
            BulletEntity roleEntity = logic as BulletEntity;
            if (roleEntity == null)
            {
                Log.Error("实体加载失败");
                return;
            }

            _bulletEntities.Add(roleEntity.BulletId, roleEntity);
        }

        private void StopRenderBullet(object sender, GameEvent e)
        {
            int id = e.GetParam1<int>();

            if (_bulletEntities.TryGetValue(id, out BulletEntity bulletEntity))
            {
                AppEntry.Entity.HideEntity(bulletEntity.Entity);
                _bulletEntities.Remove(id);
            }
        }
    }
}