using HotAssets.Scripts.Common.Event;
using HotAssets.Scripts.GamePlay.Logic.Common;
using HotAssets.Scripts.GamePlay.Render.Camera;
using HotAssets.Scripts.GamePlay.Render.RenderManager;
using UnityEngine;

namespace HotAssets.Scripts.GamePlay.Render.Joystick
{
    public class JoystickRender:GameRender
    {
        public const string AssetPath = "Assets/AAAGame/Prefabs/UI/Joystick/Joystick.prefab";
        private RectTransform _root;
        private RectTransform _pointer;
        private RectTransform _handle;
        private CameraRender _camera;

        public override void Initialize()
        {
            Subscribe(GamePlayEvent.EFightLoadingFinish,FightLoadingFinish);
        }

        public override void Clear()
        {
            base.Clear();
        }

        public void LoadJoystick()
        {
            //GF.AssetBridge.LoadAsset(AssetPath, LoadCall,typeof(GameObject));
        }

        public void FightLoadingFinish(object sender, GameEvent e)
        {
            LoadJoystick();
        }
    }
}