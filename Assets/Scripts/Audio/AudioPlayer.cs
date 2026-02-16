using UnityEngine;

[DisallowMultipleComponent]
public sealed class AudioPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource source;

    private void Awake()
    {
        if (source == null)
            source = GetComponent<AudioSource>();

        if (source == null)
            source = gameObject.AddComponent<AudioSource>();

        // Prototype-friendly defaults
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f; // 2D
    }

    public void PlayOneShot(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        if (source == null) return;

        source.PlayOneShot(clip, Mathf.Clamp01(volume));
    }
}
