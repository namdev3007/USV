using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using Michsky.UI.Shift;

public class KamikazeShip : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ShipCollisionDetector collisionDetector;
    [SerializeField] private ExplosionSequence explosionSequence;
    [SerializeField] private Rigidbody rb;

    [Header("Cameras")]
    [SerializeField] private CinemachineCamera cam1;
    [SerializeField] private CinemachineCamera cam2;
    [SerializeField] private int cam1Priority = 10;
    [SerializeField] private int cam2Priority = 20;

    [Header("Pre-Impact Detection")]
    [SerializeField] private bool switchCamWhenNearTarget = true;
    [SerializeField] private Transform detectOrigin;
    [SerializeField] private float detectDistance = 8f;
    [SerializeField] private LayerMask targetLayers = ~0;

    [Header("Impact Sequence")]
    [SerializeField] private float slowMotionScale = 0.05f;
    [SerializeField] private float delayBeforeExplosion = 1f;
    [SerializeField] private bool disableControlScriptsOnImpact = true;

    [Header("Mission Complete UI")]
    [SerializeField] private ModalWindowManager finishPanel;
    [SerializeField] private float showFinishPanelDelay = 2f;

    private bool _hasExploded;
    private bool _hasSwitchedToImpactCam;
    private bool _impactSequenceStarted;

    private void Reset()
    {
        collisionDetector = GetComponent<ShipCollisionDetector>();
        explosionSequence = GetComponent<ExplosionSequence>();
        rb = GetComponent<Rigidbody>();
        detectOrigin = transform;
    }

    private void Start()
    {
        SetCam1Active();
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

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }

    private void Update()
    {
        if (_hasExploded || _impactSequenceStarted) return;
        if (!switchCamWhenNearTarget) return;
        if (_hasSwitchedToImpactCam) return;
        if (detectOrigin == null) return;

        if (Physics.Raycast(
                detectOrigin.position,
                detectOrigin.forward,
                out RaycastHit hit,
                detectDistance,
                targetLayers,
                QueryTriggerInteraction.Ignore))
        {
            SwitchToImpactCam();
        }
    }

    private void HandleShipHit(Collider other)
    {
        if (_hasExploded || _impactSequenceStarted) return;
        StartCoroutine(ImpactSequence());
    }

    private IEnumerator ImpactSequence()
    {
        _impactSequenceStarted = true;

        SwitchToImpactCam();

        if (disableControlScriptsOnImpact)
        {
            DisableOtherBehaviours();
        }

        Time.timeScale = slowMotionScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        yield return new WaitForSecondsRealtime(delayBeforeExplosion);

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        if (_hasExploded) yield break;

        _hasExploded = true;
        explosionSequence?.Play();

        yield return new WaitForSecondsRealtime(2f);

        finishPanel?.ModalWindowIn();

        Time.timeScale = 0f;
    }

    private void SwitchToImpactCam()
    {
        if (_hasSwitchedToImpactCam) return;
        _hasSwitchedToImpactCam = true;

        if (cam1 != null) cam1.Priority = cam1Priority;
        if (cam2 != null) cam2.Priority = cam2Priority;
    }

    private void SetCam1Active()
    {
        if (cam1 != null) cam1.Priority = cam2Priority;
        if (cam2 != null) cam2.Priority = cam1Priority;
    }

    private void DisableOtherBehaviours()
    {
        MonoBehaviour[] behaviours = GetComponents<MonoBehaviour>();

        foreach (var behaviour in behaviours)
        {
            if (behaviour == null) continue;
            if (behaviour == this) continue;
            if (behaviour == collisionDetector) continue;
            if (behaviour == explosionSequence) continue;

            behaviour.enabled = false;
        }
    }

    [ContextMenu("Explode")]
    public void ExplodeNow()
    {
        if (_hasExploded || _impactSequenceStarted) return;
        StartCoroutine(ImpactSequence());
    }

    private void OnDrawGizmosSelected()
    {
        if (detectOrigin == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(detectOrigin.position, detectOrigin.position + detectOrigin.forward * detectDistance);
    }
}