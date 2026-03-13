using UnityEngine;

public class DebrisExplosion : MonoBehaviour, IExplodable
{
    [Header("Explosion")]
    [SerializeField] private float explosionForce = 25f;
    [SerializeField] private float explosionRadius = 8f;
    [SerializeField] private float upwardsModifier = 0.05f;
    [SerializeField] private float randomTorque = 15f;

    [Header("Physics")]
    [SerializeField] private float mass = 2f;
    [SerializeField] private float drag = 2f;
    [SerializeField] private float angularDrag = 2f;

    [Header("References")]
    [SerializeField] private bool autoFindRigidbodies = true;
    [SerializeField] private Rigidbody[] pieces;

    private bool _initialized;
    private Vector3 _lastExplosionPoint;

    private void Awake()
    {
        Initialize();
        SetKinematic(true);
    }

    private void Initialize()
    {
        if (_initialized) return;

        if (autoFindRigidbodies || pieces == null || pieces.Length == 0)
        {
            pieces = GetComponentsInChildren<Rigidbody>(true);
        }

        foreach (Rigidbody rb in pieces)
        {
            if (rb == null) continue;

            rb.mass = mass;
            rb.linearDamping = drag;
            rb.angularDamping = angularDrag;
            rb.useGravity = true;
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        _initialized = true;
    }

    private void SetKinematic(bool value)
    {
        foreach (Rigidbody rb in pieces)
        {
            if (rb == null) continue;

            rb.isKinematic = value;

            if (value)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

    public void SetExplosionPoint(Vector3 point)
    {
        _lastExplosionPoint = point;
    }

    public void Explode()
    {
        Initialize();
        SetKinematic(false);

        foreach (Rigidbody rb in pieces)
        {
            if (rb == null) continue;

            rb.AddExplosionForce(
                explosionForce,
                _lastExplosionPoint,
                explosionRadius,
                upwardsModifier,
                ForceMode.Impulse
            );

            rb.AddTorque(Random.onUnitSphere * randomTorque, ForceMode.Impulse);
        }
    }
}