using UnityEngine;

public class JoystickAxisTester : MonoBehaviour
{
    void Update()
    {
        float x = Input.GetAxis("X axis");
        float y = Input.GetAxis("Y axis");
        float a3 = Input.GetAxis("Joy3");
        float a4 = Input.GetAxis("Joy4");
        float a5 = Input.GetAxis("Joy5");
        float a6 = Input.GetAxis("Joy6");

        Debug.Log($"X={x:F2} | Y={y:F2} | A3={a3:F2} | A4={a4:F2} | A5={a5:F2} | A6={a6:F2}");
    }
}