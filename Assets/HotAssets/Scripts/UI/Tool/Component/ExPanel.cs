using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Builtin.Scripts.Game;
using Builtin.Scripts.UnityGameFrameworkHelper;
using GameFramework;
using HotAssets.Scripts.Common;
using HotAssets.Scripts.Common.Event;
using HotAssets.Scripts.Extension;
using HotAssets.Scripts.UI.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.UI.Tool.Component
{
    [RequireComponent(typeof(UIContainer))]
    public class ExPanel : UIFormLogic
    {
    
        public enum UIModel
        {
            /// <summary>
            /// 全屏适配
            /// </summary>
            FullScreen,

            /// <summary>
            /// 非全屏适配
            /// </summary>
            NotFullScreen
        }
    
        /// <summary>
        /// UI模式
        /// </summary>
        public UIModel m_UIModel = UIModel.FullScreen;

        /// <summary>
        /// UI所处的层级
        /// </summary>
        public GameGlobalSetting.SortingLayers Layers = GameGlobalSetting.SortingLayers.UI;

        private string m_LayerName = "";
        
        /// <summary>
        /// UI相对于当前层级的深度,每个UI在当前层级间隔20
        /// </summary>
        public const int DepthFactor = 20;

        /// <summary>
        /// 是否打开渐变效果
        /// </summary>
        public bool OpenFade = true;

        /// <summary>
        /// 渐变的过渡时间
        /// </summary>
        private const float FadeTime = 0.3f;

        /// <summary>
        /// 是否自动适配深度变化
        /// </summary>
        public bool AutoDepthChanged = true;

        /// <summary>
        /// 当前UICanvas
        /// </summary>
        private Canvas m_CachedCanvas;

        /// <summary>
        /// 当前UICanvasGroup
        /// </summary>
        private CanvasGroup m_CanvasGroup;

        /// <summary>
        /// 原深度
        /// </summary>
        public int OriginalDepth
        {
            get;
            private set;
        }

        public int Depth => m_CachedCanvas.sortingOrder;
    
        private int m_UIGroupDepth;
    
        private int m_DepthInUIGroup;

        /// <summary>
        /// 子节点所有的Canvas
        /// </summary>
        private Dictionary<string, int> m_CachedCanvasContainer;

        /// <summary>
        /// 子节点所有的渲染器
        /// </summary>
        private Dictionary<string, int> m_RendererDic;

        /// <summary>
        /// 子节点所有的SortingGroup
        /// </summary>
        private Dictionary<string, int> m_SortingGroupDic;


        /// <summary>
        /// 打开此UI时播放的音效
        /// </summary>
        [HideInInspector]
        public int OpenSoundId;

        /// <summary>
        /// 关闭此UI时播放的音效
        /// </summary>
        [HideInInspector]
        public int CloseSoundId;
   

        /// <summary>
        /// 子节点排序组件
        /// </summary>
        private ILayoutController[] m_LayoutControllers;

        public ILayoutController[] LayoutControllers
        {
            get
            {
                return m_LayoutControllers??= GetComponentsInChildren<ILayoutController>();
            }
        }
   
        /// <summary>
        /// 红点对象引用字典
        /// </summary>
        private Dictionary<int, int> m_RedDotDic;

        GraphicRaycaster m_GraphicRaycaster;     
    
        private UIContainer m_PanelUIContainer;

        public UIContainer PanelUIContainer
        {
            get
            {
                if (m_PanelUIContainer == null)
                {
                    m_PanelUIContainer = GetComponent<UIContainer>();
                }
                return m_PanelUIContainer;
            }
        }
    
        /// <summary>
        /// 获取界面是否可用。
        /// </summary>
        private bool m_UIAvailable;
    
        public bool AddCanvasGroup = true;

        public bool AddGraphicRaycaster = true;
    
        private AAAGameEventHelper m_EventHelper = new AAAGameEventHelper();

        public AAAGameEventHelper EventHelper => m_EventHelper;

        public RectTransform URectTransform
        {
            get
            {
                return GetComponent<RectTransform>();
            }
        }

        public MonoBehaviour Mono
        {
            get
            {
                return GetComponent<MonoBehaviour>();
            }
        }
    
        public UIParams Params { get; private set; }
    
        /// <summary>
        /// 子UI界面, 会随着父界面关闭而关闭
        /// </summary>
        IList<int> m_SubUIForms = null;
    
        protected void Awake()
        {
#if UNITY_EDITOR
            CheckLocalization();
#endif
        }

        /// <summary>
        /// 获取节点组件
        /// </summary>
        /// <typeparam name="T">获取节点的指定组件类型</typeparam>
        /// <param name="key">节点名</param>
        /// <returns>返回节点的指定组件类型</returns>
        public T Get<T>(string key)
        {
            return PanelUIContainer.Get<T>(key);
        }

        /// <summary>
        /// 关闭UI
        /// </summary>
        /// <param name="closeCall">由底层接口调用关闭回调</param>
        public void Close(GameFrameworkAction closeCall = null)
        {
            StopAllCoroutines();
            PlayCloseAnim();
            Close(OpenFade, closeCall);
        }

        /// <summary>
        /// 关闭UI
        /// </summary>
        public void Close(bool ignoreFade, GameFrameworkAction closeCall = null)
        {
            if (ignoreFade && isActiveAndEnabled)
            {
                StartCoroutine(CloseCo(FadeTime, closeCall));
            }
            else
            {
                if (closeCall != null)
                {
                    closeCall();
                }
                else
                {
                    AppEntry.UI.CloseUIForm(UIForm);
                }
            }
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="soundId"></param>
        private void PlayUISound(int soundId)
        {
            //AppEntry.Sound.PlaySound(soundId);
        }
   
        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
        
            m_CachedCanvas = gameObject.GetOrAddComponent<Canvas>();
            m_CachedCanvas.overrideSorting = true;
            OriginalDepth = m_CachedCanvas.sortingOrder;
        
            if (AddCanvasGroup)
            {
                m_CanvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
            }
     
            m_LayerName = Layers.ToString();
            m_CachedCanvas.sortingLayerName = m_LayerName;
            FindNeedRefreshDepthChild();
        
            URectTransform.anchoredPosition3D = Vector3.zero;
        
            if (m_UIModel == UIModel.FullScreen)
            {
                AdaptiveNotchFit();
            }
        
            if (AddGraphicRaycaster)
            {
                m_GraphicRaycaster = gameObject.GetOrAddComponent<GraphicRaycaster>();
            }

            ExButton[] btns = GetComponentsInChildren<ExButton>(true);
            foreach(ExButton btn in btns)
            {
                if (!btn.CustomPolygon)
                {                  
                    if(btn.PolygonCollider2D)
                    {
                        float width = btn.GetComponent<RectTransform>().rect.width;
                        float height = btn.GetComponent<RectTransform>().rect.height;
                        float widthValue = float.Parse((width * 0.5f).ToString(CultureInfo.InvariantCulture));
                        float heightValue = float.Parse((height * 0.5f).ToString(CultureInfo.InvariantCulture));
                        Vector2 vector1 = new Vector2(widthValue * -1, heightValue);
                        Vector2 vector2 = new Vector2(widthValue, heightValue);
                        Vector2 vector3 = new Vector2(widthValue, heightValue * -1);
                        Vector2 vector4 = new Vector2(widthValue * -1, heightValue * -1);
                        btn.PolygonCollider2D.points = new[] { vector1, vector2, vector3, vector4 };
                    }                  
                }
            }
        }

        protected override void OnOpen(object userData)
        {           
            //Debug.Log("界面打开OnOpen：" + this.gameObject.name); 
            var cvs = GetComponent<Canvas>();
        
            Params = userData as UIParams;
        
            if (Params is { SortOrder: not null } && cvs != null)
            {
                cvs.sortingOrder = Params.SortOrder.Value;
            }
       
            RegisterUI();
            RegisterEvent();
            PlayOpenAnim();
            m_UIAvailable = true;
            base.OnOpen(userData);
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            //Debug.Log("界面关闭OnClose：" + this.gameObject.name);  
            m_UIAvailable = false;
            EventHelper.RemoveAllSubscribe();
            ClearAllRedDot();
            ClearAllEventTrigger();
            CloseAllSubUIForms();
            Close(() => {
                    base.OnClose(isShutdown, userData);}
            );        
        }


        protected override void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
        {
            m_UIGroupDepth = uiGroupDepth;
            m_DepthInUIGroup = depthInUIGroup;
            base.OnDepthChanged(m_UIGroupDepth, m_DepthInUIGroup);

            //面板
            int deltaDepth = UGuiGroupHelper.DepthFactor * m_UIGroupDepth + DepthFactor * m_DepthInUIGroup;
            RefreshChildDepth(deltaDepth);
        }

        protected override void InternalSetVisible(bool visible)
        {
            if (Available && visible)
            {
                AppEntry.UI.RefocusUIForm(UIForm);
            }                    

            if (!visible)
            {
                if (m_UIAvailable)
                {
                    // m_UIBackGrounpMask?.SetActive(false); 
                    ClearAllRedDot();
                
                    //Debug.Log("设置界面可见 InternalSetVisible：" + this.gameObject.name + " " + visible);
                    Close(() =>
                    {
                        gameObject.SetActive(false);
                    });    
                }
            }
            else
            {
                if (m_GraphicRaycaster)
                {
                    SetCanvasGroupAlpha(1);
                }
                //Debug.Log("设置界面可见 InternalSetVisible：" + this.gameObject.name + " " + visible);
                gameObject.SetActive(true);
                // m_UIBackGrounpMask?.SetActive(true); 
           
            }     
        }

        protected override void OnPause()
        {
            gameObject.SetActive(false);
            base.OnPause();
        }

        /// <summary>
        /// 渐进效果
        /// </summary>
        public void FadeIn()
        {
            if (gameObject.activeSelf&&m_CanvasGroup)
            {
                if (OpenFade)
                {
                    SetCanvasGroupAlpha(0);
                    StartCoroutine(m_CanvasGroup.FadeToAlpha(1f, FadeTime));
                }
                else
                {
                    SetCanvasGroupAlpha(1);
                }
            }   
        }

        private IEnumerator CloseCo(float duration, GameFrameworkAction closeCall = null)
        {
            yield return m_CanvasGroup.FadeToAlpha(0f, duration);
            if (closeCall != null)
            {
                closeCall();
            }
            else
            {
                AppEntry.UI.CloseUIForm(UIForm);
            }
        }

        /// <summary>
        /// 初始化需要更新层级的子节点对象
        /// </summary>
        private void FindNeedRefreshDepthChild()
        {
            if (AutoDepthChanged)
            {         
                m_CachedCanvasContainer ??= new Dictionary<string, int>();
            
                List<Canvas> canvas = new List<Canvas>();
                GetComponentsInChildren(true, canvas);
                foreach (var t in canvas)
                {
                    if(t != m_CachedCanvas)
                    {
                        m_CachedCanvasContainer.TryAdd(t.name,t.sortingOrder);
                    }
                }
          
                //Renderer组件
                m_RendererDic ??= new Dictionary<string, int>();
                List<Renderer> renderers = new List<Renderer>();
                GetComponentsInChildren(true, renderers);
                foreach (var t in renderers)
                {
                    m_RendererDic.TryAdd(t.name,t.sortingOrder);
                }
            
                //SortingGroup组件
                m_SortingGroupDic ??= new Dictionary<string, int>();
                List<SortingGroup> sortingGroups = new List<SortingGroup>();
                GetComponentsInChildren(true, sortingGroups);
                foreach (var t in sortingGroups)
                {
                    m_SortingGroupDic.TryAdd(t.name,t.sortingOrder);
                }
            }
        }

        /// <summary>
        /// 刷新子节点的深度
        /// </summary>
        /// <param name="baseDepth">基础深度值，用于计算子节点的深度</param>
        private void RefreshChildDepth(int baseDepth)
        {
            m_CachedCanvas.sortingOrder = baseDepth;
            m_CachedCanvas.sortingLayerName = m_LayerName;

            if (AutoDepthChanged)
            {          
                //UI层级变化
                List<Canvas> canvas = new List<Canvas>();
                GetComponentsInChildren(true, canvas);
                foreach (var t in canvas)
                {
                    if(t != m_CachedCanvas)
                    {
                        var layer = 0;
                        if (m_CachedCanvasContainer.TryGetValue(t.name, out layer))
                        {
                            t.sortingOrder = layer + baseDepth;
                            t.sortingLayerName = m_LayerName;
                        }
                    }
                }
          
                //Renderer组件
                m_RendererDic ??= new Dictionary<string, int>();
                List<Renderer> renderers = new List<Renderer>();
                GetComponentsInChildren(true, renderers);
                foreach (var t in renderers)
                {
                    var layer = 0;
                    if (m_RendererDic.TryGetValue(t.name, out layer))
                    {
                        t.sortingOrder = layer + baseDepth;
                        t.sortingLayerName = m_LayerName;
                    }
                }
            
                //SortingGroup组件
                m_SortingGroupDic ??= new Dictionary<string, int>();
                List<SortingGroup> sortingGroups = new List<SortingGroup>();
                GetComponentsInChildren(true, sortingGroups);
                foreach (var t in sortingGroups)
                {
                    var layer = 0;
                    if (m_SortingGroupDic.TryGetValue(t.name, out layer))
                    {
                        t.sortingOrder = layer + baseDepth;
                        t.sortingLayerName = m_LayerName;
                    }
                }
            }

            //m_CallBackListener.CallLua(CallBackType.UIDepthChanged,GetInstanceID());
        }
    
        /// <summary>
        /// 使用排序组参数
        /// </summary>
        /// <param name="sortingLayerName"></param>
        /// <param name="order"></param>
        public void SetSortingGroup(string sortingLayerName, int order)
        {
            m_LayerName = sortingLayerName;
            m_CachedCanvas.overrideSorting = true;
            RefreshChildDepth(order);
        }

        /// <summary>
        ///重新刷新排序组件，使其重绘
        /// </summary>
        public void RefreshLayout()
        {
            ILayoutController[] layoutControllers = GetComponentsInChildren<ILayoutController>();
        
            if(layoutControllers.Length > 0)
            {
                for (int i = layoutControllers.Length - 1; i >= 0; i--)
                {
                    layoutControllers[i]?.SetLayoutVertical();
                    layoutControllers[i]?.SetLayoutHorizontal();
                }
            } 
        }

        /// <summary>
        /// 添加红点
        /// </summary>
        /// <param name="target">UGF UI对象</param>
        /// <param name="pos">pos 1左上 2右上 3左下 4右下</param>
        /// <param name="x">x坐标</param>
        /// <param name="y">y坐标</param>
        /// <param name="callBack">回调</param>
        public void AddRedDot(GameObject target,int pos=2,float x=0,float y=0,GameFrameworkAction callBack = null)
        {
            if(target == null) return;
        
            int tHashCode = target.GetHashCode();
  

            m_RedDotDic ??= new Dictionary<int, int>();
        
            if (!m_RedDotDic.ContainsKey(tHashCode))
            {
                /*int instanceId = UIFramework.UISubjoin.LoadUISubjoin(UIFramework.AssetBridge.GetUIFormAsset("Global/UIRedDot"),"UISubjoinRedDot",
                redDot =>
                {
                    if (!Visible)
                    {
                        ClearAllRedDot();
                        return;
                    }
                    
                    UISubjoinRedDotLogic logic = (UISubjoinRedDotLogic)redDot;
                    
                    logic.Init(target.UTransform, pos, x, y,m_CachedCanvas.sortingOrder);
                    
                    if (callBack != null)
                    {
                        m_CallBackListener.CallLua(CallBackType.UIRedDot,tHashCode,logic);
                    }
                });
            
            m_RedDotDic.Add(tHashCode, instanceId);*/
            }
        }

        /// <summary>
        /// 移除红点
        /// </summary>
        /// <param name="target">UGF UI对象</param>
        public void RemoveRedDot(GameObject target)
        {
            if (target == null)
            {
                return;
            }
        
            if (m_RedDotDic == null)
            {
                return;
            }
        
            int id = target.GetHashCode();

            if (m_RedDotDic.ContainsKey(id))
            {
                //UIFramework.UISubjoin.RemoveUISubjoin(m_RedDotDic[id]);
                m_RedDotDic.Remove(id);
            }
        }

        /// <summary>
        /// 清除该面板上所有的红点
        /// </summary>
        public void ClearAllRedDot()
        {
            /*m_CallBackListener.RemoveLua(CallBackType.UIRedDot);
        
        if (m_RedDotDic == null)
        {
            return;
        }
        
        foreach(KeyValuePair<int,int> keyValuePair in m_RedDotDic)
        {               
            UIFramework.UISubjoin.RemoveUISubjoin(keyValuePair.Value);               
        }

        m_RedDotDic.Clear();*/
        }

        /// <summary>
        /// 清除所有的事件注册函数
        /// </summary>
        public void ClearAllEventTrigger()
        {          
            /*foreach (ExButton button in GetComponentsInChildren<ExButton>())
        {
            button.EventTrigger.Clean();
        }

        foreach (UGF_ItemCell itemCell in GetComponentsInChildren<UGF_ItemCell>())
        {
            itemCell.EventTrigger.Clean();
        }
        
        SetLuaBackgrounpOnClick(null);*/
        }

        /// <summary>
        /// 刘海屏适配
        /// </summary>
        private void AdaptiveNotchFit()
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            float topOffseRatio = 0;
            topOffseRatio = GameGlobalSetting.OffsetHeight / GameGlobalSetting.DesignHeight;   
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = new Vector2(1, 1 - topOffseRatio);
            rectTransform.sizeDelta = Vector2.zero;
        }

        public void SetCanvasGroupAlpha(float val)
        {
            if(m_CanvasGroup == null) return;
            m_CanvasGroup.alpha = val;
            //Debug.LogFormat("SetCanvasGroupAlpha:{0} {1}",this.gameObject.name,val);
        }

        /// <summary>
        /// 检测本地化文本是否正确使用
        /// </summary>
        private void CheckLocalization()
        {
            ExText[] ugfTexts = GetComponentsInChildren<ExText>(true);
            for (int i = 0; i < ugfTexts.Length; i++)
            {
                ugfTexts[i].CheckLocalization();
            }
        }
    
        /// <summary>
        /// 上锁后，不会被Release
        /// </summary>
        /// <param name="isLock">锁标记</param>
        public void SetLock(bool isLock)
        {
            AppEntry.UI.SetUIFormInstanceLocked(this.UIForm,isLock);
        }
    
        /// <summary>
        /// 初始化UI绑定
        /// </summary>
        protected virtual void RegisterUI()
        {
        
        }
    
        /// <summary>
        /// 初始化注册全部的事件
        /// </summary>
        protected virtual void RegisterEvent()
        {
       
        }
    
        /// <summary>
        /// 打开子UI Form
        /// </summary>
        /// <param name="viewName">界面枚举</param>
        /// <param name="subUiOrder">子界面的显示层级(相对父界面)</param>
        /// <param name="params">UI参数</param>
        /// <returns></returns>
        public int OpenSubUIForm(string viewName, int subUiOrder = -1, UIParams param = null)
        {
            if (m_SubUIForms == null)
            {
                m_SubUIForms = new List<int>(2);
            }
        
            param ??= UIParams.Create();

            if (subUiOrder <= -1)
            {
                subUiOrder = m_SubUIForms.Count;
            }
        
            param.SortOrder = Depth + subUiOrder + 1;
            param.IsSubUIForm = true;
            var uiformId = AppEntry.UI.OpenSubUI(viewName,this.UIForm.UIGroup.Name);
            m_SubUIForms.Add(uiformId);
        
            return uiformId;
        }
    
        /// <summary>
        /// 关闭子UI Form
        /// </summary>
        /// <param name="uiformId"></param>
        public void CloseSubUIForm(int uiformId)
        {
            if (!m_SubUIForms.Contains(uiformId)) return;
            m_SubUIForms.Remove(uiformId);
            if (AppEntry.UI.HasUIForm(uiformId))
                AppEntry.UI.CloseUIForm(uiformId);
        }
    
        /// <summary>
        /// 关闭全部子UI Form
        /// </summary>
        public void CloseAllSubUIForms()
        {
            if (m_SubUIForms != null)
            {
                for (int i = m_SubUIForms.Count - 1; i >= 0; i--)
                {
                    CloseSubUIForm(m_SubUIForms[i]);
                }
            }
        }

        #region 面板动画

        private UGF_PanelAnimtion m_PanelAnimtion;
        private UGF_PanelAnimtion PanelAnimtion
        {
            get 
            { 
                if (m_PanelAnimtion == null) {

                    m_PanelAnimtion = new UGF_PanelAnimtion();
                }

                return m_PanelAnimtion;
            }
        }


        /// <summary>
        /// 打开动画类型
        /// </summary>
        public PanelAnimtionType OpenAnim = PanelAnimtionType.Null;

        /// <summary>
        /// 关闭动画类型
        /// </summary>
        public PanelAnimtionType CloseAnim = PanelAnimtionType.Null;

        /// <summary>
        /// 若使用Animtor，使用的状态名
        /// </summary>
        public string OpenAnimStateName;

        /// <summary>
        /// 若使用Animtor，使用的状态名
        /// </summary>
        public string CloseAnimStateName;
  
        /// <summary>
        /// 播放打开动画
        /// </summary>
        private void PlayOpenAnim()
        {
            StopAllCoroutines();      
       
            PlayUISound(OpenSoundId);
            FadeIn();       
        
            PanelAnimtion.PlayAnim(this, OpenAnim, () => {
                //m_CallBackListener.CallLua(CallBackType.UIOpenAnimEnd,GetInstanceID());
                URectTransform.anchoredPosition3D = Vector3.zero;
            }, OpenAnimStateName);
        }

        /// <summary>
        /// 播放关闭动画
        /// </summary>
        private void PlayCloseAnim(GameFrameworkAction gameFrameworkAction = null)
        {
            if (!isActiveAndEnabled) return;
            PlayUISound(CloseSoundId);
            PanelAnimtion.PlayAnim(this, CloseAnim, () => {
                gameFrameworkAction?.Invoke();
            }, CloseAnimStateName);                   
        }

        /// <summary>
        /// 设置打开动画开始播放前的回调
        /// </summary>
        /// <param name="luaFunction"></param>
        public void SetOpenAnimStartCallBack(GameFrameworkAction callBack)
        {
            //m_CallBackListener.AddLua(CallBackType.UIOpenAnimStart,m_InstanceId,luaFunction);
        }

        /// <summary>
        /// 设置打开动画播放结束后的回调
        /// </summary>
        /// <param name="luaFunction"></param>
        public void SetOpenAnimEndCallBack(GameFrameworkAction callBack)
        {
            //m_CallBackListener.AddLua(CallBackType.UIOpenAnimEnd,m_InstanceId,luaFunction);
        }

        #endregion
    
    }
}

