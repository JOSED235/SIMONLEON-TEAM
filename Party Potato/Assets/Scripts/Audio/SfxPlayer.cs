using UnityEngine;

/// <summary>
/// Reproductor centralizado de efectos de sonido (SFX).
/// Escucha el bus de GameEvents y reproduce los clips sin acoplarse a otros scripts.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SfxPlayer : MonoBehaviour
{
    [Header("Configuración de Audio")]
    public AudioSource sfxSource;

    [Header("Clips Rítmicos")]
    public AudioClip perfectHitClip;
    public AudioClip greatHitClip;
    public AudioClip goodHitClip;
    public AudioClip missClip;

    [Header("Clips de Eventos")]
    public AudioClip explosionClip;
    public AudioClip powerUnlockedClip;
    public AudioClip powerActivatedClip;

    private void Awake()
    {
        if (sfxSource == null) sfxSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        // Suscribirse a todo lo que hace ruido
        GameEvents.OnNoteHit += HandleNoteHit;
        GameEvents.OnNoteMiss += HandleNoteMiss;
        GameEvents.OnPlayerMiss += HandlePlayerMiss;
        GameEvents.OnPotatoExploded += HandleExploded;
        GameEvents.OnPowerUnlocked += HandlePowerUnlocked;
        GameEvents.OnPowerActivated += HandlePowerActivated;
    }

    private void OnDisable()
    {
        GameEvents.OnNoteHit -= HandleNoteHit;
        GameEvents.OnNoteMiss -= HandleNoteMiss;
        GameEvents.OnPlayerMiss -= HandlePlayerMiss;
        GameEvents.OnPotatoExploded -= HandleExploded;
        GameEvents.OnPowerUnlocked -= HandlePowerUnlocked;
        GameEvents.OnPowerActivated -= HandlePowerActivated;
    }

    private void PlayClip(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            // PlayOneShot permite superponer sonidos (ej. si dos notas suenan muy rápido)
            sfxSource.PlayOneShot(clip);
        }
    }

    private void HandleNoteHit(PlayerID player, int laneID, HitResult result, float accuracy)
    {
        switch (result)
        {
            case HitResult.Perfect: PlayClip(perfectHitClip); break;
            case HitResult.Great: PlayClip(greatHitClip); break;
            case HitResult.Good: PlayClip(goodHitClip); break;
        }
    }

    private void HandleNoteMiss(PlayerID player, int laneID) { PlayClip(missClip); }
    private void HandlePlayerMiss(PlayerID player) { PlayClip(missClip); }
    private void HandleExploded(PlayerID loser) { PlayClip(explosionClip); }
    private void HandlePowerUnlocked(PlayerID player, PowerType power) { PlayClip(powerUnlockedClip); }
    private void HandlePowerActivated(PlayerID player, PowerType power) { PlayClip(powerActivatedClip); }
}
