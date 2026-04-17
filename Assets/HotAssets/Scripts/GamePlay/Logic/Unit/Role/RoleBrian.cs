using System.Collections.Generic;
using GameFramework;
using HotAssets.Scripts.GamePlay.Logic.Buff;
using HotAssets.Scripts.GamePlay.Logic.Common;
using HotAssets.Scripts.GamePlay.Logic.ProxyManager;
using HotAssets.Scripts.GamePlay.Logic.Role;
using HotAssets.Scripts.GamePlay.Logic.TimeLine;
using HotAssets.Scripts.GamePlay.Logic.Unit.Component;
using HotAssets.Scripts.GamePlay.Logic.Unit.Core;

namespace HotAssets.Scripts.GamePlay.Logic.Unit.Role
{
    public class RoleBrian:IBrian
    {
        private RoleUnit _roleUnit;

        private TimelineProxy _timelineProxy;
        
        public RoleState RoleState
        {
            get => _roleUnit.RoleState + _roleUnit.TimelineControlState;
        }
        
        //来自操作或者ai的移动请求信息
        public fix3 MoveOrder = fix3.zero;

        // 8方向瞄准指令
        public GamePlayDefine.EAimDirection AimOrder = GamePlayDefine.EAimDirection.Right;

        // 朝向（供渲染层翻转Sprite）
        public bool FacingRight { get; private set; } = true;

        // 竖向速度（重力 + 跳跃）
        private fix _verticalVelocity = fix.Zero;

        /// <summary>当前竖向速度，供 MapProxy 判断角色是否正在起跳</summary>
        public fix VerticalVelocity => _verticalVelocity;

        private fix _gravity = -20f;
        private fix _jumpForce = 10f;
        
        ///<summary>
        ///角色主动期望的移动方向
        ///</summary>
        public fix MoveDegree{
            get{
                return _wishToMoveDegree;
            }
        }
        
        private fix _wishToMoveDegree = 0.00f;
        
        ///<summary>
        ///角色主动期望的面向（返回瞄准方向角度，供 FireBullet 使用）
        ///</summary>
        public fix FaceDegree{
            get{
                return GamePlayDefine.AimDirectionToDegree(AimOrder);
            }
        }
        
        //来自操作或者ai的旋转角度请求
        public fix RotateToOrder;
        
        //来自强制发生的位移信息，通常是技能效果等导致的，比如翻滚、被推开等
        private List<MovePreorder> forceMove = new List<MovePreorder>();
        
        //来自强制执行的旋转角度
        private List<fix> forceRotate = new List<fix>();

        //收到的来自各方的播放动画的请求
        private List<string> animOrder = new List<string>();

        // 复用列表，避免每帧 GC alloc
        private readonly List<BuffObj> _toRemoveBuffs = new List<BuffObj>();
        
        
        public static RoleBrian CreateRoleBrian(RoleUnit roleUnit)
        {
            RoleBrian roleBrian = ReferencePool.Acquire<RoleBrian>();
            roleBrian._roleUnit = roleUnit;
            roleBrian._timelineProxy = GameProxyManger.Instance.GetProxy<TimelineProxy>();
            return roleBrian;
        }
        
        public void Clear()
        {
            _roleUnit = null;
            _timelineProxy = null;
            _wishToMoveDegree = 0;
            _verticalVelocity = fix.Zero;
            AimOrder = GamePlayDefine.EAimDirection.Right;
            FacingRight = true;
            forceMove.Clear();
            forceRotate.Clear();
            animOrder.Clear();
        }

        public void LogicUpdate(fix deltaTime)
        {
            if (RoleState.IsDeath)
            {
                return;
            }

            // 每帧开始前重置落地状态，防止上一帧的接地状态残留
            // UnitMove 会在本帧通过 OR 操作重新设置 IsGrounded，确保状态准确
            _roleUnit.RoleState.IsGrounded = false;

            //如果角色没死，做这些事情：

            //无敌时间减少
            if (RoleState.IsImmune)
            {
                RoleState.ImmuneTime -= deltaTime;
            }
            
            //技能冷却时间
            for (int i = 0; i < _roleUnit.Data.Skills.Count; i++){
                if (_roleUnit.Data.Skills[i].Cooldown > 0){
                    _roleUnit.Data.Skills[i].Cooldown -= deltaTime;
                }
            }
            
            //对身上的buff进行管理
            _toRemoveBuffs.Clear();
            for (int i = 0; i < _roleUnit.Data.Buffs.Count; i++)
            {
                BuffObj buffObj = _roleUnit.Data.Buffs[i];
                if (buffObj.permanent == false)
                {
                    buffObj.duration -= deltaTime;
                }
                
                buffObj.timeElapsed += deltaTime;

                if (buffObj.model.tickTime > 0 && buffObj.model.onTick != null){
                    //float取模不精准，所以用x1000后的整数来
                    if (fixMath.roundToInt(buffObj.timeElapsed * 1000) % fixMath.roundToInt(buffObj.model.tickTime * 1000) == 0){
                        buffObj.model.onTick(buffObj);
                        buffObj.ticked += 1;
                    }
                }

                //只要duration <= 0，不管是否是permanent都移除掉
                if (buffObj.duration <= 0 ||buffObj.stack <= 0){
                    if (buffObj.model.onRemoved != null){
                        buffObj.model.onRemoved(buffObj);
                    }
                    _toRemoveBuffs.Add(buffObj);
                }
            }

            if (_toRemoveBuffs.Count > 0)
            {
                for (int i = 0; i < _toRemoveBuffs.Count; i++)
                {
                    _roleUnit.Data.Buffs.Remove(_toRemoveBuffs[i]);
                }
                AttrRecheck();
            }
            
            bool tryRun = RoleState.canMove && MoveOrder != fix3.zero;

            bool wishToMove = MoveOrder != fix3.zero;
            if (wishToMove)
            {
                _wishToMoveDegree = fixMath.atan2(MoveOrder.x, MoveOrder.y) * 180 / fixMath.PI;
            }

            CalcMove(deltaTime);
            CalcRotation();
            // CalcAnimation 已移至 RoleUnit.LogicUpdate 中，在 UnitMove 结算后调用
            //MoveOrder = fix3.zero;
        }

        /// <summary>
        /// 动画更新，在 UnitMove 结算完成后由 RoleUnit 调用，确保 IsGrounded 状态已稳定。
        /// </summary>
        public void UpdateAnimation()
        {
            bool tryRun = RoleState.canMove && MoveOrder != fix3.zero;
            CalcAnimation(tryRun);
        }
        
        ///<summary>
        ///重新计算所有属性，并且获得一个最终属性
        ////其实这个应该走脚本函数返回，抛给脚本函数多个ChaProperty，由脚本函数运作他们的运算关系，并返回结果
        ///</summary>
        private void AttrRecheck(){
            _roleUnit.RoleState.Origin();
            _roleUnit.Data.Prop.Zero();

            ChaProperty[] buffProp = _roleUnit.Data.BuffProp;
            List<BuffObj> buffs = _roleUnit.Data.Buffs;
            for (var i = 0; i < buffProp.Length; i++) buffProp[i].Zero();
            for (int i = 0; i < buffs.Count; i++){
                for (int j = 0; j < fixMath.min(buffProp.Length, buffs[i].model.propMod.Length); j++){
                    buffProp[j] += buffs[i].model.propMod[j] * buffs[i].stack;
                }
                _roleUnit.RoleState += buffs[i].model.stateMod;
            }
            
            _roleUnit.Data.Prop = (_roleUnit.Data.BaseProp + _roleUnit.Data.EquipmentProp + buffProp[0]) * buffProp[1];
            //_roleUnit.RoleBehaviour.UnitMove.bodyRadius =_roleUnit.Data._prop.bodyRadius;
        }

        /// <summary>
        /// 计算并提交本帧移动速度，包含重力、贴墙下滑限速、强制位移。
        /// 贴墙检测依赖上一帧 UnitMove.IsHitWall（单帧延迟，游戏体感无感知）。
        /// </summary>
        private void CalcMove(fix timePassed)
        {
            if (RoleState.canMove == false)
            {
                MoveOrder = fix3.zero;
            }

            UnitMove unitMove = _roleUnit.RoleBehaviour.UnitMove;

            // 应用重力：已落地（含移动平台）时不累积，避免平台上速度无限增大导致穿透
            if (!_roleUnit.RoleState.IsGrounded)
            {
                _verticalVelocity += _gravity * timePassed;
            }

            // ── 贴墙下滑检测 ────────────────────────────────────────
            // 条件：空中 + 上一帧撞墙 + 正在下落 + 玩家持续向墙方向施压
            bool pressingIntoWall = unitMove.IsHitWall &&
                ((unitMove.IsHitWallOnRight && MoveOrder.x > fix.Zero) ||
                 (!unitMove.IsHitWallOnRight && MoveOrder.x < fix.Zero));

            bool isWallSliding = !unitMove.IsGrounded &&
                                 pressingIntoWall &&
                                 _verticalVelocity < fix.Zero;

            if (isWallSliding)
            {
                // 将下落速度钳制到 WallSlideSpeed（表中精度同 MoveSpeed，×0.1 转 m/s）
                fix maxSlideVelo = -(fix)(_roleUnit.Data.Prop.WallSlideSpeed * 0.1f);
                _verticalVelocity = fixMath.max(_verticalVelocity, maxSlideVelo);

                _roleUnit.RoleState.IsOnWall = true;
                _roleUnit.RoleState.WallOnRight = unitMove.IsHitWallOnRight;
                // 贴墙时重置二段跳计数
                _roleUnit.RoleState.JumpCount = 0;
            }
            else
            {
                _roleUnit.RoleState.IsOnWall = false;
            }
            // ── 贴墙检测结束 ────────────────────────────────────────

            // 落地时归零竖向速度，但保留一个微小的向下探测速度（ground probe）
            // 原因：velocity.y=0 时 GetNearestHorizontalBlock(dir=0) 直接返回原位，
            // yBlocked 恒为 false，导致 IsGrounded 每帧在 true/false 间震荡，
            // 动画疯狂切换 Run/Fall。保留小探测速度让 UnitMove 每帧都能探到地面。
            // 地形落地 或 上帧站在移动平台上，均重置竖向速度为探测值
            if ((unitMove.IsGrounded || _roleUnit.RoleState.IsGrounded) && _verticalVelocity < fix.Zero)
            {
                // 探测值应该足够大以穿透一个网格，防止边缘掉落时失去落地状态
                _verticalVelocity = (fix)(-2.0f);
                _roleUnit.RoleState.IsOnWall = false;
                // 落地时重置二段跳计数
                _roleUnit.RoleState.JumpCount = 0;
            }

            int fmIndex = 0;
            while (fmIndex < forceMove.Count){
                MoveOrder += forceMove[fmIndex].VeloInTime(timePassed);
                if (forceMove[fmIndex].duration <= 0)
                {
                    forceMove.RemoveAt(fmIndex);
                }else{
                    fmIndex++;
                }
            }

            // 水平来自输入，竖向来自物理
            fix3 velocity = new fix3(MoveOrder.x, _verticalVelocity, fix.Zero);
            _roleUnit.RoleBehaviour.UnitMove.MoveBy(velocity);
        }
        
        /// <summary>
        /// 计算角色朝向：优先跟随移动方向，无水平移动时跟随瞄准方向水平分量。
        /// canRotate 仅约束主动旋转请求，不拦截 2D 朝向翻转，确保技能期间切换方向时 Sprite 能正确翻转。
        /// </summary>
        private void CalcRotation()
        {
            // 横版 2D：FacingRight 由移动和瞄准共同决定，不受 canRotate 约束
            if (MoveOrder.x > fix.Zero)
                FacingRight = true;
            else if (MoveOrder.x < fix.Zero)
                FacingRight = false;
            else
            {
                // 无水平移动输入（含技能锁移动期间 MoveOrder 被清零）时，跟随瞄准方向水平分量
                switch (AimOrder)
                {
                    case GamePlayDefine.EAimDirection.Right:
                    case GamePlayDefine.EAimDirection.RightUp:
                    case GamePlayDefine.EAimDirection.RightDown:
                        FacingRight = true;
                        break;
                    case GamePlayDefine.EAimDirection.Left:
                    case GamePlayDefine.EAimDirection.LeftUp:
                    case GamePlayDefine.EAimDirection.LeftDown:
                        FacingRight = false;
                        break;
                }
            }
        }
        
        //再是动画处理
        private void CalcAnimation(bool tryRun)
        {
            // 角色有活跃的技能 Timeline 时，跳过移动动画
            // 动画由 Timeline 的 CasterPlayAnim 节点负责，避免 Idle/Run 每帧覆盖技能动画
            if (_timelineProxy != null && _timelineProxy.CasterHasTimeline(_roleUnit))
            {
                return;
            }

            cfg.Anim.Direction dir = FacingRight
                ? cfg.Anim.Direction.Right
                : cfg.Anim.Direction.Left;

            // 使用 RoleState.IsGrounded，同时兼容地形碰撞（UnitMove 写入）和移动平台（UpdatePlatforms 写入）两种落地来源
            bool isGrounded = _roleUnit.RoleState.IsGrounded;

            if (_verticalVelocity > fix.Zero)
            {
                _roleUnit.RoleBehaviour.AddAnim(dir, cfg.Anim.Type.Jump, _roleUnit.Data.ActionSpeed);
            }
            else if (_roleUnit.RoleState.IsOnWall)
            {
                // 贴墙下滑动画（需在 #AnimDef.xlsx 中配置 WallSlide 类型，并重新生成配置表）
                _roleUnit.RoleBehaviour.AddAnim(dir, cfg.Anim.Type.WallSlide, _roleUnit.Data.ActionSpeed);
            }
            else if (!isGrounded && _verticalVelocity < fix.Zero)
            {
                _roleUnit.RoleBehaviour.AddAnim(dir, cfg.Anim.Type.Fall, _roleUnit.Data.ActionSpeed);
            }
            else if (tryRun)
            {
                _roleUnit.RoleBehaviour.AddAnim(dir, cfg.Anim.Type.Run, _roleUnit.Data.ActionSpeed);
            }
            else
            {
                _roleUnit.RoleBehaviour.AddAnim(dir, cfg.Anim.Type.Idle, _roleUnit.Data.ActionSpeed);
            }
        }
        
        /// <summary>
        /// 跳跃指令：地面普通跳；贴墙状态蹬墙跳（垂直力 + 反向水平推力）。
        /// </summary>
        public void OrderJump()
        {
            UnitMove unitMove = _roleUnit.RoleBehaviour.UnitMove;

            // 同时检查地形落地和平台落地状态
            bool isActuallyGrounded = unitMove.IsGrounded || _roleUnit.RoleState.IsGrounded;

            if (isActuallyGrounded)
            {
                // 普通跳跃
                _verticalVelocity = _jumpForce;
                // 跳跃时立即清除落地状态，防止下一帧的动画判定错误
                _roleUnit.RoleState.IsGrounded = false;
            }
            else if (_roleUnit.RoleState.IsOnWall)
            {
                // 蹬墙跳：垂直力来自表，水平力反方向施压
                _verticalVelocity = _roleUnit.Data.WallJumpForceY;

                fix xDir = _roleUnit.RoleState.WallOnRight
                    ? -_roleUnit.Data.WallJumpForceX
                    :  _roleUnit.Data.WallJumpForceX;

                forceMove.Add(new MovePreorder(
                    new fix3(xDir, fix.Zero, fix.Zero),
                    _roleUnit.Data.WallJumpDuration
                ));

                _roleUnit.RoleState.IsOnWall = false;
            }
        }

        /// <summary>
        /// 直接设置竖向速度（供 Timeline ApplyForceToCaster 节点调用，用于二段跳等）。
        /// 同时递增 JumpCount，由调用方保证只在空中调用。
        /// </summary>
        public void SetVerticalVelocity(fix speed)
        {
            _verticalVelocity = speed;
            _roleUnit.RoleState.JumpCount++;
        }

        /// <summary>
        /// 角色落在移动平台上时调用：将竖向速度重置为地面探测值，清零跳跃计数。
        /// 由 PlatformProxy 在检测到角色站在平台上时调用，不应从其他地方调用。
        /// 只在下落时重置速度，跳跃时保留速度（允许离开平台）
        /// </summary>
        public void OnStandOnPlatform()
        {
            // 只有在下落时才重置为探测值，跳跃时保持原有速度
            if (_verticalVelocity < fix.Zero)
            {
                // 探测值应该足够大以穿透一个网格，防止边缘掉落
                _verticalVelocity = (fix)(-2.0f);
            }
            _roleUnit.RoleState.JumpCount = 0;
        }

        /// <summary>
        /// 向角色施加一段持续水平推力（供 Timeline 策略和内部调用共用）。
        /// velocity - 速度向量；duration - 持续秒数
        /// </summary>
        public void ApplyForce(fix3 velocity, fix duration)
        {
            forceMove.Add(new MovePreorder(velocity, duration));
        }

        public void OrderAim(GamePlayDefine.EAimDirection dir)
        {
            AimOrder = dir;
        }

        ///<summary>
        ///为角色添加buff，当然，删除也是走这个的
        ///</summary>
        public void AddBuff(AddBuffInfo buff){
            BuffObj hasOne;
            bool hasExistingBuff = _roleUnit.Data.TryGetBuffById(buff.buffModel.id, out hasOne, buff.caster);
            int modStack = (int)fixMath.min(buff.addStack, buff.buffModel.maxStack);
            bool toRemove = false;
            BuffObj toAddBuff = null;
        
            if (hasExistingBuff){
                //已经存在
                hasOne.buffParam = new Dictionary<string, object>();
                if (buff.buffParam != null){
                    foreach (KeyValuePair<string, object> kv in buff.buffParam)
                    {
                        hasOne.buffParam[kv.Key] = kv.Value;
                    };
                }
            
                hasOne.duration = (buff.durationSetTo == true) ? buff.duration : (buff.duration + hasOne.duration);
                int afterAdd = hasOne.stack + modStack;
                modStack = afterAdd >= hasOne.model.maxStack ?
                    (hasOne.model.maxStack - hasOne.stack) :
                    (afterAdd <= 0 ? (0 - hasOne.stack) : modStack);
                hasOne.stack += modStack;
                hasOne.permanent = buff.permanent;
                toAddBuff = hasOne;
                toRemove = hasOne.stack <= 0;
            }
            else
            {
                //新建
                toAddBuff = BuffObj.Create(
                    buff.buffModel,
                    buff.caster,
                    _roleUnit,
                    buff.duration,
                    buff.addStack,
                    buff.permanent,
                    buff.buffParam
                );
                _roleUnit.Data.Buffs.Add(toAddBuff);
                _roleUnit.Data.Buffs.Sort((a, b)=>{
                    return a.model.priority.CompareTo(b.model.priority);
                });
            }
            if (toRemove == false && buff.buffModel.onOccur != null){
                buff.buffModel.onOccur(toAddBuff, modStack);
            }
            AttrRecheck();
        }
    }
}
