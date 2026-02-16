using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Audio Config", fileName = "AudioConfig")]
public sealed class AudioConfigSO : ScriptableObject
{
    [Header("Clips (exactly these 4 scenarios)")]
    public AudioClip flip;
    public AudioClip match;
    public AudioClip mismatch;
    public AudioClip gameOver;

    [Header("Volume")]
    [Range(0f, 1f)] public float volume = 1f;

    public bool IsValid(out string reason)
    {
        if (flip == null) { reason = "Missing flip clip"; return false; }
        if (match == null) { reason = "Missing match clip"; return false; }
        if (mismatch == null) { reason = "Missing mismatch clip"; return false; }
        if (gameOver == null) { reason = "Missing gameOver clip"; return false; }

        reason = null;
        return true;
    }
}
