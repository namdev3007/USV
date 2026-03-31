using Crest.Internal;
using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Crest
{
    /// <summary>
    /// Boat physics by sampling at multiple probe points.
    /// Handles buoyancy, hydrodynamic drag, and angular damping.
    /// </summary>
    [AddComponentMenu(Internal.Constants.MENU_PREFIX_SCRIPTS + "Boat Probes")]
    public class BoatProbes : FloatingObjectBase
    {
        [SerializeField, HideInInspector]
#pragma warning disable 414
        int _version = 1;
#pragma warning restore 414

        [Header("Forces")]
        [Tooltip("Override Rigidbody center of mass, in local space.")]
        [SerializeField] Vector3 _centerOfMass = Vector3.zero;

        [SerializeField, FormerlySerializedAs("ForcePoints")]
        FloaterForcePoints[] _forcePoints = new FloaterForcePoints[] { };

        [Tooltip("Vertical offset for where hull drag force should be applied.")]
        public float _forceHeightOffset = 0f;

        [Tooltip("Overall buoyancy multiplier.")]
        public float _forceMultiplier = 10f;

        [Tooltip("Width dimension of boat. Larger values smooth Crest query response more.")]
        public float _minSpatialLength = 12f;

        [Range(0f, 1f)]
        public float _turningHeel = 0.35f;

        [Tooltip("Clamp buoyancy force magnitude per point. Use Infinity to disable.")]
        public float _maximumBuoyancyForce = Mathf.Infinity;

        [Header("Buoyancy Tuning")]
        [Tooltip("Depth at which a buoyancy point is considered fully submerged.")]
        public float _maxSubmergenceDepth = 1.0f;

        [Tooltip("Vertical damping applied at each buoyancy point.")]
        public float _buoyancyDamping = 800f;

        [Tooltip("Extra water damping using full point velocity relative to water.")]
        public float _waterPointDamping = 100f;

        [Tooltip("Curve controlling buoyancy response from 0..1 submerged.")]
        public AnimationCurve _buoyancyCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Linear Drag")]
        [Tooltip("Quadratic drag coefficient for vertical motion.")]
        public float _dragInWaterUp = 3f;

        [Tooltip("Quadratic drag coefficient for sideways motion.")]
        public float _dragInWaterRight = 2f;

        [Tooltip("Quadratic drag coefficient for forward motion.")]
        public float _dragInWaterForward = 1f;

        [Header("Angular Drag")]
        [Tooltip("Roll damping around local forward axis.")]
        public float _rollDamping = 30f;

        [Tooltip("Pitch damping around local right axis.")]
        public float _pitchDamping = 20f;

        [Tooltip("Yaw damping around local up axis.")]
        public float _yawDamping = 12f;

        [Space(10)]
        [SerializeField] DebugFields _debug = new DebugFields();

        [Serializable]
        class DebugFields
        {
            [Tooltip("Draw Crest query results and force points.")]
            public bool _drawQueries = false;

            [Tooltip("Draw buoyancy force vectors.")]
            public bool _drawBuoyancyForces = false;

            [Tooltip("Draw drag vector.")]
            public bool _drawDragForce = false;
        }

        const float WATER_DENSITY = 1000f;

        Rigidbody _rb;
        float _totalWeight;

        Vector3[] _queryPoints;
        Vector3[] _queryResultDisps;
        Vector3[] _queryResultVels;

        readonly SampleFlowHelper _sampleFlowHelper = new SampleFlowHelper();

        public override Vector3 Velocity => _rb != null ? _rb.LinearVelocity() : Vector3.zero;
        public override float ObjectWidth => _minSpatialLength;
        public override bool InWater => true;

        void Start()
        {
            _rb = GetComponent<Rigidbody>();

            if (_rb == null)
            {
                Debug.LogError($"{nameof(BoatProbes)} requires a Rigidbody on the same GameObject.", this);
                enabled = false;
                return;
            }

            _rb.centerOfMass = _centerOfMass;

            CalcTotalWeight();

            int queryCount = _forcePoints.Length + 1;
            _queryPoints = new Vector3[queryCount];
            _queryResultDisps = new Vector3[queryCount];
            _queryResultVels = new Vector3[queryCount];
        }

        void CalcTotalWeight()
        {
            _totalWeight = 0f;

            foreach (var pt in _forcePoints)
            {
                _totalWeight += Mathf.Max(0f, pt._weight);
            }

            if (_totalWeight <= 0f)
            {
                _totalWeight = 1f;
            }
        }

        void FixedUpdate()
        {
#if UNITY_EDITOR
            CalcTotalWeight();
#endif

            if (_rb == null || OceanRenderer.Instance == null)
            {
                return;
            }

            var collProvider = OceanRenderer.Instance.CollisionProvider;
            if (collProvider == null)
            {
                return;
            }

            UpdateWaterQueries(collProvider);

            // Base water surface velocity from Crest collision query at boat center.
            var waterSurfaceVel = _queryResultVels[_forcePoints.Length];

            // Add flow sim if enabled.
            _sampleFlowHelper.Init(transform.position, _minSpatialLength);
            _sampleFlowHelper.Sample(out var surfaceFlow);
            waterSurfaceVel += new Vector3(surfaceFlow.x, 0f, surfaceFlow.y);

            if (_debug._drawQueries)
            {
                Debug.DrawLine(
                    transform.position + 5f * Vector3.up,
                    transform.position + 5f * Vector3.up + waterSurfaceVel,
                    new Color(1f, 1f, 1f, 0.6f)
                );
            }

            FixedUpdateBuoyancy(waterSurfaceVel);
            FixedUpdateDrag(waterSurfaceVel);
            FixedUpdateAngularDrag();
        }

        void UpdateWaterQueries(ICollProvider collProvider)
        {
            for (int i = 0; i < _forcePoints.Length; i++)
            {
                _queryPoints[i] = transform.TransformPoint(
                    _forcePoints[i]._offsetPosition + new Vector3(0f, _centerOfMass.y, 0f)
                );
            }

            _queryPoints[_forcePoints.Length] = transform.position;

            collProvider.Query(
                GetHashCode(),
                ObjectWidth,
                _queryPoints,
                _queryResultDisps,
                null,
                _queryResultVels
            );

            if (_debug._drawQueries)
            {
                for (int i = 0; i < _forcePoints.Length; i++)
                {
                    var query = _queryPoints[i];
                    query.y = OceanRenderer.Instance.SeaLevel + _queryResultDisps[i].y;
                    VisualiseCollisionArea.DebugDrawCross(query, 1f, Color.magenta);
                }
            }
        }

        void FixedUpdateBuoyancy(Vector3 waterSurfaceVel)
        {
            float archimedesForceMagnitude = WATER_DENSITY * Mathf.Abs(Physics.gravity.y);

            for (int i = 0; i < _forcePoints.Length; i++)
            {
                float pointWeight = Mathf.Max(0f, _forcePoints[i]._weight);
                if (pointWeight <= 0f)
                {
                    continue;
                }

                float waterHeight = OceanRenderer.Instance.SeaLevel + _queryResultDisps[i].y;
                float heightDiff = waterHeight - _queryPoints[i].y;

                if (heightDiff <= 0f)
                {
                    continue;
                }

                // Normalize submergence so buoyancy can be tuned with a curve.
                float submergence = Mathf.Clamp01(
                    heightDiff / Mathf.Max(0.0001f, _maxSubmergenceDepth)
                );

                float buoyancyFactor = _buoyancyCurve != null
                    ? _buoyancyCurve.Evaluate(submergence)
                    : submergence;

                // Base buoyancy force.
                Vector3 buoyancyForce =
                    _forceMultiplier *
                    pointWeight *
                    archimedesForceMagnitude *
                    buoyancyFactor *
                    Vector3.up / _totalWeight;

                // Vertical damping at the point.
                Vector3 pointVelocity = _rb.GetPointVelocity(_queryPoints[i]);
                float verticalVelocity = Vector3.Dot(pointVelocity, Vector3.up);
                Vector3 verticalDampingForce =
                    -verticalVelocity *
                    _buoyancyDamping *
                    pointWeight *
                    Vector3.up / _totalWeight;

                // Additional damping relative to moving water.
                Vector3 pointVelocityRelativeToWater = pointVelocity - waterSurfaceVel;
                Vector3 waterDampingForce =
                    -pointVelocityRelativeToWater *
                    _waterPointDamping *
                    pointWeight / _totalWeight;

                Vector3 totalForce = buoyancyForce + verticalDampingForce + waterDampingForce;

                if (_maximumBuoyancyForce < Mathf.Infinity)
                {
                    totalForce = Vector3.ClampMagnitude(totalForce, _maximumBuoyancyForce);
                }

                _rb.AddForceAtPosition(totalForce, _queryPoints[i], ForceMode.Force);

                if (_debug._drawBuoyancyForces)
                {
                    Debug.DrawRay(
                        _queryPoints[i],
                        totalForce / Mathf.Max(1f, _rb.mass) * 0.05f,
                        Color.cyan
                    );
                }
            }
        }

        void FixedUpdateDrag(Vector3 waterSurfaceVel)
        {
            Vector3 velocityRelativeToWater = Velocity - waterSurfaceVel;
            Vector3 forcePosition = _rb.worldCenterOfMass + _forceHeightOffset * Vector3.up;

            float upSpeed = Vector3.Dot(Vector3.up, velocityRelativeToWater);
            float rightSpeed = Vector3.Dot(transform.right, velocityRelativeToWater);
            float forwardSpeed = Vector3.Dot(transform.forward, velocityRelativeToWater);

            Vector3 upDrag =
                -Vector3.up * upSpeed * Mathf.Abs(upSpeed) * _dragInWaterUp;

            Vector3 rightDrag =
                -transform.right * rightSpeed * Mathf.Abs(rightSpeed) * _dragInWaterRight;

            Vector3 forwardDrag =
                -transform.forward * forwardSpeed * Mathf.Abs(forwardSpeed) * _dragInWaterForward;

            Vector3 totalDrag = upDrag + rightDrag + forwardDrag;

            _rb.AddForceAtPosition(totalDrag, forcePosition, ForceMode.Force);

            if (_debug._drawDragForce)
            {
                Debug.DrawRay(forcePosition, totalDrag / Mathf.Max(1f, _rb.mass) * 0.05f, Color.yellow);
            }
        }

        void FixedUpdateAngularDrag()
        {
            Vector3 localAngularVel = transform.InverseTransformDirection(_rb.angularVelocity);

    
            Vector3 localAngularDampingTorque = new Vector3(
                -localAngularVel.x * Mathf.Abs(localAngularVel.x) * _pitchDamping,
                -localAngularVel.y * Mathf.Abs(localAngularVel.y) * _yawDamping,
                -localAngularVel.z * Mathf.Abs(localAngularVel.z) * _rollDamping
            );

            Vector3 worldTorque = transform.TransformDirection(localAngularDampingTorque);
            _rb.AddTorque(worldTorque, ForceMode.Force);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawCube(transform.TransformPoint(_centerOfMass), Vector3.one * 0.25f);

            if (_forcePoints == null)
            {
                return;
            }

            for (int i = 0; i < _forcePoints.Length; i++)
            {
                var point = _forcePoints[i];
                var transformedPoint = transform.TransformPoint(
                    point._offsetPosition + new Vector3(0f, _centerOfMass.y, 0f)
                );

                Gizmos.color = Color.red;
                Gizmos.DrawCube(transformedPoint, Vector3.one * 0.1f);
            }
        }
    }

    [Serializable]
    public class FloaterForcePoints
    {
        [FormerlySerializedAs("_factor")]
        public float _weight = 1f;

        public Vector3 _offsetPosition;
    }
}