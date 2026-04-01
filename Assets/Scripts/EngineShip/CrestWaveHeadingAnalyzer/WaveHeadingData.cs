using UnityEngine;

namespace CustomTwinEngineShip
{
    public enum WaveRelativeHeading
    {
        FollowingSea,
        HeadSea,
        BeamSea,
        QuarteringSea
    }

    [System.Serializable]
    public struct WaveHeadingData
    {
        public Vector3 waveDirection;
        public float alignmentDot;
        public float headSeaFactor;
        public float beamSeaFactor;
        public float followingSeaFactor;
        public float thrustMultiplier;
        public WaveRelativeHeading relativeHeading;

        public WaveHeadingData(
            Vector3 waveDirection,
            float alignmentDot,
            float headSeaFactor,
            float beamSeaFactor,
            float followingSeaFactor,
            float thrustMultiplier,
            WaveRelativeHeading relativeHeading)
        {
            this.waveDirection = waveDirection;
            this.alignmentDot = alignmentDot;
            this.headSeaFactor = headSeaFactor;
            this.beamSeaFactor = beamSeaFactor;
            this.followingSeaFactor = followingSeaFactor;
            this.thrustMultiplier = thrustMultiplier;
            this.relativeHeading = relativeHeading;
        }
    }
}