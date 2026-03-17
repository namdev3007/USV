using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class KamikazeImpactSequence : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ShipCollisionDetector collisionDetector;
    [SerializeField] private Rigidbody rb;

    [Header("Cinemachine Cameras")]
    [SerializeField] private CinemachineCamera cam1;
    [SerializeField] private CinemachineCamera cam2;
    [SerializeField] private int cam1Priority = 10;
    [SerializeField] private int cam2Priority = 20;

    [Header("Timing")]
    [SerializeField] private float pauseBeforeExplosion = 1f;
    [SerializeField] private bool useSlowMotion = true;
    [SerializeField] private float slowMotionScale = 0.05f;

    [Header("Explosion")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private Vector3 explosionOffset = Vector3.zero;
    [SerializeField] private bool destroySelfAfterExplosion = true;
    [SerializeField] private float destroyDelay = 0.1f;

    [Header("Optional")]
    [SerializeField] private MonoBehaviour[] scriptsToDisable;

    private bool _triggered;

    private void Reset()
    {
        collisionDetector = GetComponent<ShipCollisionDetector>();
        rb = GetComponent<Rigidbody>();
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

    private void Start()
    {
        SetCam1Live();
    }

    private void HandleShipHit(Collider other)
    {
        if (_triggered) return;
        StartCoroutine(ImpactSequence(other));
    }

    private IEnumerator ImpactSequence(Collider other)
    {
        _triggered = true;

        // Tắt các script điều khiển tàu nếu cần
        if (scriptsToDisable != null)
        {
            foreach (var script in scriptsToDisable)
            {
                if (script != null)
                    script.enabled = false;
            }
        }

        // Dừng chuyển động tàu để giữ khung hình va chạm
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // Chuyển sang cam 2
        SetCam2Live();

        // Slow motion / pause nhẹ
        if (useSlowMotion)
        {
            Time.timeScale = slowMotionScale;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }

        // Chờ 1 giây thời gian thật
        yield return new WaitForSecondsRealtime(pauseBeforeExplosion);

        // Trả thời gian về bình thường trước khi nổ
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        // Vị trí nổ
        Vector3 explosionPosition = transform.position + explosionOffset;

        if (other != null)
        {
            Vector3 closestPoint = other.ClosestPoint(transform.position);
            if (closestPoint != Vector3.zero)
                explosionPosition = closestPoint + explosionOffset;
        }

        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, explosionPosition, Quaternion.identity);
        }

        if (destroySelfAfterExplosion)
        {
            Destroy(gameObject, destroyDelay);
        }
    }

    private void SetCam1Live()
    {
        if (cam1 != null) cam1.Priority = cam1Priority;
        if (cam2 != null) cam2.Priority = Mathf.Min(cam1Priority - 1, cam2Priority - 1);
    }

    private void SetCam2Live()
    {
        if (cam1 != null) cam1.Priority = cam1Priority;
        if (cam2 != null) cam2.Priority = cam2Priority;
    }
}