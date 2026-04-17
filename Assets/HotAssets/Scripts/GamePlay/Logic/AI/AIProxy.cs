using System;
using System.Collections.Generic;
using Builtin.Scripts.Game;
using cfg.AI;
using GameFramework;
using HotAssets.Scripts.Extension;
using HotAssets.Scripts.GamePlay.Logic.ProxyManager;
using HotAssets.Scripts.UI;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.GamePlay.Logic.AI
{
    /// <summary>
    /// 管理正在运行的AI，可能多个角色共用一个AI片段
    /// </summary>
    public class AIProxy:GameProxy
    {
        private TbAIGroupDef _tbAIGroupDef;
        private TbAIActionDef _tbAIActionDef;
        
        /// <summary>
        /// AI片段，可复用
        /// </summary>
        Dictionary<int,AIClip> _aiClips = new Dictionary<int,AIClip>();
        
        public override async void Initialize()
        {
            try
            {
                _tbAIGroupDef = await AppEntry.DataTable.GetDataTableLuBan<TbAIGroupDef>(cfg.Tables.ai_tbaigroupdef);
                _tbAIActionDef =  await AppEntry.DataTable.GetDataTableLuBan<TbAIActionDef>(cfg.Tables.ai_tbaiactiondef);
            }
            catch (Exception e)
            {
                Log.Error("Initialize error = {0}",e.Message);
            }
        }

        public override void Clear()
        {
            foreach (AIClip clip in _aiClips.Values)
            {
                ReferencePool.Release(clip);
            }
            
            _aiClips.Clear();
            base.Clear();
        }

        public List<AIClip> GetAIClips(int id)
        {
            List<AIClip> aiClips = new List<AIClip>();
            GetAIClips(id, aiClips);
            return aiClips;
        }

        /// <summary>
        /// 填充 AI 片段到已有容器，避免每次创建新 List。
        /// </summary>
        public void GetAIClips(int id, List<AIClip> target)
        {
            target.Clear();

            AIGroupDef aiGroupDef = _tbAIGroupDef.GetOrDefault(id);

            if (aiGroupDef == null)
            {
                Log.Warning("aiGroupDef is null {0}",id);
                return;
            }

            for (int i = 0; i < aiGroupDef.AiActionList.Count; i++)
            {
                int aiActionId = aiGroupDef.AiActionList[i];

                if (_aiClips.TryGetValue(aiActionId, out AIClip aiClip))
                {
                    target.Add(aiClip);
                    continue;
                }
                
                AIActionDef aiActionDef = _tbAIActionDef.GetOrDefault(aiActionId);

                if (aiActionDef == null)
                {
                    Log.Warning("aiActionDef is null {0}",aiActionId);
                    continue;
                }

                aiClip = AIClip.Create(aiActionDef);
                _aiClips.Add(aiActionId, aiClip);
                target.Add(aiClip);
            }
        }
    }
}
