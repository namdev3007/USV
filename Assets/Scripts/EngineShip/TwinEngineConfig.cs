using UnityEngine;

namespace CustomTwinEngineShip
{
    [System.Serializable]
    public class TwinEngineConfig
    {
        public string engineName = "Engine";

        [Header("Force")]
        public Transform thrustPoint;
        public Vector3 localThrustDirection = Vector3.forward;
        public float maxForwardThrust = 4000f;
        public float reverseThrustCoefficient = 0.5f;

        [Header("Wave Effect")]
        public bool useWaveHeadingEffect = true;

        [Header("Response")]
        public float throttleResponseTime = 0.25f;

        [Header("Visual")]
        public Transform propellerTransform;
        public float propellerRpmVisual = 1500f;
        public bool invertPropellerRotation = false;

        [HideInInspector] public float currentThrottle;
        [HideInInspector] public float throttleVelocity;
    }
}