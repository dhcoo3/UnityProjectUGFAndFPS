using System.IO;
using System.Text;
using Cysharp.Text;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.GamePlay.Logic.Map
{
    /// <summary>
    /// 从二进制文件加载 TileMap 碰撞数据，构建 MapInfo
    /// 文件格式与 TilemapCollisionExporter 导出格式对应：
    ///   int width, int height, float gridSizeX, float gridSizeY, float originX, float originY
    ///   + width*height 组 (bool groundCanPass, bool flyCanPass)
    /// </summary>
    public static class MapCollisionLoader
    {
        /// <summary>
        /// 从 bytes 资源加载碰撞数据，返回 MapInfo
        /// </summary>
        /// <param name="asset">通过资源系统加载的 TextAsset（.bytes 文件）</param>
        /// <returns>构建好的 MapInfo，失败返回 null</returns>
        public static MapInfo LoadFromBytes(TextAsset asset)
        {
            if (asset == null)
            {
                Log.Error("[MapCollisionLoader] TextAsset 为空");
                return null;
            }
            return ParseBytes(asset.bytes);
        }

        /// <summary>
        /// 直接从绝对路径加载碰撞数据（Editor 调试用）
        /// </summary>
        /// <param name="filePath">文件绝对路径</param>
        /// <returns>构建好的 MapInfo，失败返回 null</returns>
        public static MapInfo LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Log.Error(ZString.Format("[MapCollisionLoader] 文件不存在：{0}", filePath));
                return null;
            }
            return ParseBytes(File.ReadAllBytes(filePath));
        }

        /// <summary>
        /// 解析二进制数据，构建 MapInfo
        /// </summary>
        public static MapInfo ParseBytes(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream, Encoding.UTF8))
            {
                int width     = reader.ReadInt32();
                int height    = reader.ReadInt32();
                float sizeX   = reader.ReadSingle();
                float sizeY   = reader.ReadSingle();
                // originX/Y 暂存，后续如需偏移地图原点可使用
                float originX = reader.ReadSingle();
                float originY = reader.ReadSingle();

                var grid = new GridInfo[width, height];
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        bool groundCanPass = reader.ReadBoolean();
                        bool flyCanPass    = reader.ReadBoolean();
                        grid[x, y] = new GridInfo(string.Empty, groundCanPass, flyCanPass);
                    }
                }

                Log.Info(ZString.Format("[MapCollisionLoader] 加载成功 {0}x{1} 格，格子大小 {2}x{3} 米", width, height, sizeX, sizeY));
                return new MapInfo(grid, new fix2(sizeX, sizeY));
            }
        }
    }
}
