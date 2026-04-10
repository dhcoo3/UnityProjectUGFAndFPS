using System.Collections.Generic;
using GameFramework;

namespace HotAssets.Scripts.GamePlay.Logic.Player
{
    public class PlayerData:IReference
    {
        public string Id {get;private set;}
        public string Name{get;private set;}

        public int Pos = 0;
        
        public List<int> HeroIds;

        public static PlayerData Create(string id, string name)
        {
            PlayerData playerData = new PlayerData();
            playerData.Id = id;
            playerData.Name = name;
            return playerData;
        }
        
        public void Clear()
        {
            Id = string.Empty;
            Name = string.Empty;
            Pos = 0;
        }
    }
}