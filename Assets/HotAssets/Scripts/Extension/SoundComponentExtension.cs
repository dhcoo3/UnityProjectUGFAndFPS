using System.Collections.Generic;
using Builtin.Scripts.Extension;
using Builtin.Scripts.Game;
using GameFramework;
using HotAssets.Scripts.GamePlay.Logic.Common;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.Extension
{
    public static class SoundComponentExtension
    {
        private static Dictionary<string, float> _lastPlayEffectTags = new Dictionary<string, float>();
        
        /// <summary>
        /// 播放背景音乐
        /// </summary>
        /// <param name="soundCom"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static int PlayBGM(this SoundComponent soundCom, string name)
        {
            return soundCom.PlaySound(name, nameof(GamePlayDefine.SoundGroup.Music), Vector3.zero, true);
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="soundCom"></param>
        /// <param name="name"></param>
        /// <param name="group"></param>
        /// <param name="worldPos"></param>
        /// <param name="isLoop"></param>
        /// <returns></returns>
        public static int PlaySound(this SoundComponent soundCom, string name, string group, Vector3 worldPos, bool isLoop = false)
        {
            string assetName = AssetPathUtil.GetSoundPath(name);
            //TODO 临时资源存在判定
            if (AppEntry.Resource.HasAsset(assetName) == GameFramework.Resource.HasAssetResult.NotExist) return 0;
            var parms = ReferencePool.Acquire<GameFramework.Sound.PlaySoundParams>();
            parms.Clear();
            parms.Loop = isLoop;
            return soundCom.PlaySound(assetName, group, 0, parms, worldPos);
        }
        
        public static int PlayEffect(this SoundComponent soundCom, string name, bool isLoop = false)
        {
            return soundCom.PlaySound(name, nameof(GamePlayDefine.SoundGroup.Sound), Vector3.zero, isLoop);
        }
        
        public static void PlayEffect(this SoundComponent soundCom, string name, float interval)
        {
            bool hasKey = _lastPlayEffectTags.ContainsKey(name);
            if (hasKey && Time.time - _lastPlayEffectTags[name] < interval)
            {
                return;
            }
            soundCom.PlaySound(name, nameof(GamePlayDefine.SoundGroup.Sound), Vector3.zero, false);
            if (hasKey) _lastPlayEffectTags[name] = Time.time;
            else _lastPlayEffectTags.Add(name, Time.time);
        }
    }
}