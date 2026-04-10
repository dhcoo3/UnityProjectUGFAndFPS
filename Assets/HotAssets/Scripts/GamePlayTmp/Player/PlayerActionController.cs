using System.Collections.Generic;
using GamePlay.ActionSystem;
using GamePlay.ActionSystem.States;
using UnityEngine;

namespace GamePlay.Player
{
    /// <summary>
    /// 玩家动作控制器（银河恶魔城版）
    /// </summary>
    public class PlayerActionController : MonoBehaviour, IActionController
    {
        /// <summary>动作状态机</summary>
        private ActionStateMachine _stateMachine;
        /// <summary>输入管理器</summary>
        private ActionInputManager _inputManager;

        /// <summary>地面检测起点偏移</summary>
        [SerializeField] private Vector2 groundCheckOffset = new Vector2(0f, 0.05f);

        /// <summary>地面检测射线长度</summary>
        [SerializeField] private float groundCheckDistance = 0.15f;

        /// <summary>地面检测层级</summary>
        [SerializeField] private LayerMask groundLayer = ~0;

        /// <summary>水平墙壁检测层级（不应包含地面所在层，避免踩地时误检）</summary>
        [SerializeField] private LayerMask wallLayer = ~0;

        /// <summary>水平碰撞皮肤厚度，防止紧贴时抖动</summary>
        [SerializeField] private float wallCheckSkinWidth = 0.05f;

        /// <summary>碰撞盒中心相对于 transform.position 的偏移</summary>
        [SerializeField] private Vector2 colliderOffset = new Vector2(0f, 0.5f);

        /// <summary>碰撞盒尺寸（宽、高）</summary>
        [SerializeField] private Vector2 colliderSize = new Vector2(0.6f, 1f);

        /// <summary>重力加速度</summary>
        private const float Gravity = 20f;

        /// <summary>最大下落速度</summary>
        private const float MaxFallSpeed = 15f;

        /// <summary>当前垂直速度</summary>
        private float _verticalVelocity;

        /// <summary>是否在地面上</summary>
        public bool IsGrounded { get; private set; }

        /// <summary>是否启用输入</summary>
        public bool InputEnabled { get; set; } = true;

        /// <summary>角色朝向（true为右）</summary>
        public bool FacingRight { get; private set; } = true;

        /// <summary>当前生命值</summary>
        private float _currentHealth = 100f;

        /// <summary>最大生命值</summary>
        private const float MaxHealth = 100f;

        /// <summary>已解锁的能力集合</summary>
        private HashSet<ActionType> _unlockedAbilities;

        /// <summary>动画控制器</summary>
        private Animator _animator;

        private void Awake()
        {
            _unlockedAbilities = new HashSet<ActionType>();
            _animator = GetComponentInChildren<Animator>();
            InitializeActionSystem();
        }

        /// <summary>
        /// 初始化动作系统
        /// </summary>
        private void InitializeActionSystem()
        {
            _stateMachine = new ActionStateMachine();
            RegisterStates();
            _stateMachine.Initialize();
            _stateMachine.SetVerticalVelocity = SetVerticalVelocity;
            _stateMachine.GetVerticalVelocity = GetVerticalVelocity;
            _inputManager = new ActionInputManager(_stateMachine);
            _stateMachine.OnStateChanged += OnActionStateChanged;

            UnityGameFramework.Runtime.Log.Info("玩家动作系统初始化完成（银河恶魔城版）");
        }

        /// <summary>
        /// 注册所有动作状态（银河恶魔城简化优先级：0=普通, 1=动作, 2=受击, 3=死亡）
        /// </summary>
        private void RegisterStates()
        {
            // 待机 - 优先级0
            _stateMachine.RegisterState(
                new IdleState(),
                new ActionConfig(ActionType.Idle, priority: 0, canBeInterrupted: true, duration: 0f)
            );

            // 移动 - 优先级0
            _stateMachine.RegisterState(
                new RunState(),
                new ActionConfig(ActionType.Run, priority: 0, canBeInterrupted: true, duration: 0f)
            );

            // 下蹲 - 优先级0
            _stateMachine.RegisterState(
                new CrouchState(),
                new ActionConfig(ActionType.Crouch, priority: 0, canBeInterrupted: true, duration: 0f)
            );

            // 跳跃 - 优先级1
            _stateMachine.RegisterState(
                new JumpState(),
                new ActionConfig(ActionType.Jump, priority: 1, canBeInterrupted: false,
                    duration: 1.5f, cooldownTime: 0.05f)
            );

            // 下落 - 优先级1
            _stateMachine.RegisterState(
                new FallState(),
                new ActionConfig(ActionType.Fall, priority: 1, canBeInterrupted: false, duration: 5f)
            );

            // 贴墙下滑 - 优先级1
            _stateMachine.RegisterState(
                new WallSlideState(),
                new ActionConfig(ActionType.WallSlide, priority: 1, canBeInterrupted: true, duration: 0f)
            );

            // 蹬墙跳 - 优先级1
            _stateMachine.RegisterState(
                new WallJumpState(),
                new ActionConfig(ActionType.WallJump, priority: 1, canBeInterrupted: false, duration: 0.5f)
            );

            // 普通攻击 - 优先级1，前摇0.08秒
            _stateMachine.RegisterState(
                new AttackState(),
                new ActionConfig(ActionType.Attack, priority: 1, canBeInterrupted: false,
                    duration: 0.4f, cooldownTime: 0.15f, cancelableTime: 0.2f, attackDelay: 0.08f)
            );

            // 空中下砸 - 优先级1
            _stateMachine.RegisterState(
                new AirAttackState(),
                new ActionConfig(ActionType.AirAttack, priority: 1, canBeInterrupted: false, duration: 3f)
            );

            // 技能 - 优先级1，前摇0.15秒
            _stateMachine.RegisterState(
                new SkillState(),
                new ActionConfig(ActionType.Skill, priority: 1, canBeInterrupted: false,
                    duration: 0.8f, cooldownTime: 3f, attackDelay: 0.15f)
            );

            // 冲刺 - 优先级1
            _stateMachine.RegisterState(
                new DashState(),
                new ActionConfig(ActionType.Dash, priority: 1, canBeInterrupted: false,
                    duration: 0.25f, cooldownTime: 0.8f)
            );

            // 翻滚 - 优先级1
            _stateMachine.RegisterState(
                new DodgeState(),
                new ActionConfig(ActionType.Dodge, priority: 1, canBeInterrupted: false,
                    duration: 0.35f, cooldownTime: 0.6f)
            );

            // 受击 - 优先级2
            _stateMachine.RegisterState(
                new HitState(),
                new ActionConfig(ActionType.Hit, priority: 2, canBeInterrupted: false, duration: 0.25f)
            );

            // 死亡 - 优先级3
            _stateMachine.RegisterState(
                new DeadState(),
                new ActionConfig(ActionType.Dead, priority: 3, canBeInterrupted: false, duration: 2f)
            );
        }

        private void Update()
        {
            if (!InputEnabled)
            {
                return;
            }

            float deltaTime = Time.deltaTime;

            _stateMachine.Update(deltaTime);

            // 水平移动
            UpdateHorizontalMovement(deltaTime);

            // 垂直速度与位置更新
            UpdateVerticalMovement(deltaTime);

            HandleInput();
        }

        /// <summary>
        /// 更新水平移动，使用角色 Collider2D 做 BoxCast 水平碰撞检测
        /// </summary>
        private void UpdateHorizontalMovement(float deltaTime)
        {
            float moveX = 0f;

            if (_stateMachine.CurrentState is RunState runState)
            {
                runState.UpdateMovement(deltaTime);
                moveX = runState.GetCurrentSpeed() * deltaTime;
            }
            else if (_stateMachine.CurrentState is FallState fallState)
            {
                // 空中水平控制
                float horizontal = Input.GetAxisRaw("Horizontal");
                moveX = horizontal * 5f * fallState.GetAirControlMultiplier() * deltaTime;
            }
            else if (_stateMachine.CurrentState is JumpState jumpState)
            {
                // 跳跃中水平控制
                float horizontal = Input.GetAxisRaw("Horizontal");
                moveX = horizontal * 5f * jumpState.GetAirControlMultiplier() * deltaTime;
            }

            if (moveX != 0f)
            {
                float direction = Mathf.Sign(moveX);
                Vector2 center = (Vector2)transform.position + colliderOffset;
                // Y 方向略微收缩，避免脚底/头顶边缘误检测地面或天花板
                Vector2 castSize = new Vector2(colliderSize.x, colliderSize.y - wallCheckSkinWidth * 2f);
                RaycastHit2D hit = Physics2D.BoxCast(
                    center, castSize, 0f,
                    new Vector2(direction, 0f),
                    Mathf.Abs(moveX) + wallCheckSkinWidth,
                    wallLayer
                );

                if (hit.collider != null && Mathf.Abs(hit.normal.y) < 0.5f && hit.normal.x * direction < 0f)
                {
                    // hit.normal 与移动方向相对（法线朝向我们），才是前方墙壁
                    // hit.normal 与移动方向同向说明是背后重叠的碰撞体，忽略
                    float allowedMove = Mathf.Max(0f, hit.distance - wallCheckSkinWidth) * direction;
                    transform.position += new Vector3(allowedMove, 0f, 0f);
                }
                else
                {
                    transform.position += new Vector3(moveX, 0f, 0f);
                }
            }
        }

        /// <summary>
        /// 更新垂直运动：重力、射线碰撞、位置修正
        /// </summary>
        private void UpdateVerticalMovement(float deltaTime)
        {
            // 地面状态不施加重力，先做一次贴地检测
            if (IsGrounded && _verticalVelocity <= 0f)
            {
                // 贴地检测：仍在地面则不处理
                Vector2 checkOrigin = (Vector2)transform.position + groundCheckOffset;
                RaycastHit2D groundHit = Physics2D.Raycast(checkOrigin, Vector2.down, groundCheckDistance, groundLayer);
                if (groundHit.collider != null)
                {
                    _verticalVelocity = 0f;
                    return;
                }
                // 离开地面边缘，开始下落
                IsGrounded = false;
            }

            // 施加重力
            _verticalVelocity -= Gravity * deltaTime;
            if (_verticalVelocity < -MaxFallSpeed)
            {
                _verticalVelocity = -MaxFallSpeed;
            }

            // 计算本帧位移
            float moveY = _verticalVelocity * deltaTime;

            if (moveY < 0f)
            {
                // 向下移动时，射线检测防止穿透地面
                Vector2 origin = (Vector2)transform.position + groundCheckOffset;
                float checkDist = -moveY + groundCheckDistance;
                RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, checkDist, groundLayer);

                if (hit.collider != null)
                {
                    // 修正位置到地面上方
                    float correctedY = hit.point.y - groundCheckOffset.y + groundCheckDistance;
                    transform.position = new Vector3(transform.position.x, correctedY, transform.position.z);
                    _verticalVelocity = 0f;
                    OnLanded();
                    return;
                }
            }

            // 应用位移
            transform.position += new Vector3(0f, moveY, 0f);

            // 离地且处于地面状态时切换到Fall
            if (!IsGrounded)
            {
                CheckFallTransition();
            }
        }

        /// <summary>
        /// 落地处理
        /// </summary>
        private void OnLanded()
        {
            IsGrounded = true;

            // 通知当前状态落地
            if (_stateMachine.CurrentState is FallState fallState)
            {
                fallState.SetGrounded(true);
            }
            else if (_stateMachine.CurrentState is JumpState jumpState)
            {
                jumpState.SetGrounded(true);
            }
        }

        /// <summary>
        /// 设置垂直速度（供JumpState等外部调用）
        /// </summary>
        public void SetVerticalVelocity(float velocity)
        {
            _verticalVelocity = velocity;
            if (velocity > 0f)
            {
                IsGrounded = false;
            }
        }

        /// <summary>
        /// 获取当前垂直速度
        /// </summary>
        public float GetVerticalVelocity()
        {
            return _verticalVelocity;
        }

        /// <summary>
        /// 翻转角色朝向
        /// </summary>
        private void Flip()
        {
            Vector3 scale = transform.localScale;
            scale.x = FacingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        /// <summary>
        /// 检测离地后切换到下落状态
        /// </summary>
        private void CheckFallTransition()
        {
            var currentType = _stateMachine.CurrentState?.Config.ActionType;
            if (currentType == ActionType.Idle
                || currentType == ActionType.Run
                || currentType == ActionType.Crouch)
            {
                _stateMachine.ChangeState(ActionType.Fall);
            }
        }

        /// <summary>
        /// 处理玩家输入（银河恶魔城版）
        /// </summary>
        private void HandleInput()
        {
            // 移动输入
            float horizontal = Input.GetAxisRaw("Horizontal");
            _inputManager.OnMoveInput(horizontal);

            // 更新朝向并翻转角色
            if (horizontal > 0f && !FacingRight)
            {
                FacingRight = true;
                Flip();
            }
            else if (horizontal < 0f && FacingRight)
            {
                FacingRight = false;
                Flip();
            }

            // 跳跃输入
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                _inputManager.OnJumpInput();
            }

            // 跳跃键释放（控制跳跃高度）
            if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow))
            {
                _inputManager.OnJumpRelease();
            }

            // 攻击输入
            if (Input.GetKeyDown(KeyCode.J))
            {
                // 空中+按住下键 = 下砸攻击（需解锁）
                bool isAirborne = IsInAirState();
                bool isPressingDown = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);

                if (isAirborne && isPressingDown && IsAbilityUnlocked(ActionType.AirAttack))
                {
                    _inputManager.OnAirAttackInput();
                }
                else
                {
                    _inputManager.OnAttackInput();
                }
            }

            // 技能输入
            if (Input.GetKeyDown(KeyCode.K))
            {
                _inputManager.OnSkillInput(skillId: 1);
            }

            // 冲刺输入（需解锁）
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            {
                if (IsAbilityUnlocked(ActionType.Dash))
                {
                    float dashDir = FacingRight ? 1f : -1f;
                    _inputManager.OnDashInput(dashDir);
                }
            }

            // 翻滚输入
            if (Input.GetKeyDown(KeyCode.L))
            {
                _inputManager.OnDodgeInput();
            }

            // 下蹲输入
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                // 仅在地面状态下蹲
                if (!IsInAirState())
                {
                    _inputManager.OnCrouchInput(true);
                }
            }
            else if (Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.DownArrow))
            {
                _inputManager.OnCrouchInput(false);
            }
        }

        /// <summary>
        /// 判断是否在空中状态
        /// </summary>
        private bool IsInAirState()
        {
            var currentType = _stateMachine.CurrentState?.Config.ActionType;
            return currentType == ActionType.Jump
                || currentType == ActionType.Fall
                || currentType == ActionType.WallSlide
                || currentType == ActionType.WallJump;
        }

        /// <summary>
        /// 状态改变回调，驱动动画切换
        /// </summary>
        private void OnActionStateChanged(ActionType from, ActionType to)
        {
            UnityGameFramework.Runtime.Log.Info($"动作状态切换: {from} -> {to}");
            Debug.Log($"动作状态切换: {from} -> {to}");
            // 直接播放对应动画
            if (_animator != null)
            {
                string animName = to.ToString();
                _animator.Play(animName);
            }
        }

        /// <summary>
        /// 受到伤害（银河恶魔城简化版：无防御/HitStop）
        /// </summary>
        public void TakeDamage(HitData hitData)
        {
            // 检查翻滚无敌
            if (_stateMachine.CurrentState is DodgeState dodgeState && dodgeState.IsInvincible)
            {
                UnityGameFramework.Runtime.Log.Info("翻滚无敌，免疫伤害");
                return;
            }

            // 扣血
            float finalDamage = hitData.Damage;
            _currentHealth -= finalDamage;
            UnityGameFramework.Runtime.Log.Info($"受到伤害: {finalDamage}, 剩余生命: {_currentHealth}");

            // 检查死亡
            if (_currentHealth <= 0f)
            {
                _currentHealth = 0f;
                _stateMachine.ChangeState(ActionType.Dead);
                return;
            }

            // 设置受击数据后切换状态
            var hitState = _stateMachine.GetState<HitState>(ActionType.Hit);
            if (hitState != null)
            {
                hitState.SetHitData(hitData);
            }
            _stateMachine.ChangeState(ActionType.Hit);
        }

        /// <summary>
        /// 解锁能力（银河恶魔城核心机制）
        /// </summary>
        public void UnlockAbility(ActionType abilityType)
        {
            _unlockedAbilities.Add(abilityType);
            UnityGameFramework.Runtime.Log.Info($"解锁能力: {abilityType}");

            // 特殊处理：解锁二段跳
            if (abilityType == ActionType.Jump)
            {
                var jumpState = _stateMachine.GetState<JumpState>(ActionType.Jump);
                if (jumpState != null)
                {
                    jumpState.MaxJumpCount = 2;
                    UnityGameFramework.Runtime.Log.Info("二段跳已解锁");
                }
            }
        }

        /// <summary>
        /// 检查能力是否已解锁
        /// </summary>
        public bool IsAbilityUnlocked(ActionType abilityType)
        {
            return true;
            return _unlockedAbilities.Contains(abilityType);
        }

        /// <summary>
        /// 获取当前动作状态
        /// </summary>
        public ActionType GetCurrentActionType()
        {
            return _stateMachine.CurrentState?.Config.ActionType ?? ActionType.None;
        }

        /// <summary>
        /// 获取指定动作的冷却剩余时间
        /// </summary>
        public float GetActionCooldown(ActionType actionType)
        {
            return _stateMachine.GetCooldownRemaining(actionType);
        }

        /// <summary>
        /// 强制切换到指定状态
        /// </summary>
        public void ForceChangeState(ActionType actionType)
        {
            _stateMachine.ChangeState(actionType);
        }

        /// <summary>
        /// 获取当前生命值
        /// </summary>
        public float GetCurrentHealth()
        {
            return _currentHealth;
        }

        // ========== 动画事件接口实现 ==========

        /// <summary>
        /// 动画事件：攻击开始
        /// </summary>
        public void OnAnimationEvent_AttackStart()
        {
            UnityGameFramework.Runtime.Log.Info("动画事件: 攻击开始");
        }

        /// <summary>
        /// 动画事件：攻击判定帧
        /// </summary>
        public void OnAnimationEvent_AttackHit()
        {
            if (_stateMachine.CurrentState is AttackState attackState)
            {
                attackState.OnAnimationAttackHit();
            }
        }

        /// <summary>
        /// 动画事件：攻击结束
        /// </summary>
        public void OnAnimationEvent_AttackEnd()
        {
            UnityGameFramework.Runtime.Log.Info("动画事件: 攻击结束");
        }

        /// <summary>
        /// 动画事件：脚步声
        /// </summary>
        public void OnAnimationEvent_Footstep()
        {
            // TODO: 播放脚步音效
        }

        /// <summary>
        /// 动画事件：生成特效
        /// </summary>
        public void OnAnimationEvent_SpawnEffect(string effectName)
        {
            // TODO: 生成特效
            UnityGameFramework.Runtime.Log.Info($"动画事件: 生成特效 {effectName}");
        }

        /// <summary>
        /// 动画事件：自定义事件
        /// </summary>
        public void OnAnimationEvent_Custom(string eventName)
        {
            UnityGameFramework.Runtime.Log.Info($"动画事件: 自定义事件 {eventName}");
        }

        private void OnDestroy()
        {
            if (_stateMachine != null)
            {
                _stateMachine.OnStateChanged -= OnActionStateChanged;
            }
        }

        /// <summary>
        /// 在 Scene 视图绘制碰撞盒和中心���，方便���整参数
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Vector2 center = (Vector2)transform.position + colliderOffset;

            // 绘制碰撞盒轮廓
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(center, colliderSize);

            // 绘制中心点
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(center, 0.05f);
        }
    }
}
