using UnityEngine;
using Crest;
using Range = UnityEngine.RangeAttribute;

[RequireComponent(typeof(Rigidbody))]
public class USVEngine : MonoBehaviour, IShutdownable
{
    public enum WaveRelativeHeading
    {
        FollowingSea,
        HeadSea,
        BeamSea,
        QuarteringSea
    }

    [System.Serializable]
    public class Engine
    {
        public string name;
        public bool isOn = true;

        [Header("RPM / Thrust")]
        public float minRPM = 800f;
        public float maxRPM = 5000f;
        public float spinUpTime = 0.4f;
        public float spinDownTime = 0.25f;
        public float maxThrust = 4000f;
        public float reverseCoefficient = 0.5f;
        public float maxSpeed = 10f;
        public AnimationCurve thrustCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Transforms")]
        public Transform thrustPoint;
        public Transform propellerCheckPoint;
        public Transform propeller;
        public float propellerRpmRatio = 0.1f;

        [Header("Water Contact")]
        public float fullSubmergenceDepth = 0.25f;
        [Range(0f, 1f)] public float ventilationMinFactor = 0f;

        [HideInInspector] public float currentRPM;
        [HideInInspector] public float throttleInput; // âm = tiến, dương = lùi
        [HideInInspector] public float currentThrust;

        [HideInInspector] public float waterHeight;
        [HideInInspector] public float submergence;
        [HideInInspector] public float submergenceFactor;
        [HideInInspector] public bool isSubmerged;
        [HideInInspector] public Vector3 waterFlow;
        [HideInInspector] public Vector3 relativeVelocityToWater;
    }

    [Header("Engines")]
    public Engine leftEngine;
    public Engine rightEngine;

    [Header("General")]
    public float waterAngularDamping = 1.0f;
    public float minSpatialLength = 1.5f;

    [Header("Helm / Steering")]
    [Tooltip("Độ lệch lực khi đang tiến.")]
    [Range(0f, 1f)] public float forwardSteerStrength = 0.75f;

    [Tooltip("Độ lệch lực khi đang lùi.")]
    [Range(0f, 1f)] public float reverseSteerStrength = 0.45f;

    [Tooltip("Lực tiến khi chỉ bấm A/D.")]
    [Range(0f, 1f)] public float idleTurnForwardThrottle = 0.65f;

    [Tooltip("Tốc độ làm mượt lệnh động cơ.")]
    public float commandResponseSpeed = 8f;

    [Tooltip("Ngưỡng chống rung input.")]
    [Range(0f, 0.2f)] public float inputDeadZone = 0.05f;

    [Header("Speed / Efficiency")]
    public AnimationCurve speedEfficiencyCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    public float axialFlowInfluence = 0.05f;

    [Header("Wave Heading Effect")]
    [Range(0f, 1f)] public float headFollowingThreshold = 0.7f;
    [Range(0f, 1f)] public float maxHeadSeaThrustPenalty = 0.35f;
    [Range(0f, 1f)] public float maxBeamSeaThrustPenalty = 0.1f;
    [Range(0f, 0.5f)] public float maxFollowingSeaThrustBoost = 0.08f;

    [Header("Debug")]
    public bool drawThrustGizmos = true;
    public bool drawWaterDebug = true;
    public bool drawWaveHeadingDebug = true;

    private Rigidbody _rb;

    private readonly SampleHeightHelper _leftHeightHelper = new SampleHeightHelper();
    private readonly SampleHeightHelper _rightHeightHelper = new SampleHeightHelper();

    private readonly SampleFlowHelper _leftFlowHelper = new SampleFlowHelper();
    private readonly SampleFlowHelper _rightFlowHelper = new SampleFlowHelper();

    private Vector3 _waveDirection = Vector3.forward;
    private float _waveAlignmentDot = 0f;
    private float _headSeaFactor = 0f;
    private float _beamSeaFactor = 0f;
    private float _followingSeaFactor = 0f;
    private WaveRelativeHeading _waveRelativeHeading = WaveRelativeHeading.QuarteringSea;

    private float _leftCommand;
    private float _rightCommand;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.angularDamping = waterAngularDamping;
    }

    private void Update()
    {
        float rawThrottle = Input.GetAxis("Vertical");   // W = +1, S = -1
        float rawSteer = Input.GetAxis("Horizontal");    // A = -1, D = +1

        float throttle = ApplyDeadZone(rawThrottle);
        float steer = ApplyDeadZone(rawSteer);

        ComputeEngineCommands(throttle, steer, out float targetLeft, out float targetRight);

        _leftCommand = Mathf.MoveTowards(_leftCommand, targetLeft, commandResponseSpeed * Time.deltaTime);
        _rightCommand = Mathf.MoveTowards(_rightCommand, targetRight, commandResponseSpeed * Time.deltaTime);

        leftEngine.throttleInput = _leftCommand;
        rightEngine.throttleInput = _rightCommand;

        UpdateWaveHeadingState();

        UpdateWaterState(leftEngine, _leftHeightHelper, _leftFlowHelper);
        UpdateWaterState(rightEngine, _rightHeightHelper, _rightFlowHelper);

        UpdatePropeller(leftEngine);
        UpdatePropeller(rightEngine);
    }

    private void FixedUpdate()
    {
        UpdateEngine(leftEngine);
        UpdateEngine(rightEngine);
    }

    private float ApplyDeadZone(float value)
    {
        return Mathf.Abs(value) < inputDeadZone ? 0f : Mathf.Clamp(value, -1f, 1f);
    }

    /// <summary>
    /// Input người chơi:
    /// W = throttle dương
    /// S = throttle âm
    ///
    /// Nội bộ động cơ:
    /// âm = tiến
    /// dương = lùi
    /// </summary>
    private void ComputeEngineCommands(float throttle, float steer, out float left, out float right)
    {
        left = 0f;
        right = 0f;

        bool hasThrottle = Mathf.Abs(throttle) > 0.001f;
        bool hasSteer = Mathf.Abs(steer) > 0.001f;

        if (hasThrottle)
        {
            if (throttle > 0f)
            {
                // Tiến
                float baseForward = -throttle;

                if (steer < 0f) // A = quay trái => máy phải mạnh hơn
                {
                    left = baseForward * (1f - forwardSteerStrength);
                    right = baseForward;
                }
                else if (steer > 0f) // D = quay phải => máy trái mạnh hơn
                {
                    left = baseForward;
                    right = baseForward * (1f - forwardSteerStrength);
                }
                else
                {
                    left = baseForward;
                    right = baseForward;
                }

                left = Mathf.Clamp(left, -1f, 0f);
                right = Mathf.Clamp(right, -1f, 0f);
            }
            else
            {
                // Lùi
                float baseReverse = -throttle;

                if (steer < 0f) // quay trái khi lùi
                {
                    left = baseReverse * (1f - reverseSteerStrength);
                    right = baseReverse;
                }
                else if (steer > 0f) // quay phải khi lùi
                {
                    left = baseReverse;
                    right = baseReverse * (1f - reverseSteerStrength);
                }
                else
                {
                    left = baseReverse;
                    right = baseReverse;
                }

                left = Mathf.Clamp(left, 0f, 1f);
                right = Mathf.Clamp(right, 0f, 1f);
            }
        }
        else if (hasSteer)
        {
            // Chỉ bấm A/D:
            // 1 bên đẩy mạnh, 1 bên gần như không đẩy để tàu quay rõ
            float strongSide = -idleTurnForwardThrottle; // tiến
            float weakSide = 0f;

            if (steer < 0f) // A = quay trái => máy phải mạnh hơn
            {
                left = weakSide;
                right = strongSide;
            }
            else // D = quay phải => máy trái mạnh hơn
            {
                left = strongSide;
                right = weakSide;
            }

            left = Mathf.Clamp(left, -1f, 0f);
            right = Mathf.Clamp(right, -1f, 0f);
        }
        else
        {
            left = 0f;
            right = 0f;
        }
    }

    private void UpdateWaveHeadingState()
    {
        if (OceanRenderer.Instance != null)
        {
            float angleDeg = OceanRenderer.Instance.WindDirectionAngle;
            float angleRad = angleDeg * Mathf.Deg2Rad;
            _waveDirection = new Vector3(Mathf.Cos(angleRad), 0f, Mathf.Sin(angleRad)).normalized;
        }

        Vector3 shipForward = transform.forward;
        shipForward.y = 0f;

        if (shipForward.sqrMagnitude < 0.0001f)
            shipForward = Vector3.forward;
        else
            shipForward.Normalize();

        _waveAlignmentDot = Vector3.Dot(shipForward, _waveDirection);

        _headSeaFactor = Mathf.Clamp01((-_waveAlignmentDot + 1f) * 0.5f);
        _beamSeaFactor = 1f - Mathf.Abs(_waveAlignmentDot);

        _followingSeaFactor = Mathf.Clamp01((_waveAlignmentDot + 1f) * 0.5f);
        _followingSeaFactor = Mathf.Clamp01((_followingSeaFactor - 0.5f) * 2f);

        if (_waveAlignmentDot >= headFollowingThreshold)
            _waveRelativeHeading = WaveRelativeHeading.FollowingSea;
        else if (_waveAlignmentDot <= -headFollowingThreshold)
            _waveRelativeHeading = WaveRelativeHeading.HeadSea;
        else if (Mathf.Abs(_waveAlignmentDot) <= (1f - headFollowingThreshold))
            _waveRelativeHeading = WaveRelativeHeading.BeamSea;
        else
            _waveRelativeHeading = WaveRelativeHeading.QuarteringSea;

        if (drawWaveHeadingDebug)
        {
            Debug.DrawRay(transform.position + Vector3.up * 0.7f, shipForward * 4f, Color.green);
            Debug.DrawRay(transform.position + Vector3.up * 0.9f, _waveDirection * 4f, Color.cyan);
        }
    }

    private void UpdateWaterState(Engine engine, SampleHeightHelper heightHelper, SampleFlowHelper flowHelper)
    {
        if (engine == null) return;

        Transform samplePoint = engine.propellerCheckPoint != null ? engine.propellerCheckPoint : engine.thrustPoint;
        if (samplePoint == null) return;

        Vector3 pos = samplePoint.position;

        if (OceanRenderer.Instance == null)
        {
            engine.waterHeight = pos.y - 1000f;
            engine.submergence = 0f;
            engine.submergenceFactor = 0f;
            engine.isSubmerged = false;
            engine.waterFlow = Vector3.zero;
            engine.relativeVelocityToWater = Vector3.zero;
            return;
        }

        heightHelper.Init(pos, minSpatialLength);
        if (heightHelper.Sample(out float waterHeight))
            engine.waterHeight = waterHeight;

        flowHelper.Init(pos, minSpatialLength);
        if (flowHelper.Sample(out Vector2 surfaceFlow))
            engine.waterFlow = new Vector3(surfaceFlow.x, 0f, surfaceFlow.y);
        else
            engine.waterFlow = Vector3.zero;

        engine.submergence = engine.waterHeight - pos.y;
        engine.isSubmerged = engine.submergence > 0f;

        float rawSubFactor = engine.submergence / Mathf.Max(0.0001f, engine.fullSubmergenceDepth);
        engine.submergenceFactor = Mathf.Clamp01(rawSubFactor);

        if (engine.submergenceFactor > 0f)
        {
            engine.submergenceFactor = Mathf.Lerp(
                engine.ventilationMinFactor,
                1f,
                engine.submergenceFactor
            );
        }

        if (drawWaterDebug)
        {
            Vector3 waterPos = pos;
            waterPos.y = engine.waterHeight;

            Debug.DrawLine(pos, waterPos, engine.isSubmerged ? Color.cyan : Color.yellow);
            Debug.DrawRay(waterPos, Vector3.up * 0.15f, engine.isSubmerged ? Color.green : Color.red);
        }
    }

    private void UpdateEngine(Engine engine)
    {
        if (engine == null || !engine.isOn || engine.thrustPoint == null)
            return;

        float absInput = Mathf.Abs(engine.throttleInput);

        float targetRPM = absInput > 0.001f
            ? Mathf.Lerp(engine.minRPM, engine.maxRPM, absInput)
            : 0f;

        float rpmLerpSpeed = targetRPM > engine.currentRPM
            ? (1f / Mathf.Max(0.01f, engine.spinUpTime))
            : (1f / Mathf.Max(0.01f, engine.spinDownTime));

        engine.currentRPM = Mathf.Lerp(
            engine.currentRPM,
            targetRPM,
            Time.fixedDeltaTime * rpmLerpSpeed
        );

        if (absInput < 0.001f)
        {
            engine.currentThrust = 0f;
            return;
        }

        if (!engine.isSubmerged)
        {
            engine.currentThrust = 0f;
            return;
        }

        Vector3 pointVelocity = _rb.GetPointVelocity(engine.thrustPoint.position);
        engine.relativeVelocityToWater = pointVelocity - engine.waterFlow;

        float speedRelativeToWater = engine.relativeVelocityToWater.magnitude;
        float normalizedSpeed = Mathf.Clamp01(
            speedRelativeToWater / Mathf.Max(0.01f, engine.maxSpeed)
        );

        float speedEfficiency = speedEfficiencyCurve != null
            ? speedEfficiencyCurve.Evaluate(normalizedSpeed)
            : (1f - normalizedSpeed);

        float normalizedRPM = Mathf.InverseLerp(engine.minRPM, engine.maxRPM, engine.currentRPM);
        float thrustFactor = engine.thrustCurve != null
            ? engine.thrustCurve.Evaluate(normalizedRPM)
            : normalizedRPM;

        float thrust = thrustFactor * engine.maxThrust;

        // Lùi yếu hơn tiến
        if (engine.throttleInput > 0f)
            thrust *= engine.reverseCoefficient;

        thrust *= speedEfficiency;
        thrust *= engine.submergenceFactor;

        float axialWaterSpeed = Vector3.Dot(engine.relativeVelocityToWater, engine.thrustPoint.forward);
        float axialEfficiency = 1f - Mathf.Clamp01(Mathf.Abs(axialWaterSpeed) * axialFlowInfluence);
        thrust *= axialEfficiency;

        float headSeaPenalty = _headSeaFactor * maxHeadSeaThrustPenalty;
        float beamSeaPenalty = _beamSeaFactor * maxBeamSeaThrustPenalty;
        float followingSeaBoost = _followingSeaFactor * maxFollowingSeaThrustBoost;

        float waveHeadingMultiplier = 1f - headSeaPenalty - beamSeaPenalty + followingSeaBoost;
        waveHeadingMultiplier = Mathf.Clamp(waveHeadingMultiplier, 0.1f, 1.25f);

        thrust *= waveHeadingMultiplier;

        engine.currentThrust = thrust * Mathf.Sign(engine.throttleInput);

        Vector3 force = engine.thrustPoint.forward * engine.currentThrust;
        _rb.AddForceAtPosition(force, engine.thrustPoint.position, ForceMode.Force);

        if (drawThrustGizmos)
        {
            Debug.DrawRay(
                engine.thrustPoint.position,
                force * 0.0005f,
                engine == leftEngine ? Color.blue : Color.red
            );
        }
    }

    private void UpdatePropeller(Engine engine)
    {
        if (engine == null || engine.propeller == null)
            return;

        float visualRPM = engine.currentRPM;

        if (!engine.isSubmerged)
            visualRPM *= 1.1f;

        float direction = 0f;
        if (Mathf.Abs(engine.throttleInput) > 0.001f)
            direction = -Mathf.Sign(engine.throttleInput);

        float rotationSpeed = visualRPM * engine.propellerRpmRatio * direction * Time.deltaTime;
        engine.propeller.Rotate(Vector3.right * rotationSpeed, Space.Self);
    }

    public WaveRelativeHeading GetWaveRelativeHeading()
    {
        return _waveRelativeHeading;
    }

    public float GetWaveAlignmentDot()
    {
        return _waveAlignmentDot;
    }

    public void Shutdown()
    {
        ShutdownEngine(leftEngine);
        ShutdownEngine(rightEngine);

        _leftCommand = 0f;
        _rightCommand = 0f;

        enabled = false;
    }

    private void ShutdownEngine(Engine engine)
    {
        if (engine == null) return;

        engine.throttleInput = 0f;
        engine.currentThrust = 0f;
        engine.currentRPM = 0f;
        engine.submergence = 0f;
        engine.submergenceFactor = 0f;
        engine.isSubmerged = false;
        engine.waterFlow = Vector3.zero;
        engine.relativeVelocityToWater = Vector3.zero;
    }
}