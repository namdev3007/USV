using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(USVEngine))]
public class SimpleEngineSound : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private USVEngine usvEngine;
    [SerializeField] private AudioSource audioSource;

    [Header("Sound")]
    [SerializeField] private AudioClip engineClip;
    [SerializeField] private float idleVolume = 0.2f;
    [SerializeField] private float maxVolume = 1f;
    [SerializeField] private float idlePitch = 0.8f;
    [SerializeField] private float maxPitch = 1.3f;

    [Header("Smoothing")]
    [SerializeField] private float volumeLerpSpeed = 5f;
    [SerializeField] private float pitchLerpSpeed = 5f;

    private void Reset()
    {
        usvEngine = GetComponent<USVEngine>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Awake()
    {
        if (usvEngine == null)
            usvEngine = GetComponent<USVEngine>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;

        if (engineClip != null)
            audioSource.clip = engineClip;

        if (audioSource.clip != null && !audioSource.isPlaying)
            audioSource.Play();
    }

    private void Update()
    {
        if (usvEngine == null || audioSource == null)
            return;

        float leftLoad = 0f;
        float rightLoad = 0f;

        if (usvEngine.leftEngine != null && usvEngine.leftEngine.isOn)
        {
            leftLoad = Mathf.Abs(usvEngine.leftEngine.throttleInput);
        }

        if (usvEngine.rightEngine != null && usvEngine.rightEngine.isOn)
        {
            rightLoad = Mathf.Abs(usvEngine.rightEngine.throttleInput);
        }

        float load = Mathf.Max(leftLoad, rightLoad);

        float targetVolume = Mathf.Lerp(idleVolume, maxVolume, load);
        float targetPitch = Mathf.Lerp(idlePitch, maxPitch, load);

        audioSource.volume = Mathf.Lerp(audioSource.volume, targetVolume, Time.deltaTime * volumeLerpSpeed);
        audioSource.pitch = Mathf.Lerp(audioSource.pitch, targetPitch, Time.deltaTime * pitchLerpSpeed);
    }
}