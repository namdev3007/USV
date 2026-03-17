using System;
using UnityEngine;

public class ShipCollisionDetector : MonoBehaviour
{
    [SerializeField] private string targetTag = "Ship";

    public event Action<Collider> OnShipHit;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag(targetTag))
        {
            Debug.Log("Ship hit detected");
            OnShipHit?.Invoke(collision.collider);
        }
    }
}   