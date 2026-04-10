using System.Collections.Generic;
using GameFramework;
using HotAssets.Scripts.GamePlay.Logic.Buff;
using HotAssets.Scripts.GamePlay.Logic.Common;
using HotAssets.Scripts.GamePlay.Logic.Skill;
using HotAssets.Scripts.GamePlay.Logic.Unit.Core;
using HotAssets.Scripts.GamePlay.Render.Role;

namespace HotAssets.Scripts.GamePlay.Logic.Role
{
    public class RoleData:IReference
    {
        protected GamePlayDefine.RoleType _roleType = GamePlayDefine.RoleType.Null;

        public GamePlayDefine.RoleType RoleType
        {
            get
            {
                return _roleType;
            }
        }
        
        public int RoleId { get; protected set; }
        public string Name { get; protected set; }
        public fix3 InitPosition { get; set; }
        public string AssetPath { get; protected set; }

        private RoleEntity _roleEntity;

        /// <summary>
        /// 哪个用户在操作这个对象
        /// </summary>
        public string OperateId;
        
        public void Clear()
        {
            _roleType = GamePlayDefine.RoleType.Null;
        }
        
        ///<summary>
        ///角色移动力，单位：米/秒
        ///</summary>
        public fix MoveSpeed{get{
            //这个公式也可以通过给策划脚本接口获得，这里就写代码里了，不走策划脚本了
            return this.Prop.MoveSpeed;
        }}

        ///<summary>
        ///角色行动速度，是一个timescale，最小0.1，初始行动速度值也是100。
        ///</summary>
        public fix ActionSpeed{
            get{
                return this.Prop.ActionSpeed;
            }
        }
        
        ///<summary>
        ///角色的基础属性，也就是每个角色“裸体”且不带任何buff的“纯粹的属性”
        ///先写死，正式的应该读表
        ///</summary>
        public ChaProperty BaseProp = new ChaProperty(
            100, 100, 0, 20, 1
        );

        ///<summary>
        ///角色来自buff的属性
        ///这个数组并不是说每个buff可以占用一条数据，而是分类总和
        ///在这个游戏里buff带来的属性总共有2类，plus和times，用策划设计的公式就是plus的属性加完之后乘以times的属性
        ///所以数组长度其实只有2：[0]buffPlus, [1]buffTimes
        ///</summary>
        public ChaProperty[] BuffProp = new ChaProperty[2]{ChaProperty.zero, ChaProperty.zero};

        ///<summary>
        ///来自装备的属性
        ///</summary>
        public ChaProperty EquipmentProp = ChaProperty.zero;

        ///<summary>
        ///角色的技能
        ///</summary>
        public List<SkillObj> Skills = new List<SkillObj>();

        ///<summary>
        ///角色身上的buff
        ///</summary>
        public List<BuffObj> Buffs = new List<BuffObj>();
        
        ///<summary>
        ///角色当前的属性
        ///</summary>
        public ChaProperty Property{get{
            return Prop;
        }}
        
        public ChaProperty Prop = ChaProperty.zero;
        
        ///<summary>
        ///角色现有的资源，比如hp之类的
        ///</summary>
        public ChaResource Resource = new ChaResource(1);
        
        ///<summary>
        ///角色所处阵营，阵营不同就会对打
        ///</summary>
        public int Side = 0;

        ///<summary>
        ///根据tags可以判断出这是什么样的人
        ///</summary>
        public string[] tags = new string[0];
        
        ///<summary>
        ///获取角色身上对应的buffObj
        ///<param name="id">buff的model的id</param>
        ///<param name="caster">如果caster不是空，那么就代表只有buffObj.caster在caster里面的才符合条件</param>
        ///<return>符合条件的buffObj数组</return>
        ///</summary>
        public List<BuffObj> GetBuffById(int id, List<IUnit> caster = null){
            List<BuffObj> res = new List<BuffObj>();
            for (int i = 0; i < Buffs.Count;  i++){
                if (Buffs[i].model.id == id 
                    && (caster == null 
                        || caster.Count <= 0 
                        || caster.Contains(Buffs[i].caster) == true)){
                    res.Add(Buffs[i]);
                }
            }
            return res;
        }

        public int AIId = 0;

        public int AnimId = 0;

        // ── 蹬墙跳固定参数 ──────────────────────────────────────────────
        /// <summary>
        /// 蹬墙跳垂直推力（米/秒）
        /// </summary>
        public fix WallJumpForceY = (fix)9.0f;

        /// <summary>
        /// 蹬墙跳水平推力（米/秒）
        /// </summary>
        public fix WallJumpForceX = (fix)6.0f;

        /// <summary>
        /// 蹬墙跳水平力持续时长（秒）
        /// </summary>
        public fix WallJumpDuration = (fix)0.18f;
    }
}