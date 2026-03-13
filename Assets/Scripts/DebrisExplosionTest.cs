using UnityEngine;

public class DebrisExplosionTest : MonoBehaviour
{
    [Header("Auto Explode")]
    public bool explodeOnStart = true;
    public float delay = 1.0f;

    [Header("Explosion Settings")]
    public Transform explosionCenter;
    public float explosionForce = 800f;
    public float explosionRadius = 8f;
    public float upwardsModifier = 1.2f;
    public float randomTorque = 80f;

    [Header("Debris Settings")]
    public bool autoFindRigidbodies = true;
    public Rigidbody[] pieces;

    private bool _exploded = false;

    private void Start()
    {
        if (autoFindRigidbodies)
        {
            pieces = GetComponentsInChildren<Rigidbody>(true);
        }

        // Khóa toàn bộ mảnh trước khi nổ
        foreach (Rigidbody rb in pieces)
        {
            if (rb == null) continue;

            rb.isKinematic = true;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (explodeOnStart)
        {
            Invoke(nameof(Explode), delay);
        }
    }

    [ContextMenu("Explode Now")]
    public void Explode()
    {
        if (_exploded) return;
        _exploded = true;

        Vector3 center = explosionCenter != null ? explosionCenter.position : transform.position;

        foreach (Rigidbody rb in pieces)
        {
            if (rb == null) continue;

            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            rb.AddExplosionForce(
                explosionForce,
                center,
                explosionRadius,
                upwardsModifier,
                ForceMode.Impulse
            );

            rb.AddTorque(Random.onUnitSphere * randomTorque, ForceMode.Impulse);
        }
    }
}