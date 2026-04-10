using UnityEngine;
using UnityGameFramework.Runtime;

namespace Builtin.Scripts.Game
{
    public partial class AppEntry : MonoBehaviour
    {
        public static Canvas RootCanvas { get; private set; } = null;
        public static Camera UICamera { get; private set; }
        
        public static RectTransform UIRoot;
    
        public static Camera MainCamera;
        
        // Start is called before the first frame update
        void Start()
        {
            InitFramework();
            
            RootCanvas = UI.GetComponentInChildren<Canvas>();
            UICamera = RootCanvas.worldCamera;
            MainCamera = Camera.main;
            UIRoot = RootCanvas.GetComponent<RectTransform>();
        }
    
        public static void Shutdown(ShutdownType type)
        {
            GameEntry.Shutdown(type);
        }
    }
}
