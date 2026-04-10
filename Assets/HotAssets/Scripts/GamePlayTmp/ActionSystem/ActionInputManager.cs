using System.Collections.Generic;

namespace GamePlay.ActionSystem
{
    /// <summary>
    /// 动作输入管理器（银河恶魔城版，删除连招系统）
    /// </summary>
    public class ActionInputManager
    {
        /// <summary>
        /// 状态机引用
        /// </summary>
        private ActionStateMachine _stateMachine;

        public ActionInputManager(ActionStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        /// <summary>
        /// 处理移动输入
        /// </summary>
        public void OnMoveInput(float direction)
        {
            if (direction != 0f)
            {
                // 如果当前是待机状态，切换到移动状态
                if (_stateMachine.CurrentState?.Config.ActionType == ActionType.Idle)
                {
                    _stateMachine.ChangeState(ActionType.Run);
                }

                // 如果当前是移动状态，更新方向
                if (_stateMachine.CurrentState is States.RunState runState)
                {
                    runState.SetMoveDirection(direction);
                }
            }
            else
            {
                // 停止移动，返回待机
                if (_stateMachine.CurrentState?.Config.ActionType == ActionType.Run)
                {
                    _stateMachine.ChangeState(ActionType.Idle);
                }
            }
        }

        /// <summary>
        /// 处理攻击输入
        /// </summary>
        public void OnAttackInput()
        {
            _stateMachine.ChangeState(ActionType.Attack);
        }

        /// <summary>
        /// 处理空中下砸攻击输入
        /// </summary>
        public void OnAirAttackInput()
        {
            _stateMachine.ChangeState(ActionType.AirAttack);
        }

        /// <summary>
        /// 处理技能输入
        /// </summary>
        public void OnSkillInput(int skillId = 1)
        {
            // 先设置技能ID再切换状态
            var skillState = _stateMachine.GetState<States.SkillState>(ActionType.Skill);
            if (skillState != null)
            {
                skillState.SetSkillId(skillId);
            }

            _stateMachine.ChangeState(ActionType.Skill);
        }

        /// <summary>
        /// 处理跳跃输入
        /// </summary>
        public void OnJumpInput()
        {
            // 贴墙状态下跳跃触发蹬墙跳
            if (_stateMachine.CurrentState?.Config.ActionType == ActionType.WallSlide)
            {
                _stateMachine.ChangeState(ActionType.WallJump);
                return;
            }

            // 空中状态下尝试二段跳
            var currentType = _stateMachine.CurrentState?.Config.ActionType;
            if (currentType == ActionType.Fall || currentType == ActionType.Jump)
            {
                var jumpState = _stateMachine.GetState<States.JumpState>(ActionType.Jump);
                if (jumpState != null && jumpState.CanDoubleJump)
                {
                    jumpState.PerformDoubleJump();
                    _stateMachine.ChangeState(ActionType.Jump);
                    return;
                }
                return;
            }

            _stateMachine.ChangeState(ActionType.Jump);
        }

        /// <summary>
        /// 处理跳跃键释放（用于控制跳跃高度）
        /// </summary>
        public void OnJumpRelease()
        {
            if (_stateMachine.CurrentState is States.JumpState jumpState)
            {
                jumpState.SetJumpKeyHeld(false);
            }
        }

        /// <summary>
        /// 处理翻滚输入
        /// </summary>
        public void OnDodgeInput()
        {
            _stateMachine.ChangeState(ActionType.Dodge);
        }

        /// <summary>
        /// 处理冲刺输入
        /// </summary>
        public void OnDashInput(float direction)
        {
            // 先设置方向再切换状态
            var dashState = _stateMachine.GetState<States.DashState>(ActionType.Dash);
            if (dashState != null)
            {
                dashState.SetDashDirection(direction);
            }

            _stateMachine.ChangeState(ActionType.Dash);
        }

        /// <summary>
        /// 处理下蹲输入
        /// </summary>
        public void OnCrouchInput(bool isPressed)
        {
            if (isPressed)
            {
                _stateMachine.ChangeState(ActionType.Crouch);
            }
            else
            {
                // 松开下蹲键，返回待机
                if (_stateMachine.CurrentState?.Config.ActionType == ActionType.Crouch)
                {
                    _stateMachine.ChangeState(ActionType.Idle);
                }
            }
        }

        /// <summary>
        /// 处理贴墙检测（由外部物理检测调用）
        /// </summary>
        public void OnWallContact(bool isTouchingWall)
        {
            var currentType = _stateMachine.CurrentState?.Config.ActionType;

            if (isTouchingWall && currentType == ActionType.Fall)
            {
                // 下落中接触墙壁，进入贴墙下滑
                _stateMachine.ChangeState(ActionType.WallSlide);
            }
            else if (!isTouchingWall && currentType == ActionType.WallSlide)
            {
                // 离开墙壁，进入下落
                _stateMachine.ChangeState(ActionType.Fall);
            }
        }
    }
}
