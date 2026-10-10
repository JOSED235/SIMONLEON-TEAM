using UnityEngine;

/// <summary>
/// Configuración de balance central de Beat Potato (Papa Beat) almacenada como ScriptableObject.
/// Centraliza tolerancias de golpe (Perfect, Great, Good), sistema de calor, rachas y reglas de partida.
/// </summary>
[CreateAssetMenu(fileName = "GameConfig", menuName = "BeatPotato/GameConfig", order = 1)]
public class GameConfig : ScriptableObject
{
    [Header("Ventanas de Precisión Temporal (en segundos)")]
    [Tooltip("Tolerancia máxima para golpe 'Perfect' (±50 ms según GDD).")]
    [Range(0.02f, 0.08f)]
    public float perfectTolerance = 0.05f;

    [Tooltip("Tolerancia máxima para golpe 'Great' (±100 ms según GDD).")]
    [Range(0.06f, 0.12f)]
    public float greatTolerance = 0.10f;

    [Tooltip("Tolerancia máxima para golpe 'Good' (±150 ms según GDD).")]
    [Range(0.10f, 0.25f)]
    public float goodTolerance = 0.15f;

    [Tooltip("Tolerancia reducida para golpe bajo efecto de Papa Eléctrica (±30 ms).")]
    [Range(0.02f, 0.08f)]
    public float electricPerfectTolerance = 0.03f;

    [Header("Sistema de Calor y Explosión de la Papa")]
    [Tooltip("Incremento de calor por cometer un fallo (+20% según GDD).")]
    [Range(0.10f, 0.30f)]
    public float heatGainOnMiss = 0.20f;

    [Tooltip("Reducción de calor propio al lograr un golpe 'Perfect' (-5%).")]
    [Range(0.01f, 0.10f)]
    public float heatCooldownOnPerfect = 0.05f;

    [Tooltip("Umbral de calor crítico donde la papa empieza a ponerse roja y emitir humo (60%).")]
    [Range(0.40f, 0.80f)]
    public float heatThresholdCritical = 0.60f;

    [Tooltip("Umbral de calor que detona la papa y finaliza la ronda (100% = 1.0).")]
    [Range(0.80f, 1.0f)]
    public float heatThresholdExplosion = 1.0f;

    [Tooltip("Reducción de calor aplicada por el poder Papa Helada (40% según GDD).")]
    [Range(0.20f, 0.60f)]
    public float freezePotatoHeatReduction = 0.40f;

    [Header("Rachas y Sabotaje")]
    [Tooltip("Cantidad de aciertos consecutivos requeridos para desbloquear un poder (5 según GDD).")]
    [Range(3, 10)]
    public int streakThresholdForPower = 5;

    [Tooltip("Multiplicador de velocidad de caída de notas bajo efecto de Papa Cohete.")]
    [Range(1.2f, 2.0f)]
    public float rocketSpeedMultiplier = 1.50f;

    [Tooltip("Duración en segundos de la invisibilidad de notas por Papa Fantasma.")]
    [Range(1.0f, 5.0f)]
    public float ghostEffectDuration = 3.0f;

    [Tooltip("Cantidad de compases de duración para el efecto de Papa Cohete.")]
    [Range(1, 8)]
    public int rocketDurationMeasures = 4;

    [Header("Ritmo y Viaje de la Papa")]
    [Tooltip("Cantidad de beats que tarda la papa en oscilar de un extremo al otro.")]
    [Range(1f, 4f)]
    public float defaultTravelBeats = 2.0f;

    [Tooltip("Beats de cuenta regresiva antes de habilitar el flujo de notas.")]
    [Range(1, 8)]
    public int countdownBeats = 4;

    [Header("Estructura de Partida y Victoria")]
    [Tooltip("Rondas necesarias para ganar la partida (ej. 2 para formato al mejor de 3).")]
    [Range(1, 5)]
    public int roundsToWinMatch = 2;

    [Tooltip("Segundos de espera tras la explosión de la papa antes de comenzar la siguiente ronda.")]
    [Range(1.0f, 5.0f)]
    public float delayBeforeNextRound = 2.5f;

    [Tooltip("Segundos de espera antes de mostrar la pantalla final de revancha.")]
    [Range(1.0f, 5.0f)]
    public float delayBeforeGameOver = 3.0f;
}
