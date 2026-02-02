using UnityEngine;

/// <summary>
/// ScriptableObject that stores all audio clips
/// Acts as a centralized audio database
/// </summary>
[CreateAssetMenu(fileName = "AudioLibrary", menuName = "Audio/Audio Library")]
public class AudioLibrary : ScriptableObject
{
    [Header("=== MUSIC ===")]
    [Header("Menu Music")]
    public AudioClip menuMusicLoop;

    [Header("Battle Music")]
    public AudioClip battleMusicIntro;
    public AudioClip battleMusicLoop;

    [Header("=== SFX ===")]
    [Header("UI Sounds")]
    public AudioClip buttonClick;
    public AudioClip buttonHover;

    [Header("Player Movement")]
    public AudioClip playerJump;
    public AudioClip playerDash;

    [Header("Combat - Player")]
    public AudioClip playerHit;
    public AudioClip playerDeath;

    [Header("Combat - Enemy")]
    public AudioClip enemyHit;
    public AudioClip enemyDeath;

    [Header("Magic - Fire")]
    public AudioClip fireCast;
    public AudioClip fireImpact;

    [Header("Magic - Ice")]
    public AudioClip iceCast;
    public AudioClip iceImpact;

    [Header("Magic - Lightning")]
    public AudioClip lightningCast;
    public AudioClip lightningImpact;

    [Header("Magic - Ring")]
    public AudioClip ringActivate;
    public AudioClip ringTick;

    [Header("QTE")]
    public AudioClip qtePerfect;
    public AudioClip qtePartial;
    public AudioClip qteFail;

    [Header("Events")]
    public AudioClip waveComplete;
    public AudioClip waveStart;
    public AudioClip gameOver;
}
