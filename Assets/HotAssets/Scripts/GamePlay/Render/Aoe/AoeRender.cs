using System.Collections.Generic;
using HotAssets.Scripts.Common.Event;
using HotAssets.Scripts.GamePlay.Logic.Common;
using HotAssets.Scripts.GamePlay.Logic.Unit.Aoe;
using HotAssets.Scripts.GamePlay.Render.RenderManager;
using TuanjieAI.Assistant.Schema;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.GamePlay.Render.Aoe
{
    public class AoeRender:GameRender
    {
        private readonly Dictionary<int, AoeEntity> _aoeEntities = new Dictionary<int, AoeEntity>();
        
        public override void Initialize()
        {
            EventHelper.SubscribeCommon(GamePlayEvent.ERenderAoe,RenderAoe);
            EventHelper.SubscribeCommon(GamePlayEvent.EStopRenderAoe,StopRenderAoe);
        }
        
        /// <summary>
        /// 渲染子弹
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RenderAoe(object sender, GameEvent e)
        {
            AoeUnit aoeUnit = e.GetParam1<AoeUnit>();
            if (aoeUnit == null)
            {
                Log.Warning("aoeUnit unit is null");
                return;
            }

            /*string path = GF.AssetBridge.GetAoe(aoeUnit.Data.model.prefab);
            GF.AssetBridge.LoadEntity(path,typeof(AoeEntity),"Aoe",
                GamePlayDefine.LoadPriority.Aoe,LoadAoeFinish,aoeUnit);*/
        }
        
        private void LoadAoeFinish(AssetInfo info)
        {
            /*iAoeUnit aoeUnit = info.UserData as AoeUnit;
            f (aoeUnit == null)
            {
                Log.Error("aoeUnit is null");
                return;
            }

            UnityGameFramework.Runtime.Entity entity = info.Asset as UnityGameFramework.Runtime.Entity;
            if (entity == null || entity.Logic == null)
            {
                Log.Error("entity.Logic is null");
                return;
            }

            _aoeEntities.Add(aoeUnit.Data.AoeId, entity.Logic as AoeEntity);*/
        }

        private void StopRenderAoe(object sender, GameEvent e)
        {
            /*int id = e.GetParam1<int>();

            if (_aoeEntities.TryGetValue(id, out AoeEntity bulletEntity))
            {
                GF.AssetBridge.HideEntity(bulletEntity.Entity);
                _aoeEntities.Remove(id);
            }*/
        }
        
        public override void LogicUpdate(fix deltaTime)
        {
            if(_aoeEntities.Count == 0) return;

            foreach (var data in _aoeEntities.Values)
            {
                data.LogicUpdate(deltaTime);
            }
            
            base.LogicUpdate(deltaTime);
        }
    }
}
