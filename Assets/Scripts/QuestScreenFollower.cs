using UnityEngine;

public class QuestScreenFollower : MonoBehaviour
{
    [SerializeField] private Transform followTarget;
    [SerializeField] private float distance = 1.2f;
    [SerializeField] private Vector3 offset = Vector3.zero;

    private void LateUpdate()
    {
        if (followTarget == null) return;

        transform.position =
            followTarget.position +
            followTarget.forward * distance +
            followTarget.right * offset.x +
            followTarget.up * offset.y;

        transform.rotation = followTarget.rotation;
    }
}