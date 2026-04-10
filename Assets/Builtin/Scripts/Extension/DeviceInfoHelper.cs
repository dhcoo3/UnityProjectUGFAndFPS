using UnityEngine;

namespace Builtin.Scripts.Extension
{
    /// <summary>
    /// 设备信息工具类
    /// 用于收集设备相关信息
    /// </summary>
    public static class DeviceInfoHelper
    {
        /// <summary>
        /// 获取设备唯一标识符
        /// </summary>
        public static string GetDeviceId()
        {
            return SystemInfo.deviceUniqueIdentifier;
        }

        /// <summary>
        /// 获取操作系统版本
        /// </summary>
        public static string GetOSVersion()
        {
            return SystemInfo.operatingSystem;
        }

        /// <summary>
        /// 获取应用版本
        /// </summary>
        public static string GetAppVersion()
        {
            return Application.version;
        }

        /// <summary>
        /// 获取设备型号
        /// </summary>
        public static string GetDeviceModel()
        {
            return SystemInfo.deviceModel;
        }

        /// <summary>
        /// 获取设备名称
        /// </summary>
        public static string GetDeviceName()
        {
            return SystemInfo.deviceName;
        }
    }
}
