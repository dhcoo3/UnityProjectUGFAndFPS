using System.Collections.Generic;
using HotAssets.Scripts.GamePlay.Logic.ProxyManager;
using HotAssets.Scripts.GamePlay.Logic.Unit.Core;
using HotAssets.Scripts.GamePlay.Logic.Unit.Role;

namespace HotAssets.Scripts.GamePlay.Logic.TimeLine
{
    public class TimelineProxy:GameProxy
    {
        private List<TimelineObj> timelines = new List<TimelineObj>();
        
        public override void Initialize()
        {
            
        }

        public override void LogicUpdate(fix deltaTime)
        {
            if (this.timelines.Count <= 0) return;

            int idx = 0;
            while (idx < this.timelines.Count)
            {
                TimelineObj timelineObj = timelines[idx];
                fix wasTimeElapsed =timelineObj.timeElapsed;
                timelineObj.timeElapsed += deltaTime * timelineObj.timeScale;

                //判断有没有返回点
                if (
                    timelineObj.model.ChargeGoBack.atDuration < timelineObj.timeElapsed &&
                    timelineObj.model.ChargeGoBack.atDuration >= wasTimeElapsed
                ){
                    if (timelines[idx].caster is RoleUnit roleUnit){
                        if (roleUnit.RoleState.charging == true){
                            timelines[idx].timeElapsed = timelines[idx].model.ChargeGoBack.gotoDuration;
                            continue;
                        }
                    }
                }

                //执行时间点内的事情
                for (int i = 0; i < timelineObj.model.Nodes.Length; i++)
                {
                    fix nodeTimeElapsed = timelineObj.model.Nodes[i].TimeElapsed;

                    bool time1 = nodeTimeElapsed < timelineObj.timeElapsed;
                    bool time2 = nodeTimeElapsed >= wasTimeElapsed;

                    if (time1 && time2)
                    {
                        timelineObj.model.Nodes[i].DoEvent(
                            timelineObj,
                            timelineObj.model.Nodes[i]
                        );
                    }
                }

                //判断timeline是否终结
                if (timelineObj.model.Duration <= timelineObj.timeElapsed){
                    timelines.RemoveAt(idx);
                }else{
                    idx++;
                }
            }
        }
    
        ///<summary>
        ///添加一个timeline
        ///<param name="timelineModel">要添加的timeline</param>
        ///</summary>
        public void AddTimeline(TimelineObj timeline){
            this.timelines.Add(timeline);
        }

        public bool CasterHasTimeline(IUnit caster){
            for (var i = 0; i < timelines.Count; i++)
            {
                if (timelines[i].caster == caster)
                {
                    return true;
                }
            }
            return false;
        }
    }
}