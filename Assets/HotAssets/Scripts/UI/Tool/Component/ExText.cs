using System;
using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using Builtin.Scripts.Game;
using Cysharp.Threading.Tasks;
using GameFramework;
using HotAssets.Scripts.Extension;
using TMPro;
using UnityEngine;

namespace HotAssets.Scripts.UI.Tool.Component
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class ExText : MonoBehaviour
    {
        private CancellationTokenSource _loadMaterialToken;

        /// <summary>
        /// 原材质球
        /// </summary>
        private Material m_CacheOriginalMaterial;

        /// <summary>
        /// 缓存材质名
        /// </summary>
        private string _loadMaterialAssetName;

        private Material _loadMaterial;

        /// 自动本地化文字
        /// </summary>
        [HideInInspector] public bool UseLocalization;

        /// <summary>
        /// 本地化Key
        /// </summary>
        [HideInInspector] public string LocalizationKey = "";

        private WaitForEndOfFrame m_WaitForEndOfFrame;

        private TextMeshProUGUI _textMeshPro;

        public TextMeshProUGUI TMP
        {
            get { return _textMeshPro ??= GetComponent<TextMeshProUGUI>(); }
        }


        protected void Awake()
        {
            if (Application.isPlaying && UseLocalization && !string.IsNullOrEmpty(LocalizationKey))
            {
                SetLocalizationText(LocalizationKey);
            }

#if UNITY_EDITOR
            CheckLocalization();
#endif
        }

        protected void OnDestroy()
        {
            ClearLoadMaterial();
        }

        /// <summary>
        /// 设置文字
        /// </summary>
        /// <param name="newVal"></param>
        public void SetText(string newVal)
        {
            TMP.text = newVal;
        }

        /// <summary>
        /// 通过本地化Key,加载本地化文字
        /// </summary>
        /// <param name="key">多语言ID</param>
        public void SetLocalizationText(string key)
        {
            if (AppEntry.Localization == null)
            {
                return;
            }

            string str = AppEntry.Localization.GetString(key);
            if (string.IsNullOrEmpty(str))
            {
                return;
            }

            //转表时，会将@用于标记为转行符
            SetText(str);
        }

        private void ClearLoadMaterial()
        {
            if (!string.IsNullOrEmpty(_loadMaterialAssetName))
            {
                if (_loadMaterial)
                {
                    Destroy(_loadMaterial);
                    _loadMaterial = null;
                }

                AppEntry.Resource.UnloadAsset(_loadMaterialAssetName);
                _loadMaterialAssetName = string.Empty;
            }
        }

        /// <summary>
        /// 获取文字
        /// </summary>
        /// <returns></returns>
        public string GetText()
        {
            return TMP.text;
        }

        /// <summary>
        /// 设置文字大小
        /// </summary>
        /// <param name="size"></param>
        public void SetTextSize(int size)
        {
            TMP.fontSize = size;
        }

        /// <summary>
        /// 跳动变化数字
        /// </summary>
        /// <param name="startNum">开始值</param>
        /// <param name="endNum">结束值</param>
        /// <param name="duration">持续时间</param>
        /// <param name="decimalPlace">小数位传0，变化时，不再计算小数位</param>
        /// <param name="saveIntCount">整数位</param>
        /// <param name="endCall">回调</param>
        /// <param name="updateCall">回调</param>
        public void JumpToNumber(float startNum, float endNum, float duration, int decimalPlace = 0,
            int saveIntCount = 0, GameFrameworkAction endCall = null, GameFrameworkAction<float> updateCall = null)
        {
            m_WaitForEndOfFrame ??= new WaitForEndOfFrame();
            StopAllCoroutines();
            StartCoroutine(SmoothValue(startNum, endNum, duration, decimalPlace, saveIntCount, endCall, updateCall));
        }

        private IEnumerator SmoothValue(float startNum, float endNum, float duration, int decimalPlace = 0,
            int saveIntCount = 0, GameFrameworkAction endCall = null, GameFrameworkAction<float> updateCall = null)
        {
            float time = 0f;
            while (time < duration)
            {
                time += Time.deltaTime;
                double val = Mathf.Lerp(startNum, endNum, time / duration);

                //double temp = Math.Round(val, decimalPlace);
                string tempStr = Math.Round(val, decimalPlace).ToString(CultureInfo.InvariantCulture);

                if (saveIntCount != 0)
                {
                    if (tempStr.Length < saveIntCount)
                    {
                        int addLen = saveIntCount - tempStr.Length;

                        for (int i = 1; i <= addLen; i++)
                        {
                            tempStr = string.Format("{0}{1}", "0", tempStr);
                        }
                    }
                }

                SetText(tempStr);
                // updateCall?.Call(tempStr);
                yield return new WaitForEndOfFrame();
            }

            SetText(Math.Round(endNum, decimalPlace).ToString(CultureInfo.InvariantCulture));
        }


        /// <summary>
        /// 设置文字颜色
        /// </summary>
        /// <param name="color"></param>
        public void SetColor(Color color)
        {
            TMP.color = color;
            ;
        }

        /// <summary>
        /// 设置文字颜色
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        public void SetColor(float r, float g, float b, float a)
        {
            TMP.color = new Color(r, g, b, a);
        }

        /// <summary>
        /// 设置文字颜色
        /// </summary>
        /// <param name="colorStr"></param>
        public Color SetColor(string colorStr)
        {
            ColorUtility.TryParseHtmlString(colorStr, out var nowColor);
            TMP.color = nowColor;
            return nowColor;
        }

        /// <summary>
        /// 获取颜色的代码
        /// </summary>
        /// <returns></returns>
        public string GetColorString()
        {
            return GetColorString(TMP.color);
        }

        /// <summary>
        /// 获取颜色的代码
        /// </summary>
        /// <returns></returns>
        public string GetColorString(Color color)
        {
            return ColorUtility.ToHtmlStringRGB(color);
        }

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
                TMP.material = null;
                return;
            }

            _loadMaterialToken = new CancellationTokenSource();
            Material asset = await AppEntry.Resource.LoadAssetAwait<Material>(materialsAsset, _loadMaterialToken.Token);

            _loadMaterial = new Material(asset);
            TMP.material = _loadMaterial;

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
            m_CacheOriginalMaterial ??= TMP.material;
            ClearLoadMaterial();
            TMP.material = m_CacheOriginalMaterial;
        }

        /// <summary>
        /// 检测本地化文本是否正确使用
        /// </summary>
        public void CheckLocalization()
        {
            if(TMP == null) return;
            if (string.IsNullOrEmpty(TMP.text)) return;
            bool isChinese = ContainsChinese(TMP.text);
            if (isChinese)
            {
                if (name.Contains("m_")) return;
                if (!UseLocalization)
                {
                    Debug.LogWarning("文本使用中文，但没有使用代码赋值也没有使用自动本地化功能,本地化文字会失败，请点击这里调整:" + this.name, this.gameObject);
                }
            }
        }

        /// <summary>
        /// 是否包含中文
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public bool ContainsChinese(string input)
        {
            return Regex.IsMatch(input, @"[\u4e00-\u9fa5]");
        }
    }
}