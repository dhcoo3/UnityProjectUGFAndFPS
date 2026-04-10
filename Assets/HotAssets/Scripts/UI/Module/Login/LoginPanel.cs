
using Builtin.Scripts.Game;
using cfg.Fight;
using HotAssets.Scripts.UI;
using HotAssets.Scripts.UI.Module.FightShare;
using HotAssets.Scripts.UI.Module.Login;
using HotAssets.Scripts.UI.Tool.Component;
public class LoginPanel : ExPanel
{
    #region Auto Create
    private ExButton m_StartGame;
    #endregion

    protected override void OnInit(object userData)
    {
        #region Auto Bind
        m_StartGame = PanelUIContainer.Get<ExButton>("m_StartGame");
        #endregion

        #region Auto Event
        m_StartGame.SetClickListener(StartGameClickListener);
        #endregion

        base.OnInit(userData);
    }

    #region Auto Write Event
    private void StartGameClickListener()
    {
        FightShareController.Instance.EnterFight(DungeonType.Main);
        //LoginController.Instance.ConnectServer();
    }
    
    #endregion
}