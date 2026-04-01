using UnityEngine;

public class TwinEngineInputTest : MonoBehaviour
{
    void Update()
    {
        float left = Input.GetAxis("LeftEngine");
        float right = Input.GetAxis("RightEngine");

        Debug.Log($"LeftEngine: {left:F2} | RightEngine: {right:F2}");
    }
}