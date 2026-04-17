using GameFramework;

namespace HotAssets.Scripts.GamePlay.Logic.Map
{
    /// <summary>
    /// 移动平台逻辑单元（属于地图系统）。
    /// 在 StartPos 和 EndPos 之间匀速往返移动，每帧记录位移差供 MapProxy
    /// 用于将站立在平台上的角色一起携带移动。
    /// </summary>
    public class MapPlatformUnit : IReference
    {
        /// <summary>配置表 ID</summary>
        public int PlatformId;

        /// <summary>当前帧中心坐标</summary>
        public fix3 Position;

        /// <summary>上一帧中心坐标，用于计算本帧位移</summary>
        public fix3 PrevPosition;

        /// <summary>来回移动的起点</summary>
        public fix3 StartPos;

        /// <summary>来回移动的终点</summary>
        public fix3 EndPos;

        /// <summary>移动速度，单位：m/s</summary>
        public fix Speed;

        /// <summary>平台碰撞半宽，用于站立检测，X 轴方向</summary>
        public fix HalfWidth;

        /// <summary>平台碰撞半高，表面 Y = Position.y + HalfHeight</summary>
        public fix HalfHeight;

        /// <summary>预制体资源路径</summary>
        public string AssetPath;

        /// <summary>方向标记：1 = 朝 EndPos，-1 = 朝 StartPos</summary>
        private int _dir = 1;

        /// <summary>平台表面 Y 坐标</summary>
        public fix SurfaceY => Position.y + HalfHeight;

        /// <summary>
        /// 每帧更新平台位置，到达端点时反向。
        /// </summary>
        public void LogicUpdate(fix deltaTime)
        {
            PrevPosition = Position;

            fix3 target = _dir == 1 ? EndPos : StartPos;
            fix3 diff = target - Position;
            fix distSq = diff.x * diff.x + diff.y * diff.y;

            if (distSq == fix.Zero)
            {
                _dir = -_dir;
                return;
            }

            fix dist = fixMath.sqrt(distSq);
            fix moveStep = Speed * deltaTime;

            if (dist <= moveStep)
            {
                Position = target;
                _dir = -_dir;
            }
            else
            {
                Position += (diff / dist) * moveStep;
            }
        }

        /// <summary>本帧平台位移（当前 - 上一帧）。</summary>
        public fix3 GetDeltaThisFrame() => Position - PrevPosition;

        /// <summary>
        /// 判断角色是否站在平台上。
        /// 这里使用较小的垂直容差，避免离开边缘后仍长时间被判定为“站立”。
        /// </summary>
        public bool IsStandingOn(fix3 rolePos, fix bodyRadius)
        {
            fix top = SurfaceY;
            fix feetY = rolePos.y - bodyRadius;
            bool verticalMatch = feetY >= top - (fix)0.08f && feetY <= top + (fix)0.08f;
            bool horizontalMatch = rolePos.x >= Position.x - HalfWidth - bodyRadius
                                && rolePos.x <= Position.x + HalfWidth + bodyRadius;
            return verticalMatch && horizontalMatch;
        }

        public void Clear()
        {
            PlatformId = 0;
            Position = fix3.zero;
            PrevPosition = fix3.zero;
            StartPos = fix3.zero;
            EndPos = fix3.zero;
            Speed = fix.Zero;
            HalfWidth = fix.Zero;
            HalfHeight = fix.Zero;
            AssetPath = null;
            _dir = 1;
        }
    }
}
