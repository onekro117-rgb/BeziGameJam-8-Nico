using UnityEngine;

/// <summary>
/// Main audio manager - Singleton
/// Provides easy access to music and SFX
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private AudioLibrary audioLibrary;

    [Header("Controllers")]
    private MusicController musicController;
    private SFXController sfxController;

    // Public accessors
    public AudioLibrary Library => audioLibrary;
    public MusicController Music => musicController;
    public SFXController SFX => sfxController;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Get or create controllers
        musicController = GetComponentInChildren<MusicController>();
        if (musicController == null)
        {
            GameObject musicObj = new GameObject("MusicController");
            musicObj.transform.SetParent(transform);
            musicController = musicObj.AddComponent<MusicController>();
        }

        sfxController = GetComponentInChildren<SFXController>();
        if (sfxController == null)
        {
            GameObject sfxObj = new GameObject("SFXController");
            sfxObj.transform.SetParent(transform);
            sfxController = sfxObj.AddComponent<SFXController>();
        }

        Debug.Log("<color=cyan>[AudioManager]</color> Initialized successfully");
    }

    // ========================================
    // CONVENIENCE METHODS - MUSIC
    // ========================================

    public void PlayMenuMusic()
    {
        if (audioLibrary.menuMusicLoop != null)
        {
            musicController.PlayLoop(audioLibrary.menuMusicLoop, fadeIn: true);
        }
    }

    public void PlayBattleMusic()
    {
        if (audioLibrary.battleMusicIntro != null && audioLibrary.battleMusicLoop != null)
        {
            musicController.PlayIntroThenLoop(audioLibrary.battleMusicIntro, audioLibrary.battleMusicLoop, fadeIn: true);
        }
    }

    public void StopMusic(bool fadeOut = true)
    {
        musicController.StopAllMusic(fadeOut);
    }

    // ========================================
    // CONVENIENCE METHODS - UI SFX
    // ========================================

    public void PlayButtonClick()
    {
        if (audioLibrary.buttonClick != null)
            sfxController.PlaySFX(audioLibrary.buttonClick);
    }

    public void PlayButtonHover()
    {
        if (audioLibrary.buttonHover != null)
            sfxController.PlaySFX(audioLibrary.buttonHover, volumeMultiplier: 0.5f);
    }

    // ========================================
    // CONVENIENCE METHODS - PLAYER SFX
    // ========================================

    public void PlayPlayerJump()
    {
        if (audioLibrary.playerJump != null)
            sfxController.PlaySFXWithPitchVariation(audioLibrary.playerJump, pitchVariation: 0.1f);
    }

    public void PlayPlayerDash()
    {
        if (audioLibrary.playerDash != null)
            sfxController.PlaySFX(audioLibrary.playerDash);
    }

    public void PlayPlayerHit()
    {
        if (audioLibrary.playerHit != null)
            sfxController.PlaySFX(audioLibrary.playerHit);
    }

    public void PlayPlayerDeath()
    {
        if (audioLibrary.playerDeath != null)
            sfxController.PlaySFX(audioLibrary.playerDeath);
    }

    // ========================================
    // CONVENIENCE METHODS - ENEMY SFX
    // ========================================

    public void PlayEnemyHit()
    {
        if (audioLibrary.enemyHit != null)
            sfxController.PlaySFXWithPitchVariation(audioLibrary.enemyHit, pitchVariation: 0.15f);
    }

    public void PlayEnemyDeath()
    {
        if (audioLibrary.enemyDeath != null)
            sfxController.PlaySFXWithPitchVariation(audioLibrary.enemyDeath, pitchVariation: 0.2f);
    }

    // ========================================
    // CONVENIENCE METHODS - MAGIC SFX
    // ========================================

    public void PlayFireCast()
    {
        if (audioLibrary.fireCast != null)
            sfxController.PlaySFX(audioLibrary.fireCast);
    }

    public void PlayIceCast()
    {
        if (audioLibrary.iceCast != null)
            sfxController.PlaySFX(audioLibrary.iceCast);
    }

    public void PlayLightningCast()
    {
        if (audioLibrary.lightningCast != null)
            sfxController.PlaySFX(audioLibrary.lightningCast);
    }

    public void PlayRingActivate()
    {
        if (audioLibrary.ringActivate != null)
            sfxController.PlaySFX(audioLibrary.ringActivate);
    }

    public void PlayRingTick()
    {
        if (audioLibrary.ringTick != null)
            sfxController.PlaySFX(audioLibrary.ringTick, volumeMultiplier: 0.4f);
    }

    // ========================================
    // CONVENIENCE METHODS - QTE SFX
    // ========================================

    public void PlayQTEPerfect()
    {
        if (audioLibrary.qtePerfect != null)
            sfxController.PlaySFX(audioLibrary.qtePerfect);
    }

    public void PlayQTEPartial()
    {
        if (audioLibrary.qtePartial != null)
            sfxController.PlaySFX(audioLibrary.qtePartial);
    }

    public void PlayQTEFail()
    {
        if (audioLibrary.qteFail != null)
            sfxController.PlaySFX(audioLibrary.qteFail);
    }

    // ========================================
    // CONVENIENCE METHODS - EVENT SFX
    // ========================================

    public void PlayWaveComplete()
    {
        if (audioLibrary.waveComplete != null)
            sfxController.PlaySFX(audioLibrary.waveComplete);
    }

    public void PlayWaveStart()
    {
        if (audioLibrary.waveStart != null)
            sfxController.PlaySFX(audioLibrary.waveStart);
    }

    public void PlayGameOver()
    {
        if (audioLibrary.gameOver != null)
            sfxController.PlaySFX(audioLibrary.gameOver);
    }

    // ========================================
    // VOLUME CONTROL
    // ========================================

    public void SetMusicVolume(float volume)
    {
        musicController.SetVolume(volume);
    }

    public void SetSFXVolume(float volume)
    {
        sfxController.SetVolume(volume);
    }

    public void SetMasterVolume(float volume)
    {
        AudioListener.volume = Mathf.Clamp01(volume);
    }
}
