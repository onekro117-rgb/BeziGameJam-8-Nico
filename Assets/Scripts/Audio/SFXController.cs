using UnityEngine;

/// <summary>
/// Handles sound effects playback
/// </summary>
public class SFXController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float defaultVolume = 0.7f;
    [SerializeField] private int maxSimultaneousSFX = 10;

    [Header("Audio Source Pool")]
    [SerializeField] private GameObject audioSourcePrefab;

    private AudioSource[] sfxPool;
    private int currentPoolIndex = 0;

    private void Awake()
    {
        CreateAudioSourcePool();
    }

    private void CreateAudioSourcePool()
    {
        sfxPool = new AudioSource[maxSimultaneousSFX];

        for (int i = 0; i < maxSimultaneousSFX; i++)
        {
            GameObject obj = new GameObject($"SFX_Source_{i}");
            obj.transform.SetParent(transform);

            AudioSource source = obj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.volume = defaultVolume;

            sfxPool[i] = source;
        }

        Debug.Log($"<color=green>[SFX]</color> Audio source pool created with {maxSimultaneousSFX} sources");
    }

    /// <summary>
    /// Play a sound effect
    /// </summary>
    public void PlaySFX(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (clip == null)
        {
            Debug.LogWarning("SFXController: Trying to play null clip");
            return;
        }

        AudioSource source = GetAvailableSource();
        source.clip = clip;
        source.volume = defaultVolume * volumeMultiplier;
        source.Play();
    }

    /// <summary>
    /// Play a sound effect with random pitch variation
    /// </summary>
    public void PlaySFXWithPitchVariation(AudioClip clip, float pitchVariation = 0.1f, float volumeMultiplier = 1f)
    {
        if (clip == null)
        {
            Debug.LogWarning("SFXController: Trying to play null clip");
            return;
        }

        AudioSource source = GetAvailableSource();
        source.clip = clip;
        source.volume = defaultVolume * volumeMultiplier;
        source.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        source.Play();

        // Reset pitch after playing
        source.pitch = 1f;
    }

    /// <summary>
    /// Play one of several variations randomly
    /// </summary>
    public void PlaySFXVariation(AudioClip[] clips, float volumeMultiplier = 1f)
    {
        if (clips == null || clips.Length == 0)
        {
            Debug.LogWarning("SFXController: No clips provided for variation");
            return;
        }

        AudioClip randomClip = clips[Random.Range(0, clips.Length)];
        PlaySFX(randomClip, volumeMultiplier);
    }

    /// <summary>
    /// Play SFX at a specific position (3D sound)
    /// </summary>
    public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volumeMultiplier = 1f)
    {
        if (clip == null) return;

        AudioSource.PlayClipAtPoint(clip, position, defaultVolume * volumeMultiplier);
    }

    private AudioSource GetAvailableSource()
    {
        // Round-robin selection
        AudioSource source = sfxPool[currentPoolIndex];
        currentPoolIndex = (currentPoolIndex + 1) % maxSimultaneousSFX;

        return source;
    }

    /// <summary>
    /// Set SFX volume
    /// </summary>
    public void SetVolume(float volume)
    {
        defaultVolume = Mathf.Clamp01(volume);

        foreach (var source in sfxPool)
        {
            source.volume = defaultVolume;
        }
    }

    /// <summary>
    /// Stop all playing SFX
    /// </summary>
    public void StopAllSFX()
    {
        foreach (var source in sfxPool)
        {
            source.Stop();
        }
    }
}