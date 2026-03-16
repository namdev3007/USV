// Crest Ocean System
// Copyright 2020 Wave Harmonic Ltd

using Crest.Internal;
using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Crest
{
    /// <summary>
    /// Boat physics by sampling at multiple probe points.
    /// Control/engine input has been removed.
    /// This component now only handles buoyancy and drag.
    /// </summary>
    [AddComponentMenu(Internal.Constants.MENU_PREFIX_SCRIPTS + "Boat Probes")]
    public class BoatProbes : FloatingObjectBase
    {
        /// <summary>
        /// The version of this asset. Can be used to migrate across versions. This value should
        /// only be changed when the editor upgrades the version.
        /// </summary>
        [SerializeField, HideInInspector]
#pragma warning disable 414
        int _version = 0;
#pragma warning restore 414

        [Header("Forces")]
        [Tooltip("Override RB center of mass, in local space."), SerializeField]
        Vector3 _centerOfMass = Vector3.zero;

        [SerializeField, FormerlySerializedAs("ForcePoints")]
        FloaterForcePoints[] _forcePoints = new FloaterForcePoints[] { };

        [Tooltip("Vertical offset for where drag force should be applied.")]
        public float _forceHeightOffset = 0f;

        public float _forceMultiplier = 10f;

        [Tooltip("Width dimension of boat. The larger this value, the more filtered/smooth the wave response will be.")]
        public float _minSpatialLength = 12f;

        [Range(0, 1)]
        public float _turningHeel = 0.35f;

        [Tooltip("Clamps the buoyancy force to this value. Useful for handling fully submerged objects. Enter 'Infinity' to disable.")]
        public float _maximumBuoyancyForce = Mathf.Infinity;

        [Header("Drag")]
        public float _dragInWaterUp = 3f;
        public float _dragInWaterRight = 2f;
        public float _dragInWaterForward = 1f;

        [Space(10)]
        [SerializeField]
        DebugFields _debug = new DebugFields();

        [Serializable]
        class DebugFields
        {
            [Tooltip("Draw queries for each force point as gizmos.")]
            public bool _drawQueries = false;
        }

        const float WATER_DENSITY = 1000f;

        Rigidbody _rb;
        float _totalWeight;

        Vector3[] _queryPoints;
        Vector3[] _queryResultDisps;
        Vector3[] _queryResultVels;

        readonly SampleFlowHelper _sampleFlowHelper = new SampleFlowHelper();

        public override Vector3 Velocity => _rb.LinearVelocity();
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
                _totalWeight += pt._weight;
            }

            if (_totalWeight <= 0f)
            {
                _totalWeight = 1f;
            }
        }

        void FixedUpdate()
        {
#if UNITY_EDITOR
            // Sum weights every frame when running in editor in case weights are edited in the inspector.
            CalcTotalWeight();
#endif

            if (OceanRenderer.Instance == null)
            {
                return;
            }

            var collProvider = OceanRenderer.Instance.CollisionProvider;
            if (collProvider == null)
            {
                return;
            }

            UpdateWaterQueries(collProvider);

            var waterSurfaceVel = _queryResultVels[_forcePoints.Length];

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

            FixedUpdateBuoyancy();
            FixedUpdateDrag(waterSurfaceVel);
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

        void FixedUpdateBuoyancy()
        {
            float archimedesForceMagnitude = WATER_DENSITY * Mathf.Abs(Physics.gravity.y);

            for (int i = 0; i < _forcePoints.Length; i++)
            {
                float waterHeight = OceanRenderer.Instance.SeaLevel + _queryResultDisps[i].y;
                float heightDiff = waterHeight - _queryPoints[i].y;

                if (heightDiff > 0f)
                {
                    Vector3 force =
                        _forceMultiplier *
                        _forcePoints[i]._weight *
                        archimedesForceMagnitude *
                        heightDiff *
                        Vector3.up / _totalWeight;

                    if (_maximumBuoyancyForce < Mathf.Infinity)
                    {
                        force = Vector3.ClampMagnitude(force, _maximumBuoyancyForce);
                    }

                    _rb.AddForceAtPosition(force, _queryPoints[i]);
                }
            }
        }

        void FixedUpdateDrag(Vector3 waterSurfaceVel)
        {
            Vector3 velocityRelativeToWater = Velocity - waterSurfaceVel;
            Vector3 forcePosition = _rb.worldCenterOfMass + _forceHeightOffset * Vector3.up;

            _rb.AddForceAtPosition(
                _dragInWaterUp * Vector3.Dot(Vector3.up, -velocityRelativeToWater) * Vector3.up,
                forcePosition,
                ForceMode.Acceleration
            );

            _rb.AddForceAtPosition(
                _dragInWaterRight * Vector3.Dot(transform.right, -velocityRelativeToWater) * transform.right,
                forcePosition,
                ForceMode.Acceleration
            );

            _rb.AddForceAtPosition(
                _dragInWaterForward * Vector3.Dot(transform.forward, -velocityRelativeToWater) * transform.forward,
                forcePosition,
                ForceMode.Acceleration
            );
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawCube(transform.TransformPoint(_centerOfMass), Vector3.one * 0.25f);

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