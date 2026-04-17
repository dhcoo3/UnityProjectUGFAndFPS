using System;
using System.Collections;
using Builtin.Scripts.Extension;
using Builtin.Scripts.Game;
using GameFramework.Event;
using HotAssets.Scripts.Common;
using HotAssets.Scripts.Extension;
using HotAssets.Scripts.UI.Tool.Component;
using TuanjieAI.Assistant.Schema;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.UI.Module.FightShare
{
    public class OverlayFightDamageUI:ExPanel
    {
        #region Auto Create
        #endregion
        
        private GameObject DamageTextUI;
        
        protected override void RegisterUI()
        {
            #region Auto Bind
            #endregion
        }
        
        protected override void RegisterEvent()
        {
            EventHelper.Subscribe(FightPopUpNumberEventArgs.EventId, EPopUpNumberHandler);
        }

        protected override void OnOpen(object userData)
        {
            LoadDamageText();
            base.OnOpen(userData);
        }
        
        private async void LoadDamageText()
        {
            try
            {
                if (DamageTextUI == null)
                {
                    string roleHudPath = AssetPathUtil.GetUIFormPath("FightCommon/DamageTextUI");
                    var asset = await AppEntry.Resource.LoadAssetAwait<GameObject>(roleHudPath);
                    DamageTextUI = asset;
                }
            }
            catch (Exception)
            {
               Log.Warning("加载DamageTextUI失败");
            }
        }
        
        /// <summary>
        /// 响应伤害弹字事件。
        /// </summary>
        private void EPopUpNumberHandler(object sender, GameEventArgs e)
        {
            FightPopUpNumberEventArgs args = (FightPopUpNumberEventArgs)e;
            StartCoroutine(StartShow(args.Position, args.DamageVal, args.IsHeal));
        }
        
        /// <summary>
        /// 显示伤害数字：向上方随机角度弹出，并渐变消失
        /// </summary>
        IEnumerator StartShow(fix3 pos,int damageVal,bool isHeal)
        {
            // 调用异步实例化方法
            var instantiateOperation = UnityEngine.Object.InstantiateAsync(DamageTextUI,this.transform);

            // 等待实例化完成
            yield return instantiateOperation;

            // 获取实例化后的对象
            GameObject[] instantiatedObj = instantiateOperation.Result;

            if (instantiatedObj == null)
                yield break;

            UIContainer uiContainer = instantiatedObj[0].GetComponent<UIContainer>();
            uiContainer.Get<ExText>("m_Damage").SetText(damageVal.ToString());

            Vector2 uiPos = Utils.WorldToUIPoint(new Vector3(pos.x,pos.y,0));
            RectTransform rectTransform = instantiatedObj[0].GetComponent<RectTransform>();
            rectTransform.anchoredPosition3D = uiPos;

            // 添加 CanvasGroup 用于淡出控制
            CanvasGroup canvasGroup = instantiatedObj[0].GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = instantiatedObj[0].AddComponent<CanvasGroup>();

            // 随机向上角度（-45° ~ +45° 偏移，Y 方向始终为正）
            float angle = UnityEngine.Random.Range(-45f, 45f);
            Vector2 direction = new Vector2(Mathf.Sin(angle * Mathf.Deg2Rad), Mathf.Cos(angle * Mathf.Deg2Rad));
            float speed = 150f;
            float duration = 1f;
            float elapsed = 0f;
            Vector2 startPos = rectTransform.anchoredPosition;

            // 逐帧：向上弹出 + 渐变消失
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                rectTransform.anchoredPosition = startPos + direction * (speed * elapsed);
                canvasGroup.alpha = 1f - t;
                yield return null;
            }

            GameObject.Destroy(instantiatedObj[0]);
        }
    }
}
