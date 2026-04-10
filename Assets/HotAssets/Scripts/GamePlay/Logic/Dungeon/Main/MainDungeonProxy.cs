using System;
using Builtin.Scripts.Game;
using cfg.Chapter;
using HotAssets.Scripts.Extension;
using HotAssets.Scripts.GamePlay.Logic.Map;
using HotAssets.Scripts.GamePlay.Logic.ProxyManager;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.GamePlay.Logic.Dungeon.Main
{
    public class MainDungeonProxy:GameProxy,IDungeon
    {
        private MapProxy _mapProxy;
        private TbMainChapter _tbMainChapter;
        
        public override async void Initialize()
        {
            try
            {
                _tbMainChapter = await AppEntry.DataTable.GetDataTableLuBan<TbMainChapter>(cfg.Tables.chapter_tbmainchapter);
            }
            catch (Exception e)
            {
                Log.Error("Initialize error = {0}",e.Message);
            }
            
            _mapProxy = GetProxy<MapProxy>();
        }

        public void InitDungeon(int dungeonID)
        {
            MainChapter chapterData = _tbMainChapter.GetOrDefault(dungeonID);
            if (chapterData == null)
            {
                Log.Error("不存在主线关卡数据 {0}",dungeonID);
                return;
            }
            _mapProxy.LoadMap(chapterData.MapId);
        }

        public override void Clear()
        {            
            base.Clear();
        }
    }
}