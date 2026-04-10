using System;
using GameFramework;
using HotAssets.Scripts.GamePlay.Logic.GameInput;

namespace AAAGame.ScriptsHotfix.GamePlay.Logic.Frame
{
    public class FrameData:IReference
    {
        public int Index;
        public string PlayerId;
        public float Horizontal;
        public float Vertica;

        public static FrameData Create(int index)
        {
            FrameData frameData = ReferencePool.Acquire<FrameData>();
            frameData.PlayerId = "0";
            frameData.Index = index;
            return frameData;
        }

        public string Pack()
        {
            return $"{Index}|{PlayerId}|{Horizontal}|{Vertica}|";
        }

        public static InputObj UnPack(string packString)
        {
            /*string[] pack = packString.Split('|');
            InputObj inputObj = InputObj.Create(float.Parse(pack[2]), bool.Parse(pack[3]));
            inputObj.PlayerId = pack[1];*/
            return null;
        }
        
        public void Clear()
        {
            PlayerId = String.Empty;
            Index = 0;
            Horizontal = 0;
            Vertica = 0;
        }
    }
}