using UnityEngine;

namespace CustomTwinEngineShip
{
    [System.Serializable]
    public class RigidbodyTwinEngineMotorDriver
    {
        public void Apply(Rigidbody rb, TwinEngineConfig engine, float targetThrottle, float waveMultiplier, float deltaTime)
        {
            if (rb == null || engine == null || engine.thrustPoint == null)
                return;

            engine.currentThrottle = Mathf.SmoothDamp(
                engine.currentThrottle,
                Mathf.Clamp(targetThrottle, -1f, 1f),
                ref engine.throttleVelocity,
                Mathf.Max(0.01f, engine.throttleResponseTime),
                Mathf.Infinity,
                deltaTime
            );

            float thrust = engine.currentThrottle >= 0f
                ? engine.currentThrottle * engine.maxForwardThrust
                : engine.currentThrottle * engine.maxForwardThrust * engine.reverseThrustCoefficient;

            if (engine.useWaveHeadingEffect)
            {
                thrust *= waveMultiplier;
            }

            Vector3 forceDir = engine.thrustPoint.TransformDirection(engine.localThrustDirection.normalized);
            rb.AddForceAtPosition(forceDir * thrust, engine.thrustPoint.position, ForceMode.Force);

            UpdatePropellerVisual(engine, deltaTime);
        }

        private void UpdatePropellerVisual(TwinEngineConfig engine, float deltaTime)
        {
            if (engine.propellerTransform == null)
                return;

            float rpm = Mathf.Abs(engine.currentThrottle) * engine.propellerRpmVisual;
            float angle = rpm * 6f * deltaTime;

            if (engine.currentThrottle < 0f)
                angle = -angle;

            if (engine.invertPropellerRotation)
                angle = -angle;

            engine.propellerTransform.Rotate(Vector3.forward, angle, Space.Self);
        }
    }
}