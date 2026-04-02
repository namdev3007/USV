using UnityEngine;

public class CopyCameraTransform : MonoBehaviour
{
    [SerializeField] private Transform sourceCamera;

    private void LateUpdate()
    {
        if (sourceCamera == null) return;

        transform.position = sourceCamera.position;
        transform.rotation = sourceCamera.rotation;
    }
}