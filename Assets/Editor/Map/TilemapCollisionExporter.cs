using System.IO;
using System.Text;
using Cysharp.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Editor.Map
{
    /// <summary>
    /// TileMap 碰撞数据导出工具
    /// 使用场景中两个 Tilemap 层（地面碰撞层 / 飞行碰撞层）导出 GridInfo 二进制数据
    /// 导出格式：文件头(width, height, gridSizeX, gridSizeY) + 每格(groundCanPass, flyCanPass)
    /// </summary>
    public class TilemapCollisionExporter : EditorWindow
    {
        // 场景中地面碰撞层 TileMap 的 GameObject 名称（有 Tile = 不可通行）
        private string _groundBlockLayerName = "CollisionGround";
        // 场景中飞行碰撞层 TileMap 的 GameObject 名称（有 Tile = 不可通行）
        private string _flyBlockLayerName = "CollisionFly";
        // 每格大小（米），与 MapInfo 的 gridSize 对应
        private float _gridSizeX = 1f;
        private float _gridSizeY = 1f;
        // 地图 ID，用于命名导出文件
        private string _mapId = "Map_001";
        // 导出目录（相对于 Assets）
        private const string ExportDir = "Assets/HotAssets/Scene/SceneAsset";
        
        [MenuItem("Tools/地图/导出TileMap碰撞数据")]
        public static void ShowWindow()
        {
            GetWindow<TilemapCollisionExporter>("导出TileMap碰撞");
        }

        private void OnGUI()
        {
            GUILayout.Label("TileMap 碰撞导出设置", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            _mapId = EditorGUILayout.TextField("地图ID（文件名）", _mapId);
            _groundBlockLayerName = EditorGUILayout.TextField("地面阻挡层名称", _groundBlockLayerName);
            _flyBlockLayerName = EditorGUILayout.TextField("飞行阻挡层名称（可空）", _flyBlockLayerName);
            _gridSizeX = EditorGUILayout.FloatField("格子宽度（米）", _gridSizeX);
            _gridSizeY = EditorGUILayout.FloatField("格子高度（米）", _gridSizeY);

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                ZString.Format("导出路径：{0}/{1}.bytes", ExportDir, _mapId),
                MessageType.Info
            );

            EditorGUILayout.Space();
            if (GUILayout.Button("导出碰撞数据", GUILayout.Height(40)))
            {
                Export();
            }
        }

        /// <summary>
        /// 执行导出：读取 TileMap 格子，序列化为二进制文件
        /// </summary>
        private void Export()
        {
            // 查找地面碰撞层
            var groundObj = GameObject.Find(_groundBlockLayerName);
            if (groundObj == null)
            {
                EditorUtility.DisplayDialog("错误", ZString.Format("找不到地面阻挡层：{0}", _groundBlockLayerName), "确定");
                return;
            }
            var groundTilemap = groundObj.GetComponent<Tilemap>();
            if (groundTilemap == null)
            {
                EditorUtility.DisplayDialog("错误", ZString.Format("{0} 没有 Tilemap 组件", _groundBlockLayerName), "确定");
                return;
            }

            // 查找飞行碰撞层（可选，若不存在则飞行层跟随地面层）
            Tilemap flyTilemap = null;
            if (!string.IsNullOrEmpty(_flyBlockLayerName))
            {
                var flyObj = GameObject.Find(_flyBlockLayerName);
                if (flyObj != null)
                    flyTilemap = flyObj.GetComponent<Tilemap>();
            }

            // 压缩 TileMap 到实际使用区域
            groundTilemap.CompressBounds();
            flyTilemap?.CompressBounds();

            // 取地面层 bounds 作为地图范围基准（飞行层用同一范围）
            BoundsInt bounds = groundTilemap.cellBounds;
            int width = bounds.size.x;
            int height = bounds.size.y;

            if (width <= 0 || height <= 0)
            {
                EditorUtility.DisplayDialog("错误", "地面阻挡层没有任何 Tile，请先刷好碰撞格子", "确定");
                return;
            }

            // 生成 GridData 数组（groundCanPass / flyCanPass）
            // 有 Tile = 阻挡（canPass = false），无 Tile = 可通行（canPass = true）
            bool[,] groundPass = new bool[width, height];
            bool[,] flyPass = new bool[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var cellPos = new Vector3Int(bounds.xMin + x, bounds.yMin + y, 0);
                    groundPass[x, y] = !groundTilemap.HasTile(cellPos);
                    flyPass[x, y] = flyTilemap != null
                        ? !flyTilemap.HasTile(cellPos)
                        : groundPass[x, y];
                }
            }

            // 序列化写入二进制文件
            string exportPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ExportDir));
            Directory.CreateDirectory(exportPath);

            string filePath = Path.Combine(exportPath, ZString.Format("{0}.bytes", _mapId));
            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            using (var writer = new BinaryWriter(stream, Encoding.UTF8))
            {
                // 文件头
                writer.Write(width);
                writer.Write(height);
                writer.Write(_gridSizeX);
                writer.Write(_gridSizeY);
                // 地图原点偏移（TileMap cellBounds 左下角在世界坐标中的位置）
                Vector3 originWorld = groundTilemap.CellToWorld(new Vector3Int(bounds.xMin, bounds.yMin, 0));
                writer.Write(originWorld.x);
                writer.Write(originWorld.y);

                // 格子数据
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        writer.Write(groundPass[x, y]);
                        writer.Write(flyPass[x, y]);
                    }
                }
            }

            AssetDatabase.Refresh();
            UnityGameFramework.Runtime.Log.Info(
                ZString.Format("[TilemapCollisionExporter] 导出成功 -> {0}  ({1}x{2}格)", filePath, width, height)
            );
            EditorUtility.DisplayDialog("导出成功",
                ZString.Format("{0}.bytes\n尺寸：{1} x {2} 格\n格子大小：{3}x{4} 米", _mapId, width, height, _gridSizeX, _gridSizeY),
                "确定");
        }
    }
}
