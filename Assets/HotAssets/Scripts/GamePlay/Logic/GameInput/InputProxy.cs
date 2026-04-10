using System.Collections.Generic;
using GameFramework;
using HotAssets.Scripts.GamePlay.Logic.Common;
using HotAssets.Scripts.GamePlay.Logic.Frame;
using HotAssets.Scripts.GamePlay.Logic.ProxyManager;
using HotAssets.Scripts.GamePlay.Logic.Unit;
using HotAssets.Scripts.UI;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.GamePlay.Logic.GameInput
{
    public class InputProxy:GameProxy
    {
        List<InputObj> _inputObjs;
        UnitProxy _unitProxy;
        FrameProxy _frameProxy;
        
        public override void Initialize()
        {
            _inputObjs = new List<InputObj>();
            _unitProxy = GetProxy<UnitProxy>();
            _frameProxy = GetProxy<FrameProxy>();
            ReferencePool.Add<InputObj>(10);
        }

        public override void LogicUpdate(fix deltaTime)
        {
            if(_inputObjs.Count == 0) return;
            
            for (int i = 0; i < _inputObjs.Count; i++)
            {
                InputObj inputObj = _inputObjs[i];
                _unitProxy.OperatorMainRole(inputObj);
                ReferencePool.Release(inputObj);
            }
         
            _inputObjs.Clear();
        }

        public void SendInput(float horizontal, bool jump, GamePlayDefine.EAimDirection aimDir)
        {
            SendInput(horizontal, jump, false, aimDir);
        }

        /// <summary>
        /// 发送移动输入，含冲刺指令
        /// </summary>
        public void SendInput(float horizontal, bool jump, bool dash, GamePlayDefine.EAimDirection aimDir)
        {
            InputObj inputObj = InputObj.Create(horizontal, jump, dash, aimDir);

            if (_frameProxy.IsLocal)
            {
                inputObj.PlayerId = GameExtension.UserId;
                AddInputObj(inputObj);
            }
            else
            {
                //GF.Room.SendInput(inputObj);
                ReferencePool.Release(inputObj);
            }

            //Log.Info("SendInput:"+Time.time);
        }
        
        public void SendInput(KeyCode key, GamePlayDefine.EAimDirection aimDir = GamePlayDefine.EAimDirection.Right)
        {
            InputObj inputObj = InputObj.Create(key, aimDir);
            if (_frameProxy.IsLocal)
            {
                inputObj.PlayerId = GameExtension.UserId;
                AddInputObj(inputObj);
            }
            else
            {
                //GF.Room.SendInput(inputObj);
                ReferencePool.Release(inputObj);
            }
            
            //Log.Info("SendInput:"+Time.time);
        }

        public void AddInputObj(InputObj inputObj)
        {
            //Log.Info($"InputProxy LogicUpdate: {inputObj.PlayerId} {inputObj.Horizontal} Jump:{inputObj.Jump} AimDir:{inputObj.AimDir}");
            _inputObjs.Add(inputObj);
        }
    }
}