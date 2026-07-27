using UnityEngine;
using System.Collections.Generic;

namespace AnimationInstancing
{
    [System.Serializable]
    public class AnimationClipData
    {
        public string name;
        public int startFrame;
        public int endFrame;
        public bool loop;
    }

    [CreateAssetMenu(fileName = "NewAnimationData", menuName = "Animation Instancing/Animation Data")]
    public class AnimationDataAsset : ScriptableObject
    {
        [Header("Texture Data")]
        public Texture2D animationTexture;
        public int boneCount;
        public int totalFrames;
        public float fps = 30f;

        [Header("Clips")]
        public List<AnimationClipData> clips = new List<AnimationClipData>();

        public AnimationClipData GetClip(string clipName)
        {
            return clips.Find(c => c.name == clipName);
        }
    }
}
