using System;
using UnityEngine;

namespace CinematicCameraPro
{
    [Serializable]
    public class CameraTrackClip
    {
        public string name = "Clip";
        public Camera sourceCamera;
        public bool useEmbeddedShot = false;
        public CinematicShot embeddedShot = new CinematicShot("Embedded Shot");
        public float startTime = 0f;
        public float duration = 3f;

        public float Duration => Mathf.Max(0f, duration);
        public float EndTime => startTime + Duration;
    }
}
