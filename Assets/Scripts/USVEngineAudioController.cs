using UnityEngine;

[RequireComponent(typeof(USVEngine))]
public class USVEngineAudioController : MonoBehaviour
{
    [System.Serializable]
    public class EngineAudioLayerSet
    {
        public string name;
        public Transform audioPoint;

        [Header("Audio Sources")]
        public AudioSource idleSource;
        public AudioSource cruiseSource;
        public AudioSource highSource;
    }

    [Header("References")]
    [SerializeField] private USVEngine usvEngine;

    [Header("Left / Right Engine")]
    [SerializeField] private EngineAudioLayerSet leftEngineAudio;
    [SerializeField] private EngineAudioLayerSet rightEngineAudio;

    [Header("Diesel Clips")]
    [SerializeField] private AudioClip idleClip;
    [SerializeField] private AudioClip cruiseClip;
    [SerializeField] private AudioClip highClip;

    [Header("Volume")]
    [SerializeField] private float idleMaxVolume = 0.45f;
    [SerializeField] private float cruiseMaxVolume = 0.75f;
    [SerializeField] private float highMaxVolume = 1.0f;
    [SerializeField] private float twoEngineVolumeBoost = 1.1f;

    [Header("Pitch")]
    [SerializeField] private float idlePitchMin = 0.8f;
    [SerializeField] private float idlePitchMax = 1.0f;
    [SerializeField] private float cruisePitchMin = 0.9f;
    [SerializeField] private float cruisePitchMax = 1.15f;
    [SerializeField] private float highPitchMin = 1.0f;
    [SerializeField] private float highPitchMax = 1.3f;

    [Header("Response")]
    [SerializeField] private float volumeLerpSpeed = 8f;
    [SerializeField] private float pitchLerpSpeed = 8f;

    [Header("Water Effect")]
    [SerializeField] private float outOfWaterVolumeMultiplier = 0.8f;
    [SerializeField] private float outOfWaterPitchBoost = 0.08f;

    [Header("Audible Threshold")]
    [SerializeField] private float minNormalizedRPMToBeAudible = 0.02f;
    [SerializeField] private float minThrottleToBeAudible = 0.02f;

    private void Reset()
    {
        usvEngine = GetComponent<USVEngine>();
    }

    private void Awake()
    {
        if (usvEngine == null)
            usvEngine = GetComponent<USVEngine>();

        PrepareLayerSet(leftEngineAudio, "LeftEngine");
        PrepareLayerSet(rightEngineAudio, "RightEngine");
    }

    private void Update()
    {
        if (usvEngine == null) return;

        int activeEngineCount = GetAudibleEngineCount();

        UpdateEngineAudio(leftEngineAudio, usvEngine.leftEngine, activeEngineCount);
        UpdateEngineAudio(rightEngineAudio, usvEngine.rightEngine, activeEngineCount);
    }

    private void PrepareLayerSet(EngineAudioLayerSet set, string prefix)
    {
        if (set.audioPoint == null)
            set.audioPoint = transform;

        set.idleSource = CreateSourceIfMissing(set.idleSource, set.audioPoint, prefix + "_Idle");
        set.cruiseSource = CreateSourceIfMissing(set.cruiseSource, set.audioPoint, prefix + "_Cruise");
        set.highSource = CreateSourceIfMissing(set.highSource, set.audioPoint, prefix + "_High");

        SetupSource(set.idleSource, idleClip);
        SetupSource(set.cruiseSource, cruiseClip);
        SetupSource(set.highSource, highClip);
    }

    private AudioSource CreateSourceIfMissing(AudioSource source, Transform parent, string objectName)
    {
        if (source != null) return source;

        GameObject go = new GameObject(objectName);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = Vector3.zero;

        return go.AddComponent<AudioSource>();
    }

    private void SetupSource(AudioSource source, AudioClip clip)
    {
        if (source == null) return;

        source.clip = clip;
        source.loop = true;
        source.playOnAwake = false;
        source.spatialBlend = 1f;
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        source.minDistance = 3f;
        source.maxDistance = 45f;
        source.dopplerLevel = 0.2f;
        source.volume = 0f;
        source.pitch = 1f;

        if (clip != null && !source.isPlaying)
            source.Play();
    }

    private int GetAudibleEngineCount()
    {
        int count = 0;

        if (IsEngineAudible(usvEngine.leftEngine)) count++;
        if (IsEngineAudible(usvEngine.rightEngine)) count++;

        return count;
    }

    private bool IsEngineAudible(USVEngine.Engine engine)
    {
        if (engine == null || !engine.isOn) return false;

        float rpm01 = Mathf.InverseLerp(engine.minRPM, engine.maxRPM, engine.currentRPM);
        float throttle01 = Mathf.Abs(engine.throttleInput);

        return rpm01 > minNormalizedRPMToBeAudible || throttle01 > minThrottleToBeAudible;
    }

    private void UpdateEngineAudio(EngineAudioLayerSet layerSet, USVEngine.Engine engine, int activeEngineCount)
    {
        if (layerSet == null || engine == null) return;

        float rpm01 = Mathf.InverseLerp(engine.minRPM, engine.maxRPM, engine.currentRPM);
        float throttle01 = Mathf.Abs(engine.throttleInput);

        float load = Mathf.Max(rpm01, throttle01 * 0.9f);

        if (!engine.isOn)
            load = 0f;

        float idleWeight = 1f - Mathf.Clamp01(load * 1.25f);
        float cruiseWeight = 1f - Mathf.Abs(load - 0.5f) / 0.5f;
        float highWeight = Mathf.InverseLerp(0.6f, 1f, load);

        idleWeight = Mathf.Clamp01(idleWeight);
        cruiseWeight = Mathf.Clamp01(cruiseWeight);
        highWeight = Mathf.Clamp01(highWeight);

        float engineCountMultiplier = activeEngineCount >= 2 ? twoEngineVolumeBoost : 1f;

        float waterVolumeMultiplier = engine.isSubmerged ? 1f : outOfWaterVolumeMultiplier;
        float waterPitchBoost = engine.isSubmerged ? 0f : outOfWaterPitchBoost;

        float idleTargetVolume = idleWeight * idleMaxVolume * engineCountMultiplier * waterVolumeMultiplier;
        float cruiseTargetVolume = cruiseWeight * cruiseMaxVolume * engineCountMultiplier * waterVolumeMultiplier;
        float highTargetVolume = highWeight * highMaxVolume * engineCountMultiplier * waterVolumeMultiplier;

        float idleTargetPitch = Mathf.Lerp(idlePitchMin, idlePitchMax, rpm01) + waterPitchBoost;
        float cruiseTargetPitch = Mathf.Lerp(cruisePitchMin, cruisePitchMax, rpm01) + waterPitchBoost;
        float highTargetPitch = Mathf.Lerp(highPitchMin, highPitchMax, rpm01) + waterPitchBoost;

        ApplyToSource(layerSet.idleSource, idleTargetVolume, idleTargetPitch);
        ApplyToSource(layerSet.cruiseSource, cruiseTargetVolume, cruiseTargetPitch);
        ApplyToSource(layerSet.highSource, highTargetVolume, highTargetPitch);
    }

    private void ApplyToSource(AudioSource source, float targetVolume, float targetPitch)
    {
        if (source == null) return;

        source.volume = Mathf.Lerp(source.volume, targetVolume, Time.deltaTime * volumeLerpSpeed);
        source.pitch = Mathf.Lerp(source.pitch, targetPitch, Time.deltaTime * pitchLerpSpeed);
    }
}