using System;
using UnityEngine;

namespace CinematicCameraPro
{
    [Serializable]
    public class CinematicShot
    {
        const float MinTransitionDuration = 0.01f;
        const float SmartHandoffFreeAngle = 45f;
        const float SmartHandoffBlockedAngle = 120f;

        public string name = "Shot";
        public bool enabled = true;
        public CinematicPath path = new CinematicPath();
        public Transform lookAtTarget;
        public Transform secondaryLookAtTarget;
        public float primaryLookAtDuration = 3f;
        public float lookAtTransitionDuration = 0.35f;
        public float lookAtLeadTime = 0.35f;
        public float lookAtRotationSmoothTime = 0.18f;
        public float maxLookAtTurnSpeed = 180f;
        public float duration = 3f;
        public bool mirrorPathFacing = false;
        public float pathFacingYawOffset = 0f;
        
        public float Duration => Mathf.Max(0f, duration);

        public CinematicShot() 
        {
            path = new CinematicPath();
        }

        public CinematicShot(string shotName)
        {
            name = shotName;
            path = new CinematicPath();
        }

        public bool HasLookAtTargets => lookAtTarget != null || secondaryLookAtTarget != null;

        public float SecondaryLookAtDuration => Mathf.Max(0f, Duration - GetPrimaryLookAtDuration());

        public float GetPrimaryLookAtDuration()
        {
            if (secondaryLookAtTarget == null)
            {
                return Duration;
            }

            return Mathf.Clamp(primaryLookAtDuration, 0f, Duration);
        }

        public float GetLookAtTransitionDuration()
        {
            if (lookAtTarget == null || secondaryLookAtTarget == null)
            {
                return 0f;
            }

            float maxTransition = Mathf.Min(GetPrimaryLookAtDuration(), SecondaryLookAtDuration);
            return Mathf.Clamp(lookAtTransitionDuration, 0f, Mathf.Max(MinTransitionDuration, maxTransition));
        }

        public Quaternion EvaluateLookAtRotation(Vector3 position, float normalizedTime)
        {
            Transform primaryTarget = lookAtTarget;
            Transform secondaryTarget = secondaryLookAtTarget;

            if (primaryTarget == null && secondaryTarget == null)
            {
                return Quaternion.identity;
            }

            if (primaryTarget == null)
            {
                primaryTarget = secondaryTarget;
                secondaryTarget = null;
            }

            Quaternion primaryRotation = CreateLookRotation(position, primaryTarget);
            if (secondaryTarget == null)
            {
                return primaryRotation;
            }

            float shotDuration = Mathf.Max(Duration, MinTransitionDuration);
            float currentTime = Mathf.Clamp01(normalizedTime) * shotDuration;
            float firstTargetDuration = GetPrimaryLookAtDuration();
            float transitionDuration = GetLookAtTransitionDuration();
            float secondTargetDuration = SecondaryLookAtDuration;

            float intendedBlend = 0f;
            if (transitionDuration <= MinTransitionDuration)
            {
                intendedBlend = currentTime < firstTargetDuration ? 0f : 1f;
            }
            else
            {
                Vector3 currentPrimaryDirection = GetTargetDirection(position, primaryTarget);
                Vector3 currentSecondaryDirection = GetTargetDirection(position, secondaryTarget);
                float angle = GetTargetAngle(currentPrimaryDirection, currentSecondaryDirection);
                float angleFactor = Mathf.InverseLerp(SmartHandoffFreeAngle, SmartHandoffBlockedAngle, angle);
                float configuredLeadTime = Mathf.Max(0f, lookAtLeadTime);
                float preBlendDuration = Mathf.Min(
                    firstTargetDuration,
                    Mathf.Max(configuredLeadTime, transitionDuration * Mathf.Lerp(1.6f, 2.4f, angleFactor)));
                float postBlendDuration = Mathf.Min(secondTargetDuration, transitionDuration * Mathf.Lerp(0.6f, 1.0f, 1f - angleFactor));

                float blendStart = Mathf.Clamp(firstTargetDuration - preBlendDuration, 0f, shotDuration);
                float blendEnd = Mathf.Clamp(firstTargetDuration + postBlendDuration, 0f, shotDuration);

                if (currentTime <= blendStart)
                {
                    intendedBlend = 0f;
                }
                else if (currentTime >= blendEnd)
                {
                    intendedBlend = 1f;
                }
                else
                {
                    intendedBlend = Mathf.InverseLerp(blendStart, blendEnd, currentTime);
                    intendedBlend = SmootherStep(intendedBlend);
                }
            }

            Vector3 primaryDirection = GetTargetDirection(position, primaryTarget);
            Vector3 secondaryDirection = GetTargetDirection(position, secondaryTarget);
            float smartGate = EvaluateSmartHandoffGate(primaryDirection, secondaryDirection);
            float postHandoffRelease = SmootherStep(Mathf.InverseLerp(firstTargetDuration, shotDuration, currentTime));
            float gateWeight = Mathf.Lerp(smartGate, 1f, postHandoffRelease);
            float finalBlend = Mathf.Clamp01(intendedBlend * gateWeight);

            if (finalBlend <= 0.0001f)
            {
                return primaryRotation;
            }

            if (finalBlend >= 0.9999f)
            {
                return CreateLookRotation(position, secondaryTarget);
            }

            return CreateBlendedLookRotation(position, primaryTarget, secondaryTarget, finalBlend);
        }

        static Quaternion CreateLookRotation(Vector3 position, Transform target)
        {
            if (target == null)
            {
                return Quaternion.identity;
            }

            Vector3 direction = target.position - position;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return Quaternion.identity;
            }

            return Quaternion.LookRotation(direction.normalized, Vector3.up);
        }

        static Vector3 GetTargetDirection(Vector3 position, Transform target)
        {
            if (target == null)
            {
                return Vector3.zero;
            }

            Vector3 direction = target.position - position;
            return direction.sqrMagnitude <= 0.0001f ? Vector3.zero : direction.normalized;
        }

        static float GetTargetAngle(Vector3 primaryDirection, Vector3 secondaryDirection)
        {
            if (primaryDirection.sqrMagnitude <= 0.0001f || secondaryDirection.sqrMagnitude <= 0.0001f)
            {
                return 0f;
            }

            return Vector3.Angle(primaryDirection, secondaryDirection);
        }

        static Quaternion CreateBlendedLookRotation(Vector3 position, Transform primaryTarget, Transform secondaryTarget, float blendT)
        {
            if (primaryTarget == null)
            {
                return CreateLookRotation(position, secondaryTarget);
            }

            if (secondaryTarget == null)
            {
                return CreateLookRotation(position, primaryTarget);
            }

            Vector3 primaryPosition = primaryTarget.position;
            Vector3 secondaryPosition = secondaryTarget.position;
            Vector3 blendedTargetPosition = Vector3.Lerp(primaryPosition, secondaryPosition, Mathf.Clamp01(blendT));
            Vector3 blendedDirection = blendedTargetPosition - position;

            if (blendedDirection.sqrMagnitude > 0.0001f)
            {
                return Quaternion.LookRotation(blendedDirection.normalized, Vector3.up);
            }

            return blendT < 0.5f
                ? CreateLookRotation(position, primaryTarget)
                : CreateLookRotation(position, secondaryTarget);
        }

        static float EvaluateSmartHandoffGate(Vector3 primaryDirection, Vector3 secondaryDirection)
        {
            if (primaryDirection.sqrMagnitude <= 0.0001f || secondaryDirection.sqrMagnitude <= 0.0001f)
            {
                return 1f;
            }

            float angle = Vector3.Angle(primaryDirection, secondaryDirection);
            if (angle <= SmartHandoffFreeAngle)
            {
                return 1f;
            }

            if (angle >= SmartHandoffBlockedAngle)
            {
                return 0f;
            }

            float t = 1f - Mathf.InverseLerp(SmartHandoffFreeAngle, SmartHandoffBlockedAngle, angle);
            return SmootherStep(t);
        }

        static float SmootherStep(float t)
        {
            t = Mathf.Clamp01(t);
            return t * t * t * (t * (t * 6f - 15f) + 10f);
        }

        public CinematicShot Clone()
        {
            return new CinematicShot
            {
                name = name,
                enabled = enabled,
                path = path.Clone(),
                lookAtTarget = lookAtTarget,
                secondaryLookAtTarget = secondaryLookAtTarget,
                primaryLookAtDuration = primaryLookAtDuration,
                lookAtTransitionDuration = lookAtTransitionDuration,
                lookAtLeadTime = lookAtLeadTime,
                lookAtRotationSmoothTime = lookAtRotationSmoothTime,
                maxLookAtTurnSpeed = maxLookAtTurnSpeed,
                duration = duration,
                mirrorPathFacing = mirrorPathFacing,
                pathFacingYawOffset = pathFacingYawOffset
            };
        }
    }
}
