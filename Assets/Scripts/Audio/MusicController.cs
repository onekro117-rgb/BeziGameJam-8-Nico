using System.Collections;
using UnityEngine;

/// <summary>
/// Handles background music with intro/loop support
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class MusicController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float fadeOutDuration = 1f;
    [SerializeField] private float defaultVolume = 0.5f;

    private AudioSource musicSource;
    private AudioSource loopSource;
    private Coroutine fadeCoroutine;
    private Coroutine transitionCoroutine;

    private void Awake()
    {
        // Main music source
        musicSource = GetComponent<AudioSource>();
        musicSource.loop = false;
        musicSource.playOnAwake = false;
        musicSource.volume = defaultVolume;

        // Create second source for seamless loop transitions
        GameObject loopObj = new GameObject("Loop Source");
        loopObj.transform.SetParent(transform);
        loopSource = loopObj.AddComponent<AudioSource>();
        loopSource.loop = true;
        loopSource.playOnAwake = false;
        loopSource.volume = defaultVolume;
    }

    /// <summary>
    /// Play simple looping music
    /// </summary>
    public void PlayLoop(AudioClip loopClip, bool fadeIn = true)
    {
        if (loopClip == null)
        {
            Debug.LogWarning("MusicController: Trying to play null loop clip");
            return;
        }

        StopAllMusic();

        musicSource.clip = loopClip;
        musicSource.loop = true;

        if (fadeIn)
        {
            musicSource.volume = 0f;
            musicSource.Play();
            FadeIn();
        }
        else
        {
            musicSource.volume = defaultVolume;
            musicSource.Play();
        }

        Debug.Log($"<color=cyan>[Music]</color> Playing loop: {loopClip.name}");
    }

    /// <summary>
    /// Play music with intro then loop (for battle music)
    /// </summary>
    public void PlayIntroThenLoop(AudioClip introClip, AudioClip loopClip, bool fadeIn = true)
    {
        if (introClip == null || loopClip == null)
        {
            Debug.LogWarning("MusicController: Trying to play null intro/loop clips");
            return;
        }

        StopAllMusic();

        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(IntroToLoopCoroutine(introClip, loopClip, fadeIn));

        Debug.Log($"<color=cyan>[Music]</color> Playing intro: {introClip.name} → loop: {loopClip.name}");
    }

    private IEnumerator IntroToLoopCoroutine(AudioClip introClip, AudioClip loopClip, bool fadeIn)
    {
        // Play intro
        musicSource.clip = introClip;
        musicSource.loop = false;

        if (fadeIn)
        {
            musicSource.volume = 0f;
            musicSource.Play();
            yield return StartCoroutine(FadeCoroutine(musicSource, 0f, defaultVolume, fadeInDuration));
        }
        else
        {
            musicSource.volume = defaultVolume;
            musicSource.Play();
        }

        // Wait for intro to finish
        yield return new WaitForSeconds(introClip.length);

        // Start loop
        musicSource.clip = loopClip;
        musicSource.loop = true;
        musicSource.Play();
    }

    /// <summary>
    /// Stop all music with optional fade out
    /// </summary>
    public void StopAllMusic(bool fadeOut = true)
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }

        if (fadeOut && musicSource.isPlaying)
        {
            FadeOut();
        }
        else
        {
            musicSource.Stop();
            loopSource.Stop();
        }
    }

    /// <summary>
    /// Fade in current music
    /// </summary>
    public void FadeIn()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeCoroutine(musicSource, musicSource.volume, defaultVolume, fadeInDuration));
    }

    /// <summary>
    /// Fade out current music
    /// </summary>
    public void FadeOut()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeCoroutine(musicSource, musicSource.volume, 0f, fadeOutDuration));
    }

    private IEnumerator FadeCoroutine(AudioSource source, float startVolume, float targetVolume, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // Use unscaled for pause compatibility
            float t = elapsed / duration;
            source.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }

        source.volume = targetVolume;

        if (targetVolume == 0f)
        {
            source.Stop();
        }

        fadeCoroutine = null;
    }

    /// <summary>
    /// Set music volume
    /// </summary>
    public void SetVolume(float volume)
    {
        defaultVolume = Mathf.Clamp01(volume);
        musicSource.volume = defaultVolume;
        loopSource.volume = defaultVolume;
    }
}
