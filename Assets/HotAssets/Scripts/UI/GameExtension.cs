using System;
using System.Collections.Generic;
using Builtin.Scripts.Game;
using GameFramework;
using HotAssets.Scripts.CoreComponent;
using HotAssets.Scripts.GameNetwork;
using Unity.Burst;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.UI
{
    public class GameExtension:AppEntry
    {
        public static string UserId = "9527";
        public static VariablePoolComponent VariablePool { get; private set; }
        public static GameNetworkComponent GameNetwork { get; private set; }
        
        public static readonly List<GameFrameworkAction<float>> FixedUpdateActions = new List<GameFrameworkAction<float>>();
    
        public static readonly List<GameFrameworkAction<float>> UpdateActions = new List<GameFrameworkAction<float>>();
    
        private void Start()
        {
            VariablePool = GameEntry.GetComponent<VariablePoolComponent>();
            GameNetwork = GameEntry.GetComponent<GameNetworkComponent>();
        }

        private void OnApplicationQuit()
        {
            OnExitGame();
        }

        private void OnApplicationPause(bool pause)
        {
            //Log.Info("OnApplicationPause:{0}", pause);
            if (Application.isMobilePlatform && pause)
            {
                OnExitGame();
            }
        }

        [BurstCompile]
        private void FixedUpdate()
        {
            foreach (var action in FixedUpdateActions)
            {
                action.Invoke(Time.fixedDeltaTime);
            }
        }

        [BurstCompile]
        private void Update()
        {
            foreach (var action in UpdateActions)
            {
                action.Invoke(Time.deltaTime);
            }
        }

        private void OnExitGame()
        {        
            var exitTime = DateTime.UtcNow.ToString();
            UnityGameFramework.Runtime.Log.Info("Application Quit:{0}", exitTime);
        }
    }
}
