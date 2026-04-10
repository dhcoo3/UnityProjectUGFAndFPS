using Builtin.Scripts.Component;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace Builtin.Scripts.Game
{
    public partial class AppEntry : MonoBehaviour
    {
        public static BaseComponent Base { get; private set; }
        public static ConfigComponent Config { get; private set; }
        public static DataNodeComponent DataNode { get; private set; }
        public static DataTableComponent DataTable { get; private set; }
        public static DebuggerComponent Debugger { get; private set; }
        public static DownloadComponent Download { get; private set; }
        public static EntityComponent Entity { get; private set; }
        public static EventComponent Event { get; private set; }
        public static FsmComponent Fsm { get; private set; }
        public static FileSystemComponent FileSystem { get; private set; }
        public static LocalizationComponent Localization { get; private set; }
        public static NetworkComponent Network { get; private set; }
        public static ProcedureComponent Procedure { get; private set; }
        public static ResourceComponent Resource { get; private set; }
        public static SceneComponent Scene { get; private set; }
        public static SettingComponent Setting { get; private set; }
        public static SoundComponent Sound { get; private set; }
        public static UIComponent UI { get; private set; }
        public static ObjectPoolComponent ObjectPool { get; private set; }
        public static WebRequestComponent WebRequest { get; private set; }
        public static BuiltinViewComponent BuiltinView { get; private set; }
        
        private static void InitFramework()
        {
            Base = GameEntry.GetComponent<BaseComponent>();
            Config = GameEntry.GetComponent<ConfigComponent>();
            DataNode = GameEntry.GetComponent<DataNodeComponent>();
            DataTable = GameEntry.GetComponent<DataTableComponent>();
            Debugger = GameEntry.GetComponent<DebuggerComponent>();
            Download = GameEntry.GetComponent<DownloadComponent>();
            Entity = GameEntry.GetComponent<EntityComponent>();
            Event = GameEntry.GetComponent<EventComponent>();
            Fsm = GameEntry.GetComponent<FsmComponent>();
            Procedure = GameEntry.GetComponent<ProcedureComponent>();
            Localization = GameEntry.GetComponent<LocalizationComponent>();
            Network = GameEntry.GetComponent<NetworkComponent>();
            Resource = GameEntry.GetComponent<ResourceComponent>();
            FileSystem = GameEntry.GetComponent<FileSystemComponent>();
            Scene = GameEntry.GetComponent<SceneComponent>();
            Setting = GameEntry.GetComponent<SettingComponent>();
            Sound = GameEntry.GetComponent<SoundComponent>();
            UI = GameEntry.GetComponent<UIComponent>();
            ObjectPool = GameEntry.GetComponent<ObjectPoolComponent>();
            WebRequest = GameEntry.GetComponent<WebRequestComponent>();
            BuiltinView = GameEntry.GetComponent<BuiltinViewComponent>();
        }
    }
}
