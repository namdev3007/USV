using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class USVEngine : MonoBehaviour
{
    [System.Serializable]
    public class Engine
    {
        public string name;

        [Header("State")]
        public bool isOn = true;
        public bool startOnThrottle = true;

        [Header("RPM")]
        public float minRPM = 800f;
        public float maxRPM = 5000f;
        public float spinUpTime = 0.4f;

        [Header("Power")]
        public float maxThrust = 4000f;
        public float reverseCoefficient = 0.5f;
        public float maxSpeed = 10f;

        public AnimationCurve thrustCurve =
            AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Transforms")]
        public Transform thrustPoint;
        public Transform propeller;
        public float propellerRpmRatio = 0.1f;

        [HideInInspector] public float currentRPM;
        [HideInInspector] public float throttleInput;
        [HideInInspector] public float currentThrust;
    }

    public Engine leftEngine;
    public Engine rightEngine;

    [Header("General")]
    public float engineResponse = 2f;
    public float waterAngularDrag = 2f;

    [Header("Debug")]
    public bool enableDebugLogs = true;
    public bool drawThrustGizmos = true;

    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.angularDamping = waterAngularDrag;
    }
    private void Update()
    {
        float throttle = Input.GetAxis("Vertical");   // W/S
        float steer = Input.GetAxis("Horizontal"); // A/D


        // Đảo chiều trái/phải nếu đang bị ngược
        steer *= -1f;

        leftEngine.throttleInput = Mathf.Clamp(throttle + steer, -1f, 1f);
        rightEngine.throttleInput = Mathf.Clamp(throttle - steer, -1f, 1f);
    }

    private void FixedUpdate()
    {
        UpdateEngine(leftEngine);
        UpdateEngine(rightEngine);
    }

    void UpdateEngine(Engine engine)
    {
        if (!engine.isOn || engine.thrustPoint == null)
            return;

        float speed = _rb.linearVelocity.magnitude;

        if (speed > engine.maxSpeed)
        {
            if (enableDebugLogs)
                Debug.Log($"{engine.name} reached max speed limit.");
            return;
        }

        float targetRPM = Mathf.Lerp(engine.minRPM,
                                     engine.maxRPM,
                                     Mathf.Abs(engine.throttleInput));

        engine.currentRPM = Mathf.Lerp(engine.currentRPM,
                                       targetRPM,
                                       Time.fixedDeltaTime * (1f / engine.spinUpTime));

        float normalizedRPM = Mathf.InverseLerp(engine.minRPM,
                                                engine.maxRPM,
                                                engine.currentRPM);

        float thrustFactor = engine.thrustCurve.Evaluate(normalizedRPM);

        float thrust = thrustFactor * engine.maxThrust;

        if (engine.throttleInput < 0)
            thrust *= engine.reverseCoefficient;

        engine.currentThrust = thrust * Mathf.Sign(engine.throttleInput);

        Vector3 force = engine.thrustPoint.forward * engine.currentThrust;

        _rb.AddForceAtPosition(force,
                               engine.thrustPoint.position,
                               ForceMode.Force);

        if (drawThrustGizmos)
        {
            Debug.DrawRay(engine.thrustPoint.position,
                          force * 0.0005f,
                          engine == leftEngine ? Color.blue : Color.red);
        }

        if (enableDebugLogs && Mathf.Abs(engine.throttleInput) > 0.01f)
        {
            Debug.Log(
                $"{engine.name} | Throttle: {engine.throttleInput:F2} | RPM: {engine.currentRPM:F0} | Thrust: {engine.currentThrust:F0}");
        }

        UpdatePropeller(engine);
    }

    void UpdatePropeller(Engine engine)
    {
        if (engine.propeller == null) return;

        float rotationSpeed = engine.currentRPM *
                              engine.propellerRpmRatio *
                              Time.deltaTime;

        engine.propeller.Rotate(Vector3.forward * rotationSpeed);
    }
}