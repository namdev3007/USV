using Crest;
using UnityEngine;
using Range = UnityEngine.RangeAttribute;

namespace CustomTwinEngineShip
{
    [System.Serializable]
    public class CrestWaveHeadingAnalyzer : IWaveHeadingAnalyzer
    {
        [Header("Wave Heading Effect")]
        [Range(0f, 1f)] public float headFollowingThreshold = 0.7f;
        [Range(0f, 1f)] public float maxHeadSeaThrustPenalty = 0.35f;
        [Range(0f, 1f)] public float maxBeamSeaThrustPenalty = 0.10f;
        [Range(0f, 0.5f)] public float maxFollowingSeaThrustBoost = 0.08f;

        [Header("Debug")]
        public bool drawWaveHeadingDebug = true;

        [System.NonSerialized]
        private Transform _shipTransform;

        public void Initialize(Transform shipTransform)
        {
            _shipTransform = shipTransform;
        }

        public WaveHeadingData Analyze()
        {
            if (_shipTransform == null)
            {
                return new WaveHeadingData(
                    Vector3.forward,
                    0f,
                    0f,
                    0f,
                    0f,
                    1f,
                    WaveRelativeHeading.QuarteringSea
                );
            }

            Vector3 waveDirection = Vector3.forward;

            if (OceanRenderer.Instance != null)
            {
                float angleDeg = OceanRenderer.Instance.WindDirectionAngle;
                float angleRad = angleDeg * Mathf.Deg2Rad;
                waveDirection = new Vector3(Mathf.Cos(angleRad), 0f, Mathf.Sin(angleRad)).normalized;
            }

            Vector3 shipForward = _shipTransform.forward;
            shipForward.y = 0f;

            if (shipForward.sqrMagnitude < 0.0001f)
                shipForward = Vector3.forward;
            else
                shipForward.Normalize();

            float alignmentDot = Vector3.Dot(shipForward, waveDirection);

            float headSeaFactor = Mathf.Clamp01((-alignmentDot + 1f) * 0.5f);
            float beamSeaFactor = 1f - Mathf.Abs(alignmentDot);

            float followingSeaFactor = Mathf.Clamp01((alignmentDot + 1f) * 0.5f);
            followingSeaFactor = Mathf.Clamp01((followingSeaFactor - 0.5f) * 2f);

            WaveRelativeHeading relativeHeading;
            if (alignmentDot >= headFollowingThreshold)
                relativeHeading = WaveRelativeHeading.FollowingSea;
            else if (alignmentDot <= -headFollowingThreshold)
                relativeHeading = WaveRelativeHeading.HeadSea;
            else if (Mathf.Abs(alignmentDot) <= (1f - headFollowingThreshold))
                relativeHeading = WaveRelativeHeading.BeamSea;
            else
                relativeHeading = WaveRelativeHeading.QuarteringSea;

            float headSeaPenalty = headSeaFactor * maxHeadSeaThrustPenalty;
            float beamSeaPenalty = beamSeaFactor * maxBeamSeaThrustPenalty;
            float followingSeaBoost = followingSeaFactor * maxFollowingSeaThrustBoost;

            float thrustMultiplier = 1f - headSeaPenalty - beamSeaPenalty + followingSeaBoost;
            thrustMultiplier = Mathf.Clamp(thrustMultiplier, 0.1f, 1.25f);

            if (drawWaveHeadingDebug)
            {
                Debug.DrawRay(_shipTransform.position + Vector3.up * 0.7f, shipForward * 4f, Color.green);
                Debug.DrawRay(_shipTransform.position + Vector3.up * 0.9f, waveDirection * 4f, Color.cyan);
            }

            return new WaveHeadingData(
                waveDirection,
                alignmentDot,
                headSeaFactor,
                beamSeaFactor,
                followingSeaFactor,
                thrustMultiplier,
                relativeHeading
            );
        }
    }
}