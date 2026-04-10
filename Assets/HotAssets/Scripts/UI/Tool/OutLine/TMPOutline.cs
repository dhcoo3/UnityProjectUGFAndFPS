using TMPro;
using UnityEngine;

namespace HotAssets.Scripts.UI.Tool.OutLine
{
    /// <summary>
    /// TextMeshPro 描边和底纹控制器
    /// 使用材质缓存池复用相同参数的材质，降低 DrawCall
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TMPOutline : MonoBehaviour
    {
        [Header("描边设置")]
        
        [SerializeField]
        private bool enableOutline = false;

        [SerializeField]
        [Range(0f, 1f)]
        private float outlineWidth = 0f;

        [SerializeField]
        private Color outlineColor = Color.black;

        [Header("底纹设置")]
        [SerializeField]
        private bool enableUnderlay = false;

        [SerializeField]
        private Color underlayColor = new Color(0, 0, 0, 1f);

        [SerializeField]
        [Range(-1f, 1f)]
        private float underlayOffsetX = 0f;

        [SerializeField]
        [Range(-1f, 1f)]
        private float underlayOffsetY = 0f;

        [SerializeField]
        [Range(-1f, 1f)]
        private float underlayDilate = 0.3f;

        [SerializeField]
        [Range(0f, 1f)]
        private float underlaySoftness = 0f;

        private TextMeshProUGUI tmpText;
        private float lastOutlineWidth;
        private Color lastOutlineColor;
        private Color lastUnderlayColor;
        private float lastUnderlayOffsetX;
        private float lastUnderlayOffsetY;
        private float lastUnderlayDilate;
        private float lastUnderlaySoftness;

        private void Awake()
        {
            InitializeMaterial();
        }

        private void OnEnable()
        {
            InitializeMaterial();
            ApplyAllSettings();
        }

        private void OnValidate()
        {
            // 编辑器中参数变化时实时更新
            if (tmpText != null)
            {
                ApplyAllSettings();
            }
        }

        private void Update()
        {
            // 检测参数变化（编辑器模式下）
            if (!Application.isPlaying)
            {
                if (HasOutlineChanged() || HasUnderlayChanged())
                {
                    ApplyAllSettings();
                }
            }
        }

        /// <summary>
        /// 初始化组件
        /// </summary>
        private void InitializeMaterial()
        {
            if (tmpText == null)
            {
                tmpText = GetComponent<TextMeshProUGUI>();
            }
        }

        /// <summary>
        /// 应用所有设置
        /// </summary>
        private void ApplyAllSettings()
        {
            if (tmpText == null)
            {
                InitializeMaterial();
            }

            if (tmpText != null)
            {
                Material targetMat;

                if (Application.isPlaying)
                {
                    // 运行时：使用材质管理器缓存
                    targetMat = TMPMaterialManager.GetOrCreateMaterial(
                        tmpText,
                        enableOutline, outlineWidth, outlineColor,
                        enableUnderlay, underlayColor, underlayOffsetX, underlayOffsetY,
                        underlayDilate, underlaySoftness);

                    tmpText.fontSharedMaterial = targetMat;
                }
                else
                {
                    // 编辑器模式：直接修改材质实例
                    targetMat = tmpText.fontMaterial;

                    if (enableOutline)
                    {
                        targetMat.EnableKeyword("OUTLINE_ON");
                        targetMat.SetFloat(ShaderUtilities.ID_OutlineWidth, outlineWidth);
                        targetMat.SetColor(ShaderUtilities.ID_OutlineColor, outlineColor);
                    }
                    else
                    {
                        targetMat.DisableKeyword("OUTLINE_ON");
                        targetMat.SetFloat(ShaderUtilities.ID_OutlineWidth, 0f);
                    }

                    if (enableUnderlay)
                    {
                        targetMat.EnableKeyword("UNDERLAY_ON");
                        targetMat.SetColor(ShaderUtilities.ID_UnderlayColor, underlayColor);
                        targetMat.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, underlayOffsetX);
                        targetMat.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, underlayOffsetY);
                        targetMat.SetFloat(ShaderUtilities.ID_UnderlayDilate, underlayDilate);
                        targetMat.SetFloat(ShaderUtilities.ID_UnderlaySoftness, underlaySoftness);
                    }
                    else
                    {
                        targetMat.DisableKeyword("UNDERLAY_ON");
                    }
                }

                // 更新缓存值
                lastOutlineWidth = outlineWidth;
                lastOutlineColor = outlineColor;
                lastUnderlayColor = underlayColor;
                lastUnderlayOffsetX = underlayOffsetX;
                lastUnderlayOffsetY = underlayOffsetY;
                lastUnderlayDilate = underlayDilate;
                lastUnderlaySoftness = underlaySoftness;

                // 强制刷新
                tmpText.SetMaterialDirty();
            }
        }

        /// <summary>
        /// 检测描边参数是否变化
        /// </summary>
        private bool HasOutlineChanged()
        {
            return !Mathf.Approximately(lastOutlineWidth, outlineWidth) || lastOutlineColor != outlineColor;
        }

        /// <summary>
        /// 检测底纹参数是否变化
        /// </summary>
        private bool HasUnderlayChanged()
        {
            return lastUnderlayColor != underlayColor ||
                   !Mathf.Approximately(lastUnderlayOffsetX, underlayOffsetX) ||
                   !Mathf.Approximately(lastUnderlayOffsetY, underlayOffsetY) ||
                   !Mathf.Approximately(lastUnderlayDilate, underlayDilate) ||
                   !Mathf.Approximately(lastUnderlaySoftness, underlaySoftness);
        }

        /// <summary>
        /// 设置描边宽度
        /// </summary>
        public void SetOutlineWidth(float width)
        {
            outlineWidth = Mathf.Clamp01(width);
            ApplyAllSettings();
        }

        /// <summary>
        /// 设置描边颜色
        /// </summary>
        public void SetOutlineColor(Color color)
        {
            outlineColor = color;
            ApplyAllSettings();
        }

        /// <summary>
        /// 设置底纹颜色
        /// </summary>
        public void SetUnderlayColor(Color color)
        {
            underlayColor = color;
            ApplyAllSettings();
        }

        /// <summary>
        /// 设置底纹偏移
        /// </summary>
        public void SetUnderlayOffset(float offsetX, float offsetY)
        {
            underlayOffsetX = Mathf.Clamp(offsetX, -1f, 1f);
            underlayOffsetY = Mathf.Clamp(offsetY, -1f, 1f);
            ApplyAllSettings();
        }

        /// <summary>
        /// 设置底纹扩散和柔软度
        /// </summary>
        public void SetUnderlayDilateSoftness(float dilate, float softness)
        {
            underlayDilate = Mathf.Clamp(dilate, -1f, 1f);
            underlaySoftness = Mathf.Clamp01(softness);
            ApplyAllSettings();
        }
    }
}