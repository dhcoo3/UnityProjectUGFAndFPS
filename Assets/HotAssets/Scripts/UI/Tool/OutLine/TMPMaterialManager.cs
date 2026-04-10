using System.Collections.Generic;
using Cysharp.Text;
using TMPro;
using UnityEngine;

namespace HotAssets.Scripts.UI.Tool.OutLine
{
    /// <summary>
    /// TextMeshPro 材质缓存管理器
    /// 通过材质池复用相同参数的材质，降低 DrawCall
    /// </summary>
    public class TMPMaterialManager : MonoBehaviour
    {
        private static TMPMaterialManager instance;
        private Dictionary<string, Material> materialCache = new Dictionary<string, Material>();

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 获取或创建材质（带描边和底纹）
        /// </summary>
        public static Material GetOrCreateMaterial(
            TextMeshProUGUI text,
            bool enableOutline,
            float outlineWidth,
            Color outlineColor,
            bool enableUnderlay,
            Color underlayColor,
            float underlayOffsetX,
            float underlayOffsetY,
            float underlayDilate,
            float underlaySoftness)
        {
            if (instance == null)
            {
                GameObject go = new GameObject("TMPMaterialManager");
                instance = go.AddComponent<TMPMaterialManager>();
            }

            // 生成唯一键
            string key = GenerateMaterialKey(
                enableOutline, outlineWidth, outlineColor,
                enableUnderlay, underlayColor, underlayOffsetX, underlayOffsetY,
                underlayDilate, underlaySoftness);

            // 尝试从缓存获取
            if (instance.materialCache.TryGetValue(key, out Material cachedMat))
            {
                return cachedMat;
            }

            // 创建新材质
            Material newMat = new Material(text.fontSharedMaterial);
            newMat.name = $"TMP_Material_{key}";

            // 设置描边
            if (enableOutline)
            {
                newMat.EnableKeyword("OUTLINE_ON");
                newMat.SetFloat(ShaderUtilities.ID_OutlineWidth, outlineWidth);
                newMat.SetColor(ShaderUtilities.ID_OutlineColor, outlineColor);
            }
            else
            {
                newMat.DisableKeyword("OUTLINE_ON");
                newMat.SetFloat(ShaderUtilities.ID_OutlineWidth, 0f);
            }

            // 设置底纹
            if (enableUnderlay)
            {
                newMat.EnableKeyword("UNDERLAY_ON");
                newMat.SetColor(ShaderUtilities.ID_UnderlayColor, underlayColor);
                newMat.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, underlayOffsetX);
                newMat.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, underlayOffsetY);
                newMat.SetFloat(ShaderUtilities.ID_UnderlayDilate, underlayDilate);
                newMat.SetFloat(ShaderUtilities.ID_UnderlaySoftness, underlaySoftness);
            }
            else
            {
                newMat.DisableKeyword("UNDERLAY_ON");
            }

            // 缓存材质
            instance.materialCache[key] = newMat;

            return newMat;
        }

        /// <summary>
        /// 生成材质唯一键
        /// </summary>
        private static string GenerateMaterialKey(
            bool enableOutline, float outlineWidth, Color outlineColor,
            bool enableUnderlay, Color underlayColor, float underlayOffsetX, float underlayOffsetY,
            float underlayDilate, float underlaySoftness)
        {
            using (var sb = ZString.CreateStringBuilder())
            {
                sb.Append(enableOutline ? '1' : '0');
                sb.Append('_');
                sb.Append(outlineWidth.ToString("F2"));
                sb.Append('_');
                sb.Append(ColorToString(outlineColor));
                sb.Append('_');
                sb.Append(enableUnderlay ? '1' : '0');
                sb.Append('_');
                sb.Append(ColorToString(underlayColor));
                sb.Append('_');
                sb.Append(underlayOffsetX.ToString("F2"));
                sb.Append('_');
                sb.Append(underlayOffsetY.ToString("F2"));
                sb.Append('_');
                sb.Append(underlayDilate.ToString("F2"));
                sb.Append('_');
                sb.Append(underlaySoftness.ToString("F2"));
                return sb.ToString();
            }
        }

        /// <summary>
        /// 颜色转字符串
        /// </summary>
        private static string ColorToString(Color color)
        {
            return $"{(int)(color.r * 255)}{(int)(color.g * 255)}{(int)(color.b * 255)}{(int)(color.a * 255)}";
        }

        /// <summary>
        /// 清理未使用的材质
        /// </summary>
        public static void ClearUnusedMaterials()
        {
            if (instance == null) return;

            List<string> keysToRemove = new List<string>();

            foreach (var kvp in instance.materialCache)
            {
                if (kvp.Value == null)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                instance.materialCache.Remove(key);
            }
        }

        /// <summary>
        /// 获取缓存材质数量
        /// </summary>
        public static int GetCachedMaterialCount()
        {
            return instance != null ? instance.materialCache.Count : 0;
        }

        private void OnDestroy()
        {
            // 清理所有缓存材质
            if (instance == this)
            {
                foreach (var mat in materialCache.Values)
                {
                    if (mat != null)
                    {
                        Destroy(mat);
                    }
                }
                materialCache.Clear();
            }
        }
    }
}