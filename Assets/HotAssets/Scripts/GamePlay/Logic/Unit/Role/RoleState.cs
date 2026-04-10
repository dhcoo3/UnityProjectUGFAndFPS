namespace HotAssets.Scripts.GamePlay.Logic.Unit.Role
{
    ///<summary>
    ///角色的可操作状态
    ///</summary>
    public class RoleState{
    
        /// <summary>
        /// 无敌时间
        /// </summary>
        public fix ImmuneTime;

        /// <summary>
        /// 是否无敌
        /// </summary>
        public bool IsImmune
        {
            get{return ImmuneTime > 0;}
        }
    
        ///<summary>
        ///是否可以移动坐标
        ///</summary>
        public bool canMove;

        ///<summary>
        ///是否可以转身
        ///</summary>
        public bool canRotate;

        ///<summary>
        ///是否可以使用技能，这里的是“使用技能”特指整个技能流程是否可以开启
        ///如果是类似中了沉默，则应该走buff的onCast，尤其是类似wow里面沉默了不能施法但是还能放致死打击（部分技能被分类为法术，会被沉默，而不是法术的不会）
        ///</summary>
        public bool canUseSkill;
    
        /// <summary>
        /// 是否处于死亡状态
        /// </summary>
        public bool IsDeath = false;
    
        ///<summary>
        ///角色是否处于一种蓄力的状态
        ///</summary>
        public bool charging = false;

        /// <summary>
        /// 是否踩在地面上
        /// </summary>
        public bool IsGrounded = false;

        /// <summary>
        /// 是否正在贴墙下滑（空中且侧面紧贴墙壁）
        /// </summary>
        public bool IsOnWall = false;

        /// <summary>
        /// 贴墙方向：true = 右侧有墙，false = 左侧有墙（仅 IsOnWall 为 true 时有效）
        /// </summary>
        public bool WallOnRight = false;

        /// <summary>
        /// 本次离地后已使用的额外跳跃次数（二段跳计数），落地或贴墙后重置为 0
        /// </summary>
        public int JumpCount = 0;

        public RoleState(bool canMove = true, bool canRotate = true, bool canUseSkill = true){
            this.canMove = canMove;
            this.canRotate = canRotate;
            this.canUseSkill = canUseSkill;
            this.ImmuneTime = 0;
        }

        public void Origin(){
            this.canMove = true;
            this.canRotate = true;
            this.canUseSkill = true;
            this.ImmuneTime = 0;
        }

        public static RoleState origin = new RoleState(true, true, true);

        ///<summary>
        ///昏迷效果
        ///</summary>
        public static RoleState stun = new RoleState(false, false, false);

        public static RoleState operator +(RoleState cs1, RoleState cs2){
            return new RoleState(
                cs1.canMove & cs2.canMove,
                cs1.canRotate & cs2.canRotate,
                cs1.canUseSkill & cs2.canUseSkill
            );
        }
    }
}