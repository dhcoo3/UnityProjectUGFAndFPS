using System;
using System.Security.Cryptography;
using System.Text;

namespace Builtin.Scripts.Extension
{
    /// <summary>
    /// 签名工具类（客户端）
    /// 与服务器使用相同的签名算法
    /// </summary>
    public static class SignatureHelper
    {
        /// <summary>
        /// 生成签名
        /// </summary>
        /// <param name="uin">用户ID</param>
        /// <param name="token">认证令牌</param>
        /// <param name="timestamp">时间戳（毫秒）</param>
        /// <param name="secretKey">密钥</param>
        /// <returns>Base64编码的签名</returns>
        public static string GenerateSign(uint uin, string token, long timestamp, string secretKey)
        {
            var data = $"{uin}|{token}|{timestamp}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// 获取当前时间戳（毫秒）
        /// </summary>
        public static long GetTimestamp()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}
