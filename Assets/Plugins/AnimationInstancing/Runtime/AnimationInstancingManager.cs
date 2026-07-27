using UnityEngine;
using System.Collections.Generic;

namespace AnimationInstancing
{
    [System.Serializable]
    public struct ConfigMapping
    {
        public string key;
        public AnimationInstancingConfig config;
    }

    [DefaultExecutionOrder(-100)]
    public class AnimationInstancingManager : MonoBehaviour
    {
        public static AnimationInstancingManager Instance { get; private set; }

        public AnimationInstancingConfig config;
        public List<ConfigMapping> characterConfigs;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public AnimationInstancingConfig GetConfigForCharacter(string name)
        {
            if (characterConfigs == null) return null;
            foreach (var mapping in characterConfigs)
            {
                if (!string.IsNullOrEmpty(mapping.key) && name.Contains(mapping.key))
                {
                    return mapping.config;
                }
            }
            return null;
        }
    }
}
