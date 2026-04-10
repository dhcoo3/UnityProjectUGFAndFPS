using GameFramework;
using HotAssets.Scripts.GamePlay.Logic.Common;
using UnityEngine;

namespace HotAssets.Scripts.GamePlay.Logic.GameInput
{
    public class InputObj:IReference
    {
        public fix Horizontal;
        public bool Jump;
        /// <summary>冲刺指令（按下 LeftShift 时为 true）</summary>
        public bool Dash;
        public GamePlayDefine.EAimDirection AimDir;
        public string PlayerId;
        public KeyCode Key = KeyCode.None;

        public static InputObj Create(fix horizontal, bool jump, bool dash, GamePlayDefine.EAimDirection aimDir)
        {
            InputObj inputObj = ReferencePool.Acquire<InputObj>();
            inputObj.Horizontal = horizontal;
            inputObj.Jump = jump;
            inputObj.Dash = dash;
            inputObj.AimDir = aimDir;
            return inputObj;
        }

        public static InputObj Create(fix horizontal, bool jump, GamePlayDefine.EAimDirection aimDir)
        {
            return Create(horizontal, jump, false, aimDir);
        }

        public static InputObj Create(KeyCode key, GamePlayDefine.EAimDirection aimDir = GamePlayDefine.EAimDirection.Right)
        {
            InputObj inputObj = ReferencePool.Acquire<InputObj>();
            inputObj.Key = key;
            inputObj.AimDir = aimDir;
            return inputObj;
        }

        public string Pack()
        {
            return $"{Horizontal}|{(Jump ? 1 : 0)}|{(int)AimDir}|";
        }

        public void Clear()
        {
            Horizontal = fix.Zero;
            Jump = false;
            Dash = false;
            AimDir = GamePlayDefine.EAimDirection.Right;
            PlayerId = string.Empty;
            Key = KeyCode.None;
        }
    }
}