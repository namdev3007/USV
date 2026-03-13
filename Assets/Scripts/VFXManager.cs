using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Spawn(GameObject prefab, Vector3 position, Quaternion rotation, float lifeTime = 10f)
    {
        if (prefab == null) return;

        GameObject vfx = Instantiate(prefab, position, rotation);
        Destroy(vfx, lifeTime);
    }
}