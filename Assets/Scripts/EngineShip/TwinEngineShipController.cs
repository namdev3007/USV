using UnityEngine;
using static USVEngine;

namespace CustomTwinEngineShip
{
    [RequireComponent(typeof(Rigidbody))]
    public class TwinEngineShipController : MonoBehaviour
    {
        [Header("References")]
        public Rigidbody targetRigidbody;

        [Header("Input")]
        public UnityJoystickShipInputSource inputSource = new UnityJoystickShipInputSource();

        [Header("Mixer")]
        public DifferentialThrottleMixer mixer = new DifferentialThrottleMixer();

        [Header("Engines")]
        public TwinEngineConfig leftEngine = new TwinEngineConfig { engineName = "Left Engine" };
        public TwinEngineConfig rightEngine = new TwinEngineConfig { engineName = "Right Engine" };

        [Header("Wave Heading Analyzer")]
        public bool enableWaveHeadingEffect = true;
        public CrestWaveHeadingAnalyzer waveHeadingSettings;

        [Header("Stabilization")]
        public bool stabilizeRoll = false;
        public bool stabilizePitch = false;
        public float maxStabilizationAngle = 20f;
        public float rollStabilizationTorque = 3000f;
        public float pitchStabilizationTorque = 2000f;

        [Header("Debug")]
        public bool drawGizmos = true;
        public float leftCommand;
        public float rightCommand;
        public WaveRelativeHeading currentWaveRelativeHeading;
        public float currentWaveAlignmentDot;
        public float currentWaveThrustMultiplier = 1f;

        private RigidbodyTwinEngineMotorDriver _motorDriver;
        private IWaveHeadingAnalyzer _waveHeadingAnalyzer;
        private WaveHeadingData _waveHeadingData;

        private void Reset()
        {
            targetRigidbody = GetComponent<Rigidbody>();
        }

        private void Awake()
        {
            if (targetRigidbody == null)
                targetRigidbody = GetComponent<Rigidbody>();

            _motorDriver = new RigidbodyTwinEngineMotorDriver();

            if (waveHeadingSettings == null)
            {
                waveHeadingSettings = new CrestWaveHeadingAnalyzer(transform);
            }

            _waveHeadingAnalyzer = waveHeadingSettings ?? new CrestWaveHeadingAnalyzer(transform);
        }

        private void Update()
        {
            mixer.controlMode = inputSource.controlMode;

            if (enableWaveHeadingEffect && _waveHeadingAnalyzer != null)
            {
                _waveHeadingData = _waveHeadingAnalyzer.Analyze();
                currentWaveRelativeHeading = _waveHeadingData.relativeHeading;
                currentWaveAlignmentDot = _waveHeadingData.alignmentDot;
                currentWaveThrustMultiplier = _waveHeadingData.thrustMultiplier;
            }
            else
            {
                currentWaveRelativeHeading = WaveRelativeHeading.QuarteringSea;
                currentWaveAlignmentDot = 0f;
                currentWaveThrustMultiplier = 1f;
            }
        }

        private void FixedUpdate()
        {
            if (targetRigidbody == null) return;

            ShipInputData input = inputSource.ReadInput();
            Vector2 output = mixer.Mix(input);

            leftCommand = output.x;
            rightCommand = output.y;

            float dt = Time.fixedDeltaTime;
            float waveMultiplier = enableWaveHeadingEffect ? currentWaveThrustMultiplier : 1f;

            _motorDriver.Apply(targetRigidbody, leftEngine, leftCommand, waveMultiplier, dt);
            _motorDriver.Apply(targetRigidbody, rightEngine, rightCommand, waveMultiplier, dt);

            ApplyStabilization();
        }

        private void ApplyStabilization()
        {
            if (!stabilizeRoll && !stabilizePitch)
                return;

            Vector3 localEuler = transform.localEulerAngles;
            float pitch = NormalizeAngle(localEuler.x);
            float roll = NormalizeAngle(localEuler.z);

            Vector3 localTorque = Vector3.zero;

            if (stabilizePitch)
            {
                float pitchFactor = Mathf.Clamp(pitch / Mathf.Max(0.01f, maxStabilizationAngle), -1f, 1f);
                localTorque.x = -pitchFactor * pitchStabilizationTorque;
            }

            if (stabilizeRoll)
            {
                float rollFactor = Mathf.Clamp(roll / Mathf.Max(0.01f, maxStabilizationAngle), -1f, 1f);
                localTorque.z = -rollFactor * rollStabilizationTorque;
            }

            targetRigidbody.AddRelativeTorque(localTorque, ForceMode.Force);
        }

        private float NormalizeAngle(float angle)
        {
            while (angle > 180f) angle -= 360f;
            while (angle < -180f) angle += 360f;
            return angle;
        }

        public WaveRelativeHeading GetWaveRelativeHeading()
        {
            return currentWaveRelativeHeading;
        }

        public float GetWaveAlignmentDot()
        {
            return currentWaveAlignmentDot;
        }

        public float GetWaveThrustMultiplier()
        {
            return currentWaveThrustMultiplier;
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;

            DrawEngineGizmo(leftEngine, Color.red);
            DrawEngineGizmo(rightEngine, Color.blue);
        }

        private void DrawEngineGizmo(TwinEngineConfig engine, Color color)
        {
            if (engine == null || engine.thrustPoint == null) return;

            Gizmos.color = color;
            Gizmos.DrawSphere(engine.thrustPoint.position, 0.12f);

            Vector3 dir = engine.thrustPoint.TransformDirection(engine.localThrustDirection.normalized);
            Gizmos.DrawRay(engine.thrustPoint.position, dir * 1.2f);
        }
    }
}