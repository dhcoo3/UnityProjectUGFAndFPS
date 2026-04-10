using System;
using System.Collections;
using System.Threading;
using Builtin.Scripts.Game;
using Cysharp.Threading.Tasks;
using GameFramework;
using HotAssets.Scripts.Extension;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace HotAssets.Scripts.UI.Tool.Component
{
    public class ExImage : Image
    {
        private Material ModifiedMat { get; set; }

        /// <summary>
        /// 是否为动态加载图片
        /// </summary>
        [HideInInspector] public bool DynamicLoad;

        /// <summary>
        /// 动态加载路径,不填则在代码中处理
        /// </summary>
        [HideInInspector] public string DynamicLoadPath = "";

        /// <summary>
        /// 动态加载后的透明度设置
        /// </summary>
        [HideInInspector] public float DynamicLoadAlphaVal = 1;

        /// <summary>
        /// 缓存原材质
        /// </summary>
        private Material _cacheOriginalMaterial;

        private Material _loadMaterial;

        /// <summary>
        /// 缓存加载的图片名
        /// </summary>
        private string _loadSpriteAssetName;

        /// <summary>
        /// 缓存加载的材质名
        /// </summary>
        private string _loadMaterialAssetName;

        private WaitForEndOfFrame _waitForEndOfFrame;

        /// <summary>
        /// 透明度颜色，用于做透明度渐变使用,避免频繁new color
        /// </summary>
        private Color _alphaColor = Color.white;

       private CancellationTokenSource _loadSpriteToken;
        private CancellationTokenSource _loadMaterialToken;
        private CancellationTokenSource _downloadImageToken;

        protected override void Awake()
        {
            StartDynamicLoadImage();

#if UNITY_EDITOR
            CheckImageSize();
#endif

            base.Awake();
        }

#if UNITY_EDITOR
        /// <summary>
        /// 检查图片尺寸，当尺寸宽高大于512*512时，使用动态加载，不要直接绑定，并勾选NeedLoad,加快UI面板的加载速度
        /// </summary>
        private void CheckImageSize()
        {
            if (!Application.isPlaying && sprite != null)
            {
                if (sprite.rect is { height: >= 512, width: >= 512 }
                    && DynamicLoad == false)
                {
                    string path = AssetDatabase.GetAssetPath(sprite);
                    if (!path.Contains("Assets/Res")) return;
                    Debug.LogWarning("请优化该图片,清除图片引用,从代码动态加载,并勾选NeedLoad。Name=" + gameObject.name + " " + path,
                        gameObject);
                }
            }
        }
#endif

        protected override void Start()
        {
            if (material)
            {
                _cacheOriginalMaterial = material;
            }

            base.Start();
        }

        protected override void OnDestroy()
        {
            CancelInvoke();
            _loadSpriteToken?.Cancel();
            _loadMaterialToken?.Cancel(); 
            _downloadImageToken?.Cancel();
            
            if (!string.IsNullOrEmpty(_loadSpriteAssetName))
            {
                AppEntry.Resource.UnloadAsset(_loadSpriteAssetName);
            }
            
            ClearLoadMaterial();

            _loadSpriteToken = null;
            _loadMaterialToken = null;
            _downloadImageToken = null;
            _loadSpriteAssetName = string.Empty;
            base.OnDestroy();
        }


        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            switch (type)
            {
                case Type.Filled when m_SlicedClipMode &&
                                      (fillMethod == FillMethod.Horizontal || fillMethod == FillMethod.Vertical) &&
                                      hasBorder:
                    GenerateSlicedSprite(toFill);
                    break;
                default:
                    base.OnPopulateMesh(toFill);
                    break;
            }
        }

        public string GetCacheTextureAssetName()
        {
            if (_loadSpriteAssetName == null) return "";
            return _loadSpriteAssetName;
        }

        /// <summary>
        /// 设置图片，通过路径来加载AB包的图片
        /// 使用场景：当图片为单张图（不在图集中）,且需要此图做单独的材质处理时使用此接口，Sprite.Create生成的为独立个体
        /// PS:如果无需使用九宫格、拉伸、填充等操作，为减少开销可使用UAppEntry_RawImage加载图片,不需要调用Sprite.Create
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="callback">回调</param>
        /// <param name="loadIsSynNativeSize">加载后是否自适应大小</param>
        public async UniTask SetImageCs(string assetPath, GameFrameworkAction callback = null,
            bool loadIsSynNativeSize = false)
        {
            SetAlpha(0);

            if (_loadSpriteToken != null)
            {
                _loadSpriteToken.Cancel();
                _loadSpriteToken = null;
            }
            else
            {
                if (!string.IsNullOrEmpty(_loadSpriteAssetName))
                {
                    AppEntry.Resource.UnloadAsset(_loadSpriteAssetName);
                    _loadSpriteAssetName = string.Empty;
                }
            }

            if (string.IsNullOrEmpty(assetPath))
            {
                sprite = null;
                return;
            }

            _loadSpriteToken = new CancellationTokenSource();
            var asset = await AppEntry.Resource.LoadAssetAwait<Sprite>(assetPath, _loadSpriteToken.Token);
            sprite = asset;

            SetAlpha(DynamicLoadAlphaVal);

            if (loadIsSynNativeSize)
            {
                SetNativeSize();
            }

            _loadSpriteAssetName = assetPath;
            _loadSpriteToken = null;
            callback?.Invoke();
        }

        /// <summary>
        /// 设置图片，通过路径来加载AB包的图片
        /// 使用场景：当图片为单张图（不在图集中）,且需要此图做单独的材质处理时使用此接口，Sprite.Create生成的为独立个体
        /// PS:如果无需使用九宫格、拉伸、填充等操作，为减少开销可使用UGF_RawImage加载图片,不需要调用Sprite.Create
        /// </summary>
        /// <param name="atlasAssetPath">图集路径</param>
        /// <param name="assetName">图集内图处名</param>
        /// <param name="callback">加载回调</param>
        /// <param name="loadIsSynNativeSize">使用图片大小</param>
        public async UniTask SetImageCs(string atlasAssetPath, string assetName, GameFrameworkAction callback = null,
            bool loadIsSynNativeSize = false)
        {
            SetAlpha(0);
            if (_loadSpriteToken != null)
            {
                _loadSpriteToken.Cancel();
                _loadSpriteToken = null;
            }
            else
            {
                if (!string.IsNullOrEmpty(_loadSpriteAssetName))
                {
                    AppEntry.Resource.UnloadAsset(_loadSpriteAssetName);
                    _loadSpriteAssetName = string.Empty;
                }
            }

            if (string.IsNullOrEmpty(atlasAssetPath))
            {
                sprite = null;
                return;
            }

            _loadSpriteToken = new CancellationTokenSource();
            var atlas = await AppEntry.Resource.LoadAssetAwait<SpriteAtlas>(atlasAssetPath, _loadSpriteToken.Token);

            this.sprite = atlas.GetSprite(assetName);
            SetAlpha(DynamicLoadAlphaVal);
            if (loadIsSynNativeSize)
            {
                SetNativeSize();
            }

            _loadSpriteAssetName = atlasAssetPath;
            _loadSpriteToken = null;
            callback?.Invoke();
        }

        /// <summary>
        /// 下载图片到本地
        /// </summary>
        /// <param name="url">图片URL地址</param>
        /// <param name="callback">回调</param>
        /// <param name="loadIsSynNativeSize">使用图片尺寸</param>
        public async UniTask DownloadImage(string url, GameFrameworkAction callback = null,
            bool loadIsSynNativeSize = false)
        {
            SetAlpha(0);

            if (_downloadImageToken != null)
            {
                _downloadImageToken.Cancel();
                _downloadImageToken = null;
            }

            if (string.IsNullOrEmpty(url))
            {
                sprite = null;
                return;
            }

            _downloadImageToken = new CancellationTokenSource();

            try
            {
                Texture2D texture = await ResourceExtension.DownloadImage(url, _downloadImageToken.Token);

                if (texture != null)
                {
                    // 将Texture2D转换为Sprite
                    Rect rect = new Rect(0, 0, texture.width, texture.height);
                    Vector2 pivot = new Vector2(0.5f, 0.5f);
                    sprite = Sprite.Create(texture, rect, pivot);

                    SetAlpha(DynamicLoadAlphaVal);

                    if (loadIsSynNativeSize)
                    {
                        SetNativeSize();
                    }

                    callback?.Invoke();
                }
            }
            catch (OperationCanceledException)
            {
                // 下载被取消，不做处理
                Debug.Log($"Download image cancelled: {url}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Download image error: {ex.Message}");
            }
            finally
            {
                _downloadImageToken = null;
            }
        }

        /// <summary>
        /// 中断下载图片
        /// </summary>
        public void CancelDownloadImage()
        {
            if (_downloadImageToken != null)
            {
                _downloadImageToken.Cancel();
                _downloadImageToken = null;
            }
        }

        /// <summary>
        /// 设置颜色
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        public void SetColor(float r, float g, float b, float a)
        {
            color = new Color(r, g, b, a);
        }

        /// <summary>
        /// 设置颜色
        /// </summary>
        /// <param name="newColor"></param>
        public void SetColor(Color newColor)
        {
            this.color = newColor;
        }

        /// <summary>
        /// 获取颜色的代码
        /// </summary>
        /// <returns></returns>
        public string GetColorString()
        {
            return GetColorString(color);
        }

        /// <summary>
        /// 获取颜色的代码
        /// </summary>
        /// <returns></returns>
        public string GetColorString(Color newColor)
        {
            return ColorUtility.ToHtmlStringRGB(newColor);
        }


        /// <summary>
        /// 设置颜色
        /// </summary>
        /// <param name="colorStr"></param>
        public void SetColor(string colorStr)
        {
            ColorUtility.TryParseHtmlString(colorStr, out var nowColor);
            color = nowColor;
        }

        /// <summary>
        /// 重置图片大小到原资源Size
        /// </summary>
        public void SetImageNativeSize()
        {
            SetNativeSize();
        }

        /// <summary>
        /// 设置图片是否可被射线检测
        /// </summary>
        /// <param name="isRaycastTarget"></param>
        public void SetRaycastTarget(bool isRaycastTarget)
        {
            raycastTarget = isRaycastTarget;
        }

        public override Material GetModifiedMaterial(Material baseMaterial)
        {
            Material cModifiedMat = base.GetModifiedMaterial(baseMaterial);
            ModifiedMat = cModifiedMat;
            return cModifiedMat;
        }

        /// <summary>
        /// 设置图片上的Material的某个属性的值
        /// </summary>
        /// <param name="newName"></param>
        /// <param name="val"></param>
        public void SetMaterialParameterFloat(string newName, float val)
        {
            if (_loadMaterial)
            {
                _loadMaterial.SetFloat(newName, val);
                return;
            }
            ModifiedMat.SetFloat(newName, val);
        }

        /// <summary>
        /// 设置图片的Material
        /// </summary>
        /// <param name="materialsAsset">加载的材质名字</param>
        /// <param name="callBack">加载回调</param>
        /// <param name="restoreTime">使用当前加载的材质显示多少时间后，还原为原材质</param>
        public async UniTask SetMaterial(string materialsAsset, GameFrameworkAction callBack = null,
            float restoreTime = -1)
        {
            ;
            if (_loadMaterialToken != null)
            {
                _loadMaterialToken.Cancel();
                _loadMaterialToken = null;
            }
            else
            {
                ClearLoadMaterial();
            }

            if (string.IsNullOrEmpty(_loadMaterialAssetName))
            {
                this.material = null;
                return;
            }

            _loadMaterialToken = new CancellationTokenSource();
            Material asset = await AppEntry.Resource.LoadAssetAwait<Material>(materialsAsset, _loadMaterialToken.Token);

            _loadMaterial = new Material(asset);
            this.material = _loadMaterial;

            _loadMaterialAssetName = materialsAsset;
            _loadMaterialToken = null;

            if (restoreTime > -1)
            {
                Invoke("RestoreMaterial", restoreTime);
            }

            callBack?.Invoke();
        }

        /// <summary>
        /// 还原初始材质
        /// </summary>
        public void RestoreMaterial()
        {
            _cacheOriginalMaterial ??= material;
            ClearLoadMaterial();
            material = _cacheOriginalMaterial;
        }

        /// <summary>
        /// 通过CanvasGroup设置透明，会包含影响到子节点
        /// </summary> 
        /// <param name="endValue"></param>
        public void SetCanvasGroupAlpha(float endValue)
        {
            CanvasGroup group = this.gameObject.GetOrAddComponent<CanvasGroup>();
            group.alpha = endValue;
        }

        /// <summary>
        /// 设置Fill裁剪,只适用于Filled模式
        /// </summary>
        /// <param name="value"></param>
        public void SetFillAmount(float value)
        {
            StopAllCoroutines();
            if (type == Type.Filled)
            {
                fillAmount = value;
            }
        }

        /// <summary>
        /// 设置Fill裁剪,只适用于Filled模式，使用缓动效果
        /// </summary>
        /// <param name="toVal"></param>
        /// <param name="duration"></param>
        /// <param name="updateCall"></param>
        /// <param name="endCall"></param>
        public void DoFillAmount(float toVal, float duration, GameFrameworkAction<float> updateCall,
            GameFrameworkAction endCall = null)
        {
            if (!gameObject.activeInHierarchy) return;
            _waitForEndOfFrame ??= new WaitForEndOfFrame();
            StopAllCoroutines();
            StartCoroutine(SmoothAmountValue(toVal, duration, updateCall, endCall));
        }

        private IEnumerator SmoothAmountValue(float value, float duration, GameFrameworkAction<float> updateCall,
            GameFrameworkAction endCall = null)
        {
            float time = 0f;
            float originalValue = fillAmount;

            while (time < duration)
            {
                time += Time.deltaTime;
                fillAmount = Mathf.Lerp(originalValue, value, time / duration);
                updateCall.Invoke(fillAmount);
                yield return _waitForEndOfFrame;
            }

            fillAmount = value;
            endCall?.Invoke();
        }

        /// <summary>
        /// 设置透明度
        /// </summary>
        /// <param name="alpha">透明度</param>
        public void SetAlpha(float alpha)
        {
            if (this.sprite == null)
            {
                alpha = 0;
            }

            color = new Color(color.r, color.g, color.b, alpha);
        }

        /// <summary>
        /// Alpha渐变
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="duration"></param>
        /// <param name="ignoreTimeScale"></param>
        public override void CrossFadeAlpha(float alpha, float duration, bool ignoreTimeScale)
        {
            if (!gameObject.activeInHierarchy) return;
            _waitForEndOfFrame ??= new WaitForEndOfFrame();
            StopAllCoroutines();
            _alphaColor = color;
            StartCoroutine(SmoothAlphaValue(_alphaColor.a, alpha, duration));
        }

        public IEnumerator SmoothAlphaValue(float oldVal, float value, float duration)
        {
            float time = 0f;
            float originalValue = oldVal;
            while (time < duration)
            {
                time += Time.deltaTime;
                _alphaColor.a = Mathf.Lerp(originalValue, value, time / duration);
                color = _alphaColor;
                yield return _waitForEndOfFrame;
            }

            color = new Color(color.r, color.g, color.b, value);
        }

        /// <summary>
        /// 激活UI时，检测动态加载
        /// </summary>
        private void StartDynamicLoadImage()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (!DynamicLoad)
            {
                return;
            }

            if (string.IsNullOrEmpty(DynamicLoadPath))
            {
                if (string.IsNullOrEmpty(_loadSpriteAssetName))
                {
                    SetAlpha(0);
                }

                return;
            }

            if (!DynamicLoadPath.Contains("Assets/Res"))
            {
                return;
            }

            if (_loadSpriteAssetName != null
                && _loadSpriteAssetName.Equals(DynamicLoadPath))
            {
                return;
            }

            SetImageCs(DynamicLoadPath);
        }

        private void ClearLoadMaterial()
        {
            if (_loadMaterial)
            {
                Destroy(_loadMaterial);
                _loadMaterial = null;
            }
            
            if (!string.IsNullOrEmpty(_loadMaterialAssetName))
            {
                AppEntry.Resource.UnloadAsset(_loadMaterialAssetName);
                _loadMaterialAssetName = string.Empty;
            }
        }

        #region Filled模块支持九宫格效果

        [HideInInspector] public bool m_SlicedClipMode = false;

        private Vector2[] s_VertScratch = new Vector2[4];
        private Vector2[] s_UVScratch = new Vector2[4];

        private void GenerateSlicedSprite(VertexHelper toFill)
        {
            var activeSprite = overrideSprite ?? sprite;

            Vector4 outer, inner, padding, border;

            if (activeSprite != null)
            {
                outer = UnityEngine.Sprites.DataUtility.GetOuterUV(activeSprite);
                inner = UnityEngine.Sprites.DataUtility.GetInnerUV(activeSprite);
                padding = UnityEngine.Sprites.DataUtility.GetPadding(activeSprite);
                border = activeSprite.border;
            }
            else
            {
                outer = Vector4.zero;
                inner = Vector4.zero;
                padding = Vector4.zero;
                border = Vector4.zero;
            }

            Rect rect = GetPixelAdjustedRect();
            Vector4 adjustedBorders = GetAdjustedBorders(border / pixelsPerUnit, rect);
            padding = padding / pixelsPerUnit;

            s_VertScratch[0] = new Vector2(padding.x, padding.y);
            s_VertScratch[3] = new Vector2(rect.width - padding.z, rect.height - padding.w);

            s_VertScratch[1].x = adjustedBorders.x;
            s_VertScratch[1].y = adjustedBorders.y;

            s_VertScratch[2].x = rect.width - adjustedBorders.z;
            s_VertScratch[2].y = rect.height - adjustedBorders.w;

            for (int i = 0; i < 4; ++i)
            {
                s_VertScratch[i].x += rect.x;
                s_VertScratch[i].y += rect.y;
            }

            s_UVScratch[0] = new Vector2(outer.x, outer.y);
            s_UVScratch[1] = new Vector2(inner.x, inner.y);
            s_UVScratch[2] = new Vector2(inner.z, inner.w);
            s_UVScratch[3] = new Vector2(outer.z, outer.w);

            float xLength = s_VertScratch[3].x - s_VertScratch[0].x;
            float yLength = s_VertScratch[3].y - s_VertScratch[0].y;
            float len1XRatio = (s_VertScratch[1].x - s_VertScratch[0].x) / xLength;
            float len1YRatio = (s_VertScratch[1].y - s_VertScratch[0].y) / yLength;
            float len2XRatio = (s_VertScratch[2].x - s_VertScratch[1].x) / xLength;
            float len2YRatio = (s_VertScratch[2].y - s_VertScratch[1].y) / yLength;
            float len3XRatio = (s_VertScratch[3].x - s_VertScratch[2].x) / xLength;
            float len3YRatio = (s_VertScratch[3].y - s_VertScratch[2].y) / yLength;
            int xLen = 3, yLen = 3;
            if (fillMethod == FillMethod.Horizontal)
            {
                if (fillAmount >= (len1XRatio + len2XRatio))
                {
                    float ratio = 1 - (fillAmount - (len1XRatio + len2XRatio)) / len3XRatio;
                    s_VertScratch[3].x = s_VertScratch[3].x - (s_VertScratch[3].x - s_VertScratch[2].x) * ratio;
                    s_UVScratch[3].x = s_UVScratch[3].x - (s_UVScratch[3].x - s_UVScratch[2].x) * ratio;
                }
                else if (fillAmount >= len1XRatio)
                {
                    xLen = 2;
                    float ratio = 1 - (fillAmount - len1XRatio) / len2XRatio;
                    s_VertScratch[2].x = s_VertScratch[2].x - (s_VertScratch[2].x - s_VertScratch[1].x) * ratio;
                }
                else
                {
                    xLen = 1;
                    float ratio = 1 - fillAmount / len1XRatio;
                    s_VertScratch[1].x = s_VertScratch[1].x - (s_VertScratch[1].x - s_VertScratch[0].x) * ratio;
                    s_UVScratch[1].x = s_UVScratch[1].x - (s_UVScratch[1].x - s_UVScratch[0].x) * ratio;
                }
            }
            else if (fillMethod == FillMethod.Vertical)
            {
                if (fillAmount >= (len1YRatio + len2YRatio))
                {
                    float ratio = 1 - (fillAmount - (len1YRatio + len2YRatio)) / len3YRatio;
                    s_VertScratch[3].y = s_VertScratch[3].y - (s_VertScratch[3].y - s_VertScratch[2].y) * ratio;
                    s_UVScratch[3].y = s_UVScratch[3].y - (s_UVScratch[3].y - s_UVScratch[2].y) * ratio;
                }
                else if (fillAmount >= len1YRatio)
                {
                    yLen = 2;
                    float ratio = 1 - (fillAmount - len1YRatio) / len2YRatio;
                    s_VertScratch[2].y = s_VertScratch[2].y - (s_VertScratch[2].y - s_VertScratch[1].y) * ratio;
                }
                else
                {
                    yLen = 1;
                    float ratio = 1 - fillAmount / len1YRatio;
                    s_VertScratch[1].y = s_VertScratch[1].y - (s_VertScratch[1].y - s_VertScratch[0].y) * ratio;
                    s_UVScratch[1].y = s_UVScratch[1].y - (s_UVScratch[1].y - s_UVScratch[0].y) * ratio;
                }
            }

            toFill.Clear();

            for (int x = 0; x < xLen; ++x)
            {
                int x2 = x + 1;

                for (int y = 0; y < yLen; ++y)
                {
                    if (!fillCenter && x == 1 && y == 1)
                        continue;

                    int y2 = y + 1;


                    AddQuad(toFill,
                        new Vector2(s_VertScratch[x].x, s_VertScratch[y].y),
                        new Vector2(s_VertScratch[x2].x, s_VertScratch[y2].y),
                        color,
                        new Vector2(s_UVScratch[x].x, s_UVScratch[y].y),
                        new Vector2(s_UVScratch[x2].x, s_UVScratch[y2].y));
                }
            }
        }

        static void AddQuad(VertexHelper vertexHelper, Vector2 posMin, Vector2 posMax, Color32 color, Vector2 uvMin,
            Vector2 uvMax)
        {
            int startIndex = vertexHelper.currentVertCount;

            vertexHelper.AddVert(new Vector3(posMin.x, posMin.y, 0), color, new Vector2(uvMin.x, uvMin.y));
            vertexHelper.AddVert(new Vector3(posMin.x, posMax.y, 0), color, new Vector2(uvMin.x, uvMax.y));
            vertexHelper.AddVert(new Vector3(posMax.x, posMax.y, 0), color, new Vector2(uvMax.x, uvMax.y));
            vertexHelper.AddVert(new Vector3(posMax.x, posMin.y, 0), color, new Vector2(uvMax.x, uvMin.y));

            vertexHelper.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            vertexHelper.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
        }

        private Vector4 GetAdjustedBorders(Vector4 border, Rect adjustedRect)
        {
            Rect originalRect = rectTransform.rect;

            for (int axis = 0; axis <= 1; axis++)
            {
                float borderScaleRatio;

                if (originalRect.size[axis] != 0)
                {
                    borderScaleRatio = adjustedRect.size[axis] / originalRect.size[axis];
                    border[axis] *= borderScaleRatio;
                    border[axis + 2] *= borderScaleRatio;
                }

                float combinedBorders = border[axis] + border[axis + 2];
                if (adjustedRect.size[axis] < combinedBorders && combinedBorders != 0)
                {
                    borderScaleRatio = adjustedRect.size[axis] / combinedBorders;
                    border[axis] *= borderScaleRatio;
                    border[axis + 2] *= borderScaleRatio;
                }
            }

            return border;
        }

        #endregion
    }
}