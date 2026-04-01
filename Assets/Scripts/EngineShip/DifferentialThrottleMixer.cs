using UnityEngine;

namespace CustomTwinEngineShip
{
    [System.Serializable]
    public class DifferentialThrottleMixer : IThrottleMixer
    {
        [Header("General")]
        public ControlMode controlMode = ControlMode.TwinStick;

        [Header("Mixed Mode")]
        [Range(0f, 2f)] public float steeringSensitivity = 1f;
        public bool clampOutput = true;

        [Header("Pivot Turning")]
        public bool allowInnerEngineReverseForPivot = false;
        [Range(0f, 1f)] public float pivotReverseStrength = 0.35f;

        public Vector2 Mix(ShipInputData input)
        {
            if (controlMode == ControlMode.TwinStick)
            {
                return new Vector2(
                    Mathf.Clamp(input.leftThrottle, -1f, 1f),
                    Mathf.Clamp(input.rightThrottle, -1f, 1f)
                );
            }

            float throttle = Mathf.Clamp(input.throttle, -1f, 1f);
            float steering = Mathf.Clamp(input.steering, -1f, 1f) * steeringSensitivity;

            float left = throttle - steering;
            float right = throttle + steering;

            if (allowInnerEngineReverseForPivot && Mathf.Abs(throttle) < 0.15f && Mathf.Abs(steering) > 0.15f)
            {
                float pivot = Mathf.Abs(steering) * pivotReverseStrength;

                if (steering < 0f)
                {
                    left = -pivot;
                    right = pivot;
                }
                else
                {
                    left = pivot;
                    right = -pivot;
                }
            }

            if (clampOutput)
            {
                float max = Mathf.Max(1f, Mathf.Abs(left), Mathf.Abs(right));
                left /= max;
                right /= max;
            }

            return new Vector2(
                Mathf.Clamp(left, -1f, 1f),
                Mathf.Clamp(right, -1f, 1f)
            );
        }
    }
}