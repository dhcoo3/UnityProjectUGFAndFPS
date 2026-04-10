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

        /// <summary>上一帧中心坐标，用于计算本帧位移差</summary>
        public fix3 PrevPosition;

        /// <summary>来回移动的起点</summary>
        public fix3 StartPos;

        /// <summary>来回移动的终点</summary>
        public fix3 EndPos;

        /// <summary>移动速度（m/s）</summary>
        public fix Speed;

        /// <summary>平台碰撞半宽（用于站立检测，X 轴方向）</summary>
        public fix HalfWidth;

        /// <summary>平台碰撞半高（Y 轴方向；上表面 = Position.y + HalfHeight）</summary>
        public fix HalfHeight;

        /// <summary>预制体资源路径</summary>
        public string AssetPath;

        // 方向标志：1 = 向 EndPos 移动，-1 = 向 StartPos 移动
        private int _dir = 1;

        /// <summary>平台上表面的 Y 坐标</summary>
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

        /// <summary>
        /// 本帧平台位移差（当前位置 - 上帧位置）。
        /// </summary>
        public fix3 GetDeltaThisFrame() => Position - PrevPosition;

        /// <summary>
        /// 检测角色是否站在平台上。
        /// 竖向向下容差 0.5m（允许重力已将角色略微拉低），向上 0.3m（允许刚踩上来的情况）。
        /// </summary>
        /// <param name="rolePos">角色当前脚部坐标（逻辑层 Position）</param>
        /// <param name="bodyRadius">角色碰撞体半径</param>
        public bool IsStandingOn(fix3 rolePos, fix bodyRadius)
        {
            fix top = SurfaceY;
            // 角色 Position 是圆心，脚底 = rolePos.y - bodyRadius，需要与平台上表面对齐
            fix feetY = rolePos.y - bodyRadius;
            bool verticalMatch = feetY >= top - (fix)0.5f && feetY <= top + (fix)0.3f;
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