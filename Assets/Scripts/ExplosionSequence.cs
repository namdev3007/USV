using UnityEngine;

public class ExplosionSequence : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private GameObject originalVisual;
    [SerializeField] private GameObject debrisRoot;
    [SerializeField] private DebrisExplosion debrisExplosion;
    [SerializeField] private Transform explosionPoint;

    [Header("Effects")]
    [SerializeField] private GameObject explosionVfxPrefab;
    [SerializeField] private AudioClip explosionSfx;
    [SerializeField][Range(0f, 1f)] private float explosionVolume = 1f;

    [Header("Disable On Explode")]
    [SerializeField] private Behaviour[] behavioursToDisable;
    [SerializeField] private Collider[] collidersToDisable;
    [SerializeField] private MonoBehaviour[] shutdownTargets;

    public void Play()
    {
        Vector3 point = explosionPoint != null ? explosionPoint.position : transform.position;

        ShutdownSystems();
        DisableColliders();
        SpawnEffects(point);
        ActivateDebris(point);
        HideOriginal();
    }

    private void ShutdownSystems()
    {
        if (shutdownTargets != null)
        {
            foreach (MonoBehaviour target in shutdownTargets)
            {
                if (target == null) continue;

                if (target is IShutdownable shutdownable)
                {
                    shutdownable.Shutdown();
                }
                else
                {
                    target.enabled = false;
                }
            }
        }

        if (behavioursToDisable != null)
        {
            foreach (Behaviour behaviour in behavioursToDisable)
            {
                if (behaviour == null) continue;
                behaviour.enabled = false;
            }
        }
    }

    private void DisableColliders()
    {
        if (collidersToDisable == null) return;

        foreach (Collider col in collidersToDisable)
        {
            if (col == null) continue;
            col.enabled = false;
        }
    }

    private void SpawnEffects(Vector3 point)
    {
        if (VFXManager.Instance != null)
        {
            VFXManager.Instance.Spawn(explosionVfxPrefab, point, Quaternion.identity);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySfxAtPoint(explosionSfx, point, explosionVolume);
        }
    }

    private void ActivateDebris(Vector3 point)
    {
        if (debrisRoot != null)
        {
            debrisRoot.SetActive(true);
        }

        if (debrisExplosion != null)
        {
            debrisExplosion.SetExplosionPoint(point);
            debrisExplosion.Explode();
        }
    }

    private void HideOriginal()
    {
        if (originalVisual != null)
        {
            originalVisual.SetActive(false);
        }
    }
}