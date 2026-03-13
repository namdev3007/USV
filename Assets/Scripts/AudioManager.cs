using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private float spatialBlend = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void PlaySfxAtPoint(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;

        GameObject go = new GameObject("SFX_" + clip.name);
        go.transform.position = position;

        AudioSource source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.spatialBlend = spatialBlend;
        source.Play();

        Destroy(go, clip.length + 0.1f);
    }
}