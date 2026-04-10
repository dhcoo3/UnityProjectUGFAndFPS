using HotAssets.Scripts.GamePlay.Logic.Common;
using HotAssets.Scripts.GamePlay.Logic.ProxyManager;
using UnityEngine;

namespace HotAssets.Scripts.GamePlay.Logic.GameInput
{
    /// <summary>
    /// 在逻辑帧之前采集原始 Unity 输入，避免 GetKeyDown ��瞬时状态因执行顺序延迟一帧或丢失
    /// 新增瞬时动作（闪避、冲刺等）统一在此处读取，由 InputProxy 转发给逻辑层
    /// </summary>
    public class InputCollector
    {
        private InputProxy _inputProxy;
        private float _lastHorizontal;
        private float _lastVertical;
        private GamePlayDefine.EAimDirection _lastAimDir = GamePlayDefine.EAimDirection.Right;

        public void Initialize()
        {
            _inputProxy = GameProxyManger.Instance.GetProxy<InputProxy>();
        }

        /// <summary>
        /// 每帧逻辑开始前调用，采集本帧所有输入并转发给 InputProxy
        /// </summary>
        public void Collect()
        {
            if (Application.platform != RuntimePlatform.WindowsEditor) return;

            float ix = Input.GetAxisRaw("Horizontal");
            float iz = Input.GetAxisRaw("Vertical");
            bool jump = Input.GetKeyDown(KeyCode.Space);
            bool dash = Input.GetKeyDown(KeyCode.LeftShift);
            // 有方向输入时更新瞄准方向，无输入时保留上次方向，避免松键那一帧重置为 Right
            if (ix != 0 || iz != 0)
            {
                _lastAimDir = CalcAimDirection(ix, iz);
            }

            GamePlayDefine.EAimDirection aimDir = _lastAimDir;

            // 水平/垂直输入或跳跃/冲刺有变化时才发送，避免每帧无效冗余输入
            if (ix != 0 || iz != 0 || jump || dash || _lastHorizontal != 0 || _lastVertical != 0)
            {
                _inputProxy.SendInput(ix, jump, dash, aimDir);
            }
            
            _lastHorizontal = ix;
            _lastVertical = iz;
          
            if (Input.GetKey(KeyCode.K))
            {
                _inputProxy.SendInput(KeyCode.K, aimDir);
            }
            else if (Input.GetKey(KeyCode.L))
            {
                _inputProxy.SendInput(KeyCode.L, aimDir);
            }
            else if (Input.GetKey(KeyCode.J))
            {
                _inputProxy.SendInput(KeyCode.J, aimDir);
            }
        }

        public void Clear()
        {
            _inputProxy = null;
            _lastHorizontal = 0;
            _lastVertical = 0;
            _lastAimDir = GamePlayDefine.EAimDirection.Right;
        }

        private GamePlayDefine.EAimDirection CalcAimDirection(float h, float v)
        {
            if (h > 0.3f  && v >  0.3f) return GamePlayDefine.EAimDirection.RightUp;
            if (h > 0.3f  && v < -0.3f) return GamePlayDefine.EAimDirection.RightDown;
            if (h < -0.3f && v >  0.3f) return GamePlayDefine.EAimDirection.LeftUp;
            if (h < -0.3f && v < -0.3f) return GamePlayDefine.EAimDirection.LeftDown;
            if (h > 0.3f)               return GamePlayDefine.EAimDirection.Right;
            if (h < -0.3f)              return GamePlayDefine.EAimDirection.Left;
            if (v >  0.3f)              return GamePlayDefine.EAimDirection.Up;
            if (v < -0.3f)              return GamePlayDefine.EAimDirection.Down;
            return GamePlayDefine.EAimDirection.Right;
        }
    }
}