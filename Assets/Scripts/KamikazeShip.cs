using UnityEngine;

public class KamikazeShip : MonoBehaviour
{
    [SerializeField] private ShipCollisionDetector collisionDetector;
    [SerializeField] private ExplosionSequence explosionSequence;

    private bool _hasExploded;

    private void Reset()
    {
        collisionDetector = GetComponent<ShipCollisionDetector>();
        explosionSequence = GetComponent<ExplosionSequence>();
    }

    private void OnEnable()
    {
        if (collisionDetector != null)
        {
            collisionDetector.OnShipHit += HandleShipHit;
        }
    }

    private void OnDisable()
    {
        if (collisionDetector != null)
        {
            collisionDetector.OnShipHit -= HandleShipHit;
        }
    }

    private void HandleShipHit(Collider other)
    {
        if (_hasExploded) return;
        _hasExploded = true;

        explosionSequence?.Play();
    }

    [ContextMenu("Explode")]
    public void ExplodeNow()
    {
        if (_hasExploded) return;
        _hasExploded = true;

        explosionSequence?.Play();
    }
}