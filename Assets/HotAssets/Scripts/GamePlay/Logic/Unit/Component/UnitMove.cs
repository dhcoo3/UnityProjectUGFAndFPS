using AAAGame.ScriptsHotfix.GamePlay.Logic.Map;
using GameFramework;
using HotAssets.Scripts.GamePlay.Logic.Map;
using HotAssets.Scripts.GamePlay.Logic.ProxyManager;
using HotAssets.Scripts.GamePlay.Logic.Unit.Core;
using HotAssets.Scripts.GamePlay.Logic.Unit.Role;

namespace HotAssets.Scripts.GamePlay.Logic.Unit.Component
{
    ///<summary>
    ///单位移动控件，所有需要移动的单位都应该添加这个来控制它的移动，不论是角色，aoe还是子弹，但是地形不能用这个，因为地形是terrain不是unit
    ///这里负责的是每一帧往一个方向移动，至于往什么方向移动，这应该是其他控件的事情，比如角色是由操作来决定的，子弹则是轨迹决定的
    ///在这游戏里，角色只有x,z方向移动，依赖于地形的y移动如果有，也不归这个逻辑来管理，而是由视觉元素（比如小土坡）自身决定的
    ///</summary>
    public class UnitMove:IReference
    {
        //是否有权移动
        private bool _canMove = true;
        public bool HitObstacle => _hitObstacle;

        private bool _hitObstacle = false;

        public bool IsGrounded { get; private set; } = false;

        /// <summary>
        /// 本帧 X 轴是否被墙体阻挡（向左或向右移动但被修正），用于手雷水平弹跳检测
        /// </summary>
        public bool IsHitWall { get; private set; } = false;

        /// <summary>
        /// 撞墙方向：true = 右侧有墙，false = 左侧有墙（仅 IsHitWall 为 true 时有效）
        /// </summary>
        public bool IsHitWallOnRight { get; private set; } = false;
        
        //要移动的方向的力，单位：米/秒。
        private fix3 _velocity = fix3.zero;
        
        private MapProxy _mapProxy;
        
        private IUnit _unit;

        private fix3 _tmpVector3 = fix3.zero;
        public static UnitMove Create(IUnit unit)
        {
            UnitMove unitMove = ReferencePool.Acquire<UnitMove>();
            unitMove._mapProxy = GameProxyManger.Instance.GetProxy<MapProxy>();
            unitMove._unit = unit;
            return unitMove;
        }

        public void LogicUpdate(fix fixedDeltaTime) {
            if (_canMove == false || _velocity == fix3.zero) return;

            _tmpVector3.x =  _velocity.x * fixedDeltaTime + _unit.Behaviour.Position.x;
            _tmpVector3.y =  _velocity.y * fixedDeltaTime + _unit.Behaviour.Position.y;
            _tmpVector3.z =  _velocity.z * fixedDeltaTime + _unit.Behaviour.Position.z;

            MapTargetPosInfo mapTargetPosInfo = _mapProxy.MapInfo.FixTargetPosition(
                _unit.Behaviour.Position,
                _unit.BodyRadius,
                _tmpVector3,
                _unit.MoveType, (_unit.IgnoreBorder&& _unit.MoveType == cfg.Game.MoveType.Fly)
            );

            // 检测Y轴是否被地面阻挡（用于落地判断）
            bool wasMovingDown = _velocity.y <= fix.Zero;
            bool yBlocked = fixMath.roundToInt(_tmpVector3.y * 1000) != fixMath.roundToInt(mapTargetPosInfo.suggestPos.y * 1000);
            IsGrounded = wasMovingDown && yBlocked;

            // 检测X轴是否被墙体阻挡（用于手雷弹跳）
            bool wasMovingHorizontal = _velocity.x != fix.Zero;
            bool xBlocked = fixMath.roundToInt(_tmpVector3.x * 1000) != fixMath.roundToInt(mapTargetPosInfo.suggestPos.x * 1000);
            IsHitWall = wasMovingHorizontal && xBlocked;
            if (IsHitWall)
                IsHitWallOnRight = _velocity.x > fix.Zero;

            // 同步落地状态到 RoleState：使用 OR 合并，不覆盖平台系统（MapProxy）已写入的 true
            if (_unit is RoleUnit roleUnit)
                roleUnit.RoleState.IsGrounded = roleUnit.RoleState.IsGrounded || IsGrounded;

            if (_unit.SmoothMove == false && mapTargetPosInfo.obstacle){
                _hitObstacle = true;
                _canMove = false;
            }

            _unit.Behaviour.Position = mapTargetPosInfo.suggestPos;
            _velocity = fix3.zero;
            _tmpVector3 = fix3.zero;
        }

        private void StopMoving(){
            _velocity = fix3.zero;
        }

        ///<summary>
        ///当前的移动方向
        ///</summary>
        public fix3 GetMoveDirection(){
            return _velocity;
        }

        ///<summary>
        ///移动向某个方向，距离也决定了速度，距离单位是米，1秒内移动的量
        ///<param name="moveForce">移动方向和力，单位：米/秒</param>
        ///</summary>
        public void MoveBy(fix3 moveForce){
            if (_canMove == false) return;
            this._velocity = moveForce;
        }
        
        /// <summary>
        /// 应用旋转到坐标
        /// </summary>
        public void ApplyPosition(fix3 position)
        {
            _unit.Behaviour.Position = position;
        }

        ///<summary>
        ///禁止角色可以移动能力，会停止当前的移动
        ///</summary>
        public void DisableMove(){
            StopMoving();
            _canMove = false;
        }

        ///<summary>
        ///开启角色可以移动的能力
        ///</summary>
        public void EnableRotate(){
            _canMove = true;
        }

        public void Clear()
        {
            _unit = null;
            IsGrounded = false;
            IsHitWall = false;
            IsHitWallOnRight = false;
            _canMove = true;        // 重置，避免对象池复用时子弹/单位永远无法移动
            _hitObstacle = false;
            _velocity = fix3.zero;
        }
    }   
}



