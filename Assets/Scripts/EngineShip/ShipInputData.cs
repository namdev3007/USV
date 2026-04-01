using UnityEngine;

namespace CustomTwinEngineShip
{
    [System.Serializable]
    public struct ShipInputData
    {
        public float throttle;     // ga tổng: -1..1
        public float steering;     // lái: -1..1
        public float leftThrottle; // ga máy trái: -1..1
        public float rightThrottle;// ga máy phải: -1..1

        public ShipInputData(float throttle, float steering, float leftThrottle, float rightThrottle)
        {
            this.throttle = Mathf.Clamp(throttle, -1f, 1f);
            this.steering = Mathf.Clamp(steering, -1f, 1f);
            this.leftThrottle = Mathf.Clamp(leftThrottle, -1f, 1f);
            this.rightThrottle = Mathf.Clamp(rightThrottle, -1f, 1f);
        }
    }
}