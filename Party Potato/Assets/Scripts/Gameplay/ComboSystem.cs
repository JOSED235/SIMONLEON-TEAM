using UnityEngine;

/// <summary>
/// Gestiona las rachas de aciertos consecutivos por jugador en Beat Potato (Papa Beat).
/// Escucha <see cref="GameEvents.OnHitResolved"/> y <see cref="GameEvents.OnPlayerMiss"/> /
/// <see cref="GameEvents.OnNoteMiss"/> para mantener contadores independientes.
///
/// Regla oficial (GDD):
/// - Cualquier HitResult >= Good (Good, Great, Perfect) suma +1 a la racha del jugador.
/// - Un Miss reinicia la racha a 0 y publica <see cref="GameEvents.OnStreakReset"/>.
/// - Al alcanzar <see cref="GameConfig.streakThresholdForPower"/> aciertos consecutivos
///   el jugador obtiene UNA carga de poder; la carga no se acumula.
/// - Publica <see cref="GameEvents.OnStreakUpdated"/> tras cada cambio de racha.
///
/// Emisor de: OnStreakUpdated, OnStreakReset, OnPowerUnlocked.
/// Consumidores esperados: GameUI, PowerUpManager.
/// </summary>
public class ComboSystem : MonoBehaviour
{
    [Header("Configuración de Balance")]
    [Tooltip("Asset de configuración con el umbral de racha para desbloquear un poder.")]
    public GameConfig config;

    // Rachas actuales independientes por jugador
    private int streak1 = 0;
    private int streak2 = 0;

    // Indica si el jugador ya tiene una carga de poder pendiente de usar
    private bool powerReady1 = false;
    private bool powerReady2 = false;

    // ── Propiedades de solo lectura para tests y UI ────────────────────────
    public int Streak1 { get { return streak1; } }
    public int Streak2 { get { return streak2; } }
    public bool PowerReady1 { get { return powerReady1; } }
    public bool PowerReady2 { get { return powerReady2; } }

    // ── Ciclo de vida ──────────────────────────────────────────────────────
    private void Awake()
    {
        if (config == null)
        {
            config = ScriptableObject.CreateInstance<GameConfig>();
            Debug.LogWarning("[ComboSystem] No se asignó GameConfig en el Inspector. Usando valores predeterminados.");
        }
    }

    private void OnEnable()
    {
        GameEvents.OnHitResolved += HandleHitResolved;
        GameEvents.OnPlayerMiss  += HandlePlayerMiss;
        GameEvents.OnNoteMiss    += HandleNoteMiss;
        GameEvents.OnRoundStarted += HandleRoundStarted;
    }

    private void OnDisable()
    {
        GameEvents.OnHitResolved  -= HandleHitResolved;
        GameEvents.OnPlayerMiss   -= HandlePlayerMiss;
        GameEvents.OnNoteMiss     -= HandleNoteMiss;
        GameEvents.OnRoundStarted -= HandleRoundStarted;
    }

    // ── Manejadores de eventos ─────────────────────────────────────────────

    /// <summary>
    /// Procesa el resultado de un golpe evaluado por el sistema de ritmo.
    /// Good, Great y Perfect incrementan la racha; Miss la reinicia.
    /// </summary>
    private void HandleHitResolved(PlayerID player, HitResult result, float accuracy)
    {
        if (result == HitResult.Miss)
        {
            ResetStreak(player);
        }
        else
        {
            // Good, Great o Perfect
            IncrementStreak(player);
        }
    }

    /// <summary>
    /// El jugador no pulsó a tiempo. Reinicia la racha.
    /// </summary>
    private void HandlePlayerMiss(PlayerID player)
    {
        ResetStreak(player);
    }

    /// <summary>
    /// Una nota venció sin ser pulsada (auto-miss). Reinicia la racha.
    /// </summary>
    private void HandleNoteMiss(PlayerID player, int laneID)
    {
        ResetStreak(player);
    }

    /// <summary>
    /// Al comenzar una nueva ronda se reinician rachas y cargas de poder.
    /// Garantiza un estado limpio sin remanentes de la ronda anterior.
    /// </summary>
    private void HandleRoundStarted(int roundNumber)
    {
        streak1 = 0;
        streak2 = 0;
        powerReady1 = false;
        powerReady2 = false;
    }

    // ── Lógica de racha ────────────────────────────────────────────────────

    private void IncrementStreak(PlayerID player)
    {
        int threshold = (config != null) ? config.streakThresholdForPower : 5;

        if (player == PlayerID.Player1)
        {
            streak1++;
            bool nowReady = streak1 >= threshold && !powerReady1;
            if (nowReady)
            {
                powerReady1 = true;
                GameEvents.TriggerPowerUnlocked(PlayerID.Player1, PowerType.None);
                Debug.Log("[ComboSystem] ¡Poder desbloqueado para Jugador 1! Racha: " + streak1);
            }
            GameEvents.TriggerStreakUpdated(PlayerID.Player1, streak1, powerReady1);
        }
        else if (player == PlayerID.Player2)
        {
            streak2++;
            bool nowReady = streak2 >= threshold && !powerReady2;
            if (nowReady)
            {
                powerReady2 = true;
                GameEvents.TriggerPowerUnlocked(PlayerID.Player2, PowerType.None);
                Debug.Log("[ComboSystem] ¡Poder desbloqueado para Jugador 2! Racha: " + streak2);
            }
            GameEvents.TriggerStreakUpdated(PlayerID.Player2, streak2, powerReady2);
        }
    }

    private void ResetStreak(PlayerID player)
    {
        if (player == PlayerID.Player1)
        {
            if (streak1 == 0) return; // ya estaba en 0, no emitir evento redundante
            streak1 = 0;
            GameEvents.TriggerStreakReset(PlayerID.Player1);
            GameEvents.TriggerStreakUpdated(PlayerID.Player1, 0, powerReady1);
        }
        else if (player == PlayerID.Player2)
        {
            if (streak2 == 0) return;
            streak2 = 0;
            GameEvents.TriggerStreakReset(PlayerID.Player2);
            GameEvents.TriggerStreakUpdated(PlayerID.Player2, 0, powerReady2);
        }
    }

    /// <summary>
    /// Consume la carga de poder de un jugador (llamado por PowerUpManager al activar el poder).
    /// </summary>
    public void ConsumePower(PlayerID player)
    {
        if (player == PlayerID.Player1)
        {
            powerReady1 = false;
        }
        else if (player == PlayerID.Player2)
        {
            powerReady2 = false;
        }
    }
}
