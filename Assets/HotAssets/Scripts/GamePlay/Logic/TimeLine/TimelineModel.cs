namespace HotAssets.Scripts.GamePlay.Logic.TimeLine
{
    public class TimelineModel
    {
        public readonly int Id;

        ///<summary>
        ///Timeline运行多久之后发生，单位：秒
        ///</summary>
        public readonly TimelineNode[] Nodes;

        ///<summary>
        ///Timeline一共多长时间（到时间了就丢掉了），单位秒
        ///</summary>
        public readonly fix Duration;

        ///<summary>
        ///如果有caster，并且caster处于蓄力状态，则可能会经历跳转点
        ///</summary>
        public readonly TimeLineGoTo ChargeGoBack;

        public TimelineModel(int id, TimelineNode[] nodes, fix duration, TimeLineGoTo chargeGoBack){
            this.Id = id;
            this.Nodes = nodes;
            this.Duration = duration;
            this.ChargeGoBack = chargeGoBack;
        }
    }
}
