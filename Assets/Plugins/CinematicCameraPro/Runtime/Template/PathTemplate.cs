using System;
using UnityEngine;

namespace CinematicCameraPro
{
    [Serializable]
    public class PathTemplate
    {
        public string name;
        public string description;
        public PathType defaultPathType = PathType.CatmullRom;
        public EasingType defaultEasing = EasingType.EaseOut;
        public Vector3[] keyframeOffsets;
        public float[] keyframeTimes;
        public float defaultDuration = 3f;
        public bool lookAtTargetRequired;
        public string category;

        public CinematicShot GenerateShot(Transform target = null, float duration = 0f)
        {
            if (duration <= 0f) duration = defaultDuration;

            var shot = new CinematicShot(name);
            shot.path.pathType = defaultPathType;
            shot.path.SetEasingPreset(defaultEasing);
            shot.lookAtTarget = target;
            shot.duration = duration;

            Vector3 basePosition = target != null ? target.position : Vector3.zero;

            for (int i = 0; i < keyframeOffsets.Length; i++)
            {
                float time = keyframeTimes.Length > i ? keyframeTimes[i] * duration : (float)i / (keyframeOffsets.Length - 1) * duration;
                var kf = new PathPoint(basePosition + keyframeOffsets[i], time);
                shot.path.keyframes.Add(kf);
            }

            shot.path.NormalizeKeyframeTimes();
            shot.path.AutoCalculateTangents();
            return shot;
        }
    }

    public static class BuiltInTemplates
    {
        public static PathTemplate[] GetAll()
        {
            return new PathTemplate[]
            {
                CreateLinear(),
                CreateOrbit(),
                CreateFocusPush(),
                CreateFlythrough(),
                CreateFigure8(),
                CreateArc()
            };
        }

        static PathTemplate CreateLinear()
        {
            return new PathTemplate
            {
                name = "Linear",
                description = "Simple start to end movement",
                defaultPathType = PathType.Linear,
                keyframeOffsets = new Vector3[]
                {
                    new Vector3(0, 0, -5),
                    new Vector3(0, 0, 5)
                },
                keyframeTimes = new float[] { 0, 1 },
                category = "Basic"
            };
        }

        static PathTemplate CreateOrbit()
        {
            return new PathTemplate
            {
                name = "Orbit",
                description = "Circular orbit around target",
                defaultPathType = PathType.CatmullRom,
                keyframeOffsets = new Vector3[]
                {
                    new Vector3(5, 0, 0),
                    new Vector3(0, 2, 5),
                    new Vector3(-5, 0, 0),
                    new Vector3(0, -2, 5)
                },
                keyframeTimes = new float[] { 0, 0.33f, 0.66f, 1f },
                defaultDuration = 4f,
                lookAtTargetRequired = true,
                category = "Cinematic"
            };
        }

        static PathTemplate CreateFocusPush()
        {
            return new PathTemplate
            {
                name = "Focus Push",
                description = "Push in to focus on target",
                defaultPathType = PathType.CatmullRom,
                keyframeOffsets = new Vector3[]
                {
                    new Vector3(0, 2, -8),
                    new Vector3(0, 1, -4),
                    new Vector3(0, 0.5f, -1)
                },
                keyframeTimes = new float[] { 0, 0.5f, 1f },
                defaultDuration = 3f,
                lookAtTargetRequired = true,
                category = "Cinematic"
            };
        }

        static PathTemplate CreateFlythrough()
        {
            return new PathTemplate
            {
                name = "Flythrough",
                description = "Fly through the scene",
                defaultPathType = PathType.CatmullRom,
                keyframeOffsets = new Vector3[]
                {
                    new Vector3(-10, 5, -10),
                    new Vector3(0, 3, -5),
                    new Vector3(10, 2, 0),
                    new Vector3(5, 4, 10)
                },
                keyframeTimes = new float[] { 0, 0.33f, 0.66f, 1f },
                defaultDuration = 5f,
                category = "Cinematic"
            };
        }

        static PathTemplate CreateFigure8()
        {
            return new PathTemplate
            {
                name = "Figure 8",
                description = "Figure 8 movement pattern",
                defaultPathType = PathType.CatmullRom,
                keyframeOffsets = new Vector3[]
                {
                    new Vector3(5, 0, 0),
                    new Vector3(0, 0, 5),
                    new Vector3(-5, 0, 0),
                    new Vector3(0, 0, -5),
                    new Vector3(5, 0, 0)
                },
                keyframeTimes = new float[] { 0, 0.25f, 0.5f, 0.75f, 1f },
                defaultDuration = 4f,
                lookAtTargetRequired = true,
                category = "Cinematic"
            };
        }

        static PathTemplate CreateArc()
        {
            return new PathTemplate
            {
                name = "Arc",
                description = "Arc movement around target",
                defaultPathType = PathType.CatmullRom,
                keyframeOffsets = new Vector3[]
                {
                    new Vector3(-5, 1, -3),
                    new Vector3(0, 2, 0),
                    new Vector3(5, 1, -3)
                },
                keyframeTimes = new float[] { 0, 0.5f, 1f },
                defaultDuration = 2f,
                lookAtTargetRequired = true,
                category = "Cinematic"
            };
        }
    }
}
