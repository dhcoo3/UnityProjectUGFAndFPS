using System.Collections.Generic;
using Unity.Mathematics;

namespace HotAssets.Scripts.GamePlay.Logic.AI.AIStrategy
{
    public class AIMoveData
    {
        public List<int2> Path = new List<int2>();
        public int CurrentPathIndex;
    }

    public class AIWaitData
    {
        public float TimeElapsed;
    }

    /// <summary>
    /// 巡逻状态数据，每个 UnitAI 持有一份，由 AIPatrol 策略读写
    /// </summary>
    public class AIPatrolData
    {
        /// <summary>巡逻左边界（绝对坐标）</summary>
        public fix LeftBound;

        /// <summary>巡逻右边界（绝对坐标）</summary>
        public fix RightBound;

        /// <summary>当前移动方向：1=右，-1=左</summary>
        public fix Direction = fix.One;

        /// <summary>是否已完成初始化</summary>
        public bool Initialized;

        /// <summary>
        /// 重置巡逻状态
        /// </summary>
        public void Reset()
        {
            Initialized = false;
            Direction = fix.One;
            LeftBound = fix.Zero;
            RightBound = fix.Zero;
        }
    }
}