using UnityEngine;
using Unity.Cinemachine;

public class ShipCameraSwitcher : MonoBehaviour
{
    [Header("Camera References")]
    [SerializeField] private CinemachineCamera firstPersonCamera;
    [SerializeField] private CinemachineCamera thirdPersonCamera;

    [Header("Switch Settings")]
    [SerializeField] private KeyCode switchKey = KeyCode.C;
    [SerializeField] private bool startWithThirdPerson = true;
    [SerializeField] private int activePriority = 20;
    [SerializeField] private int inactivePriority = 10;

    private bool _isThirdPerson;

    private void Start()
    {
        _isThirdPerson = startWithThirdPerson;
        ApplyCameraState();
    }

    private void Update()
    {
        if (Input.GetKeyDown(switchKey))
        {
            _isThirdPerson = !_isThirdPerson;
            ApplyCameraState();
        }
    }

    private void ApplyCameraState()
    {
        if (firstPersonCamera == null || thirdPersonCamera == null)
        {
            Debug.LogWarning("[ShipCameraSwitcher] Chưa gán camera.");
            return;
        }

        if (_isThirdPerson)
        {
            thirdPersonCamera.Priority = activePriority;
            firstPersonCamera.Priority = inactivePriority;
        }
        else
        {
            thirdPersonCamera.Priority = inactivePriority;
            firstPersonCamera.Priority = activePriority;
        }
    }

    public void SwitchToFirstPerson()
    {
        _isThirdPerson = false;
        ApplyCameraState();
    }

    public void SwitchToThirdPerson()
    {
        _isThirdPerson = true;
        ApplyCameraState();
    }

    public void ToggleCamera()
    {
        _isThirdPerson = !_isThirdPerson;
        ApplyCameraState();
    }
}