using GameProto;
using Google.Protobuf;
using HotAssets.Scripts.UI.Core;
using UnityGameFramework.Runtime;
using Builtin.Scripts.Extension;
using Cysharp.Threading.Tasks;

namespace HotAssets.Scripts.UI.Module.Login
{
    /// <summary>
    /// 登录状态枚举
    /// </summary>
    public enum LoginState
    {
        Idle,           // 空闲
        Connecting,     // 连接中
        LoginSending,   // 登录请求发送中
        LoginSuccess,   // 登录成功
        LoginFailed     // 登录失败
    }

    public class LoginController: ModuleController<LoginController>, IController
    {
        private const string SECRET_KEY = "YourSecretKey_ChangeThisInProduction_32Chars!"; // 与服务器保持一致
        private const int LOGIN_TIMEOUT_MS = 10000; // 10秒超时

        private LoginState _currentState = LoginState.Idle;
        private System.Threading.CancellationTokenSource _timeoutCts;

        public override void RegisterProto()
        {
           // GameExtension.GameNetwork.RegisterProto(CsRoleLoginRes.Descriptor, CsMsgType.MsgRoleLoginRes, MsgRoleLoginRes);
           // GameExtension.GameNetwork.RegisterProto(CsKickOutNotify.Descriptor, CsMsgType.MsgKickOut, MsgKickOut);
        }

        public override void RegisterEvent()
        {

        }

        /// <summary>
        /// 连接服务器并登录
        /// </summary>
        public void ConnectServer()
        {
            if (_currentState == LoginState.Connecting || _currentState == LoginState.LoginSending)
            {
                Log.Warning("Login already in progress, state: {0}", _currentState);
                return;
            }

            _currentState = LoginState.Connecting;
            Log.Info("Connecting to server...");

            GameExtension.GameNetwork.SetConnectCallBack(ConnectSucc);
            string serverIp = "127.0.0.1";
            int port = 12345;
            GameExtension.GameNetwork.Connect("Game", serverIp, port);
        }

        /// <summary>
        /// 连接成功回调
        /// </summary>
        private void ConnectSucc(string channelName)
        {
            Log.Info("Connected to server successfully, channel: {0}", channelName);
            UIModuleManager.Instance.InitGameModule();
            LoginReq();
        }

        /// <summary>
        /// 发送登录请求
        /// </summary>
        private void LoginReq()
        {
            _currentState = LoginState.LoginSending;

            // 获取设备信息
            var deviceId = DeviceInfoHelper.GetDeviceId();
            var osVersion = DeviceInfoHelper.GetOSVersion();
            var appVersion = DeviceInfoHelper.GetAppVersion();

            // 生成时间戳和签名
            var timestamp = SignatureHelper.GetTimestamp();
            var uin = 10000u;
            var token = "32324324324";
            var sign = SignatureHelper.GenerateSign(uin, token, timestamp, SECRET_KEY);

            // 构建登录请求
            CsRoleLoginReq data = new CsRoleLoginReq
            {
                Uin = uin,
                Token = token,
                IoRTSVer = 12323,
                AliChannel = "client",
                Timestamp = timestamp,
                Sign = sign,
                DeviceId = deviceId,
                OsVersion = osVersion,
                AppVersion = appVersion
            };

            Log.Info("Sending login request, UIN: {0}, Timestamp: {1}, DeviceId: {2}", uin, timestamp, deviceId);
            GameExtension.GameNetwork.Send((int)CsMsgType.MsgRoleLoginReq, data);

            // 启动超时检测
            StartLoginTimeout().Forget();
        }

        /// <summary>
        /// 登录超时检测
        /// </summary>
        private async UniTaskVoid StartLoginTimeout()
        {
            _timeoutCts?.Cancel();
            _timeoutCts = new System.Threading.CancellationTokenSource();

            try
            {
                await UniTask.Delay(LOGIN_TIMEOUT_MS, cancellationToken: _timeoutCts.Token);

                // 超时未收到响应
                if (_currentState == LoginState.LoginSending)
                {
                    _currentState = LoginState.LoginFailed;
                    Log.Error("Login timeout after {0}ms", LOGIN_TIMEOUT_MS);
                    // 这里可以触发超时事件，通知UI显示错误
                }
            }
            catch (System.OperationCanceledException)
            {
                // 正常取消，忽略
            }
        }

        /// <summary>
        /// 登录响应处理
        /// </summary>
        private void MsgRoleLoginRes(IMessage msg)
        {
            // 取消超时检测
            _timeoutCts?.Cancel();

            var csRoleLoginRes = (CsRoleLoginRes)msg;

            Log.Info("Login response received, Code: {0}, Message: {1}, IsNew: {2}",
                csRoleLoginRes.Succ, csRoleLoginRes.ErrorMsg, csRoleLoginRes.IsNew);

            if (csRoleLoginRes.Succ == CsErrorCode.ErrCodeSucc)
            {
                _currentState = LoginState.LoginSuccess;
                LoginModel.Instance.LoginData = csRoleLoginRes;
                Fire(LoginConst.Event.LoginSucc);
                Log.Info("Login successful!");
            }
            else
            {
                _currentState = LoginState.LoginFailed;
                HandleLoginError(csRoleLoginRes);
            }
        }

        /// <summary>
        /// 处理登录错误
        /// </summary>
        private void HandleLoginError(CsRoleLoginRes response)
        {
            string errorMsg = response.ErrorMsg;

            switch (response.Succ)
            {
                case CsErrorCode.ErrCodeInvalidParam:
                    Log.Error("Login failed: Invalid parameters");
                    break;
                case CsErrorCode.ErrCodeInvalidToken:
                    Log.Error("Login failed: Invalid token");
                    break;
                case CsErrorCode.ErrCodeTokenExpired:
                    Log.Error("Login failed: Token expired");
                    break;
                case CsErrorCode.ErrCodeAccountBanned:
                    Log.Error("Login failed: Account is banned");
                    break;
                case CsErrorCode.ErrCodeTimestampInvalid:
                    Log.Error("Login failed: Timestamp validation failed");
                    break;
                case CsErrorCode.ErrCodeSignInvalid:
                    Log.Error("Login failed: Signature verification failed");
                    break;
                case CsErrorCode.ErrCodeTooManyFails:
                    Log.Error("Login failed: Too many failed attempts, retry after {0}s", response.RetryAfterSeconds);
                    break;
                case CsErrorCode.ErrCodeServerError:
                    Log.Error("Login failed: Server error");
                    break;
                default:
                    Log.Error("Login failed: Unknown error, Code: {0}", response.Succ);
                    break;
            }

            // 这里可以触发错误事件，通知UI显示具体错误信息
        }

        /// <summary>
        /// 踢出通知处理
        /// </summary>
        private void MsgKickOut(IMessage msg)
        {
            var kickNotify = (CsKickOutNotify)msg;
            Log.Warning("Kicked out from server, Reason: {0}, Message: {1}", kickNotify.Reason, kickNotify.Message);

            _currentState = LoginState.Idle;

            // 这里可以触发踢出事件，通知UI显示提示并返回登录界面
        }

        /// <summary>
        /// 获取当前登录状态
        /// </summary>
        public LoginState GetCurrentState()
        {
            return _currentState;
        }
    }
}