using TMPro;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

namespace Editor.UI.ExUI
{
    public class ExUICreateConfig : ScriptableObject
    {
        public int ButtonOnClickId = 0;
        public Selectable.Transition ButtonTransition = Selectable.Transition.None;
        public AnimatorController ButtonAnimatorController;
        public Sprite ButtonDefaultImage2;
        public TMP_FontAsset DefaultFont;

        public Texture2D InputFieldDefaultBgImage;

        public Texture2D SliderDefaultBgImage;
        public Texture2D SliderDefaultBgImage2;
        public Texture2D ScriptIcon;
    }
}