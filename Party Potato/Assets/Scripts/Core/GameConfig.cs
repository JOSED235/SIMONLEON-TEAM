using UnityEngine;

/// <summary>
/// Configuración de balance central de Papa Vóley almacenada como ScriptableObject.
/// Centraliza tolerancias de golpe, duraciones de viaje, umbrales de racha y reglas de victoria.
/// </summary>
[CreateAssetMenu(fileName = "GameConfig", menuName = "PapaVoley/GameConfig", order = 1)]
public class GameConfig : ScriptableObject
{
    [Header("Ventanas de Precisión Temporal (en segundos)")]
    [Tooltip("Tolerancia máxima para golpe 'Perfecto' (±70 ms según diseño).")]
    [Range(0.02f, 0.15f)]
    public float perfectTolerance = 0.07f;

    [Tooltip("Tolerancia máxima para golpe 'Bien' (±150 ms según diseño).")]
    [Range(0.08f, 0.30f)]
    public float goodTolerance = 0.15f;

    [Tooltip("Tolerancia reducida para golpe 'Perfecto' bajo efecto de Papa Eléctrica (±50 ms).")]
    [Range(0.02f, 0.10f)]
    public float electricPerfectTolerance = 0.05f;

    [Header("Viaje de la Papa y Tiempos de Ritmo")]
    [Tooltip("Cantidad de beats que tarda la papa en viajar de un jugador al otro por defecto.")]
    [Range(1f, 4f)]
    public float defaultTravelBeats = 2f;

    [Tooltip("Multiplicador de velocidad de viaje al activar ¡Papa Cohete! (ej. 1.25 = 25% más rápido).")]
    [Range(1.1f, 1.5f)]
    public float rocketSpeedMultiplier = 1.25f;

    [Tooltip("Beats de cuenta regresiva antes del primer saque de cada ronda.")]
    [Range(1, 8)]
    public int countdownBeats = 4;

    [Header("Rachas y Carga de Poderes")]
    [Tooltip("Cantidad de aciertos perfectos consecutivos requeridos para desbloquear una carga de poder.")]
    [Range(3, 10)]
    public int streakThresholdForPower = 5;

    [Tooltip("Duración en golpes bonificados del efecto de la Papa Dorada.")]
    [Range(1, 5)]
    public int goldenBonusHits = 2;

    [Header("Sistema de Calor")]
    [Tooltip("Incremento de calor normalizado por cada intercambio exitoso (0..1).")]
    [Range(0.01f, 0.20f)]
    public float heatGainPerHit = 0.05f;

    [Tooltip("Incremento adicional de calor para la papa cuando el rival comete un fallo.")]
    [Range(0.05f, 0.30f)]
    public float heatGainOnMiss = 0.15f;

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
