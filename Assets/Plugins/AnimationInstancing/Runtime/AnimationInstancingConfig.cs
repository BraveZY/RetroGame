using UnityEngine;

namespace AnimationInstancing
{
    [CreateAssetMenu(fileName = "AnimationInstancingConfig", menuName = "Animation Instancing/Config")]
    public class AnimationInstancingConfig : ScriptableObject
    {
        [Header("Global Color Settings")]
        public Texture2D colorMask;

        [Header("Channel R (Red)")]
        public bool randomizeR = true;
        public Color[] randomColorsR;

        [Header("Channel G (Green)")]
        public bool randomizeG = true;
        public Color[] randomColorsG;

        [Header("Channel B (Blue)")]
        public bool randomizeB = true;
        public Color[] randomColorsB;

        [Header("Animation Randomness")]
        public bool enableGlobalRandomness = true;
        public bool randomizeStartTime = false;
        public bool randomizeSpeed = false;
        public Vector2 speedRange = new Vector2(0.9f, 1.1f);
    }
}
