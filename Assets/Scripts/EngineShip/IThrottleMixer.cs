using UnityEngine;

namespace CustomTwinEngineShip
{
    public interface IThrottleMixer
    {
        Vector2 Mix(ShipInputData input);
    }
}