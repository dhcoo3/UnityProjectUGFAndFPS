using System.Collections.Generic;
using Builtin.Scripts.Extension;
using cfg;
using Cysharp.Threading.Tasks;
using Luban;
using UnityEngine.EventSystems;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.Extension
{
    public static class DataTableComponentExtension
    {
        /// <summary>
        /// LuBan配置表
        /// </summary>
        private static cfg.Tables _tables;
        
        /// <summary>
        /// 已加载的配置表字典，键为表名，值为对应的配置表对象
        /// </summary>
        private static readonly Dictionary<string, Tables.IVOFun> _loadedTables = new Dictionary<string, Tables.IVOFun>();

        /// <summary>
        /// 存储已加载的配置表二进制数据，键为表名，值为对应的二进制数据。
        /// </summary>
        private static readonly Dictionary<string, byte[]> _dataTableBytes = new Dictionary<string, byte[]>();
        
        /// <summary>
        /// 配置表
        /// </summary>
        public static cfg.Tables Tables
        {
            get
            {
                return _tables ??= new cfg.Tables();
            }
        }

        public static async UniTask<T> GetDataTableLuBan<T>(this DataTableComponent com,string tableName) where T : Tables.IVOFun
        {
            if (_loadedTables.TryGetValue(tableName, out Tables.IVOFun table))
            {
                return (T)table;
            }
            
            byte[] bytes;
#if UNITY_EDITOR
            bytes = await Builtin.Scripts.Game.AppEntry.Resource.LoadBinaryAwait(AssetPathUtil.GetDataTablePath(tableName, true));
#else
            bytes = Builtin.Scripts.Game.AppEntry.Resource.LoadBinaryFromFileSystem(AssetPathUtil.GetDataTablePath(tableName, true));
#endif
            
            string key = System.IO.Path.GetFileNameWithoutExtension(tableName);
            _dataTableBytes.TryAdd(key,bytes);
            
            if (Tables.CfgDic.TryGetValue(tableName,out Tables.IVOFun source))
            {
                source._LoadData(new ByteBuf(bytes));
                source.ResolveRef(Tables);
                _loadedTables.Add(tableName,source);
            }
            
            return (T)_loadedTables[tableName];
        }
    }
}