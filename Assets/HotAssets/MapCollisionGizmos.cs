using System.Collections;
using System.Collections.Generic;
using HotAssets.Scripts.GamePlay.Logic.Map;
using HotAssets.Scripts.GamePlay.Logic.ProxyManager;
using UnityEngine;

/// <summary>
/// 编辑器 Gizmos 可视化碰撞格子，挂到场景任意 GameObject 运行时查看
/// 绿色=可通行 红色=阻挡
/// </summary>
public class MapCollisionGizmos : MonoBehaviour
{
    [Header("格子透明度")]
    [Range(0f, 1f)] public float alpha = 0.3f;

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        var proxy = GameProxyManger.Instance?.GetProxy<MapProxy>();
        MapInfo info = proxy?.MapInfo;
        if (info == null) return;

        float sx = (float)info.gridSize.x;
        float sy = (float)info.gridSize.y;

        for (int x = 0; x < info.MapWidth(); x++)
        for (int y = 0; y < info.MapHeight(); y++)
        {
            bool pass = info.grid[x, y].groundCanPass;
            Gizmos.color = pass
                ? new Color(0, 1, 0, alpha)
                : new Color(1, 0, 0, alpha);

            Vector3 center = new Vector3(x * sx, y * sy, 0);
            Gizmos.DrawCube(center, new Vector3(sx * 0.9f, sy * 0.9f, 0.05f));
        }
    }
}