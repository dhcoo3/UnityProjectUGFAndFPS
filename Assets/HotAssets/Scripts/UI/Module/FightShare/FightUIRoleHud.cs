using HotAssets.Scripts.GamePlay.Logic.Unit.Role;
using HotAssets.Scripts.UI.Tool.Component;
using UnityEngine;

namespace HotAssets.Scripts.UI.Module.FightShare
{
    public class FightUIRoleHud
    {
        private UIContainer m_uiNode;
        
        private ExImage m_HpBarBg;
        private ExImage m_HpBar;
        private ExText m_Name;
  
        
        private Vector2 _tmpV2 = Vector2.zero;
        private RectTransform _rectTransform;
        private FightUIRole _fightUIRole;
        
        public FightUIRoleHud(FightUIRole fightUIRole)
        {
            _fightUIRole = fightUIRole;
        }
        
        public void RegisterUI(GameObject go)
        {
            m_uiNode = go.GetComponent<UIContainer>();
            
            if (m_uiNode == null)
            {
                return;
            }
            
            m_HpBarBg = m_uiNode.Get<ExImage>("m_HpBarBg");
            m_HpBar = m_uiNode.Get<ExImage>("m_HpBar");
            m_Name = m_uiNode.Get<ExText>("m_Name");
            
            _rectTransform = m_uiNode.GetComponent<RectTransform>();
            
            m_Name.SetText(_fightUIRole.RoleUnit.Data.OperateId);
        }

        public void UpdateHudPos(float x, float y)
        {
            _tmpV2.x = x;
            _tmpV2.y = y;
            _rectTransform.anchoredPosition =_tmpV2;
        }
        
        public void UpdateHudHp(RoleUnit roleUnit)
        {
            m_HpBar.DoFillAmount(roleUnit.Data.Resource.hp/(float)roleUnit.Data.Prop.Hp,0.1f,(f =>
            {
                
            } ));
        }
    }
}