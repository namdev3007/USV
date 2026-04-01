using UnityEngine;

namespace CustomTwinEngineShip
{
    public enum ControlMode
    {
        Mixed,      // 1 ga + 1 lái => mixer tự chia trái/phải
        TwinStick   // mỗi cần điều khiển 1 máy
    }

    [System.Serializable]
    public class UnityJoystickShipInputSource : IShipInputSource
    {
        [Header("Control Mode")]
        public ControlMode controlMode = ControlMode.TwinStick;

        [Header("Mixed Mode Axes")]
        public string throttleAxis = "Vertical";
        public string steeringAxis = "Horizontal";

        [Header("Twin Stick Axes")]
        public string leftEngineAxis = "LeftEngine";
        public string rightEngineAxis = "RightEngine";

        [Header("Invert")]
        public bool invertThrottle;
        public bool invertSteering;
        public bool invertLeftEngine;
        public bool invertRightEngine;

        [Header("Dead Zones")]
        [Range(0f, 0.3f)] public float deadZone = 0.05f;

        public ShipInputData ReadInput()
        {
            if (controlMode == ControlMode.TwinStick)
            {
                float left = ReadAxis(leftEngineAxis, invertLeftEngine);
                float right = ReadAxis(rightEngineAxis, invertRightEngine);

                float throttle = (left + right) * 0.5f;
                float steering = (right - left) * 0.5f;

                return new ShipInputData(throttle, steering, left, right);
            }
            else
            {
                float throttle = ReadAxis(throttleAxis, invertThrottle);
                float steering = ReadAxis(steeringAxis, invertSteering);

                return new ShipInputData(throttle, steering, 0f, 0f);
            }
        }

        private float ReadAxis(string axisName, bool invert)
        {
            if (string.IsNullOrWhiteSpace(axisName))
                return 0f;

            float value = Input.GetAxis(axisName);
            if (invert) value = -value;

            if (Mathf.Abs(value) < deadZone)
                value = 0f;

            return Mathf.Clamp(value, -1f, 1f);
        }
    }
}