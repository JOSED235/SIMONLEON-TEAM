using System.Collections;
using UnityEngine;

/// <summary>
/// Gestiona la activación, duración y expiración de los poderes especiales en Beat Potato.
///
/// Responsabilidad de este sistema (Backend):
/// - Validar que el jugador tenga una carga disponible antes de activar.
/// - Consumir la carga a través de <see cref="ComboSystem.ConsumePower"/>.
/// - Emitir <see cref="GameEvents.OnPowerActivated"/> al activar y
///   <see cref="GameEvents.OnPowerExpired"/> al expirar.
/// - Gestionar el temporizador de duración según el tipo de poder y <see cref="GameConfig"/>.
/// - Garantizar que un jugador solo tenga un poder activo a la vez.
///
/// Efectos visuales y sobre notas (Papa Cohete, Papa Fantasma, etc.) son
/// responsabilidad de NoteSpawner / NoteObject y GameUI — este sistema solo
/// notifica el inicio y fin del efecto para mantener la separación de módulos.
///
/// Prioridad según GDD: Papa Cohete (1), Papa Fantasma (1), Papa Eléctrica (2), Papa Helada (2).
///
/// Emisor de: OnPowerActivated, OnPowerExpired.
/// Consumidores de: OnPowerActivated → NoteSpawner, RhythmManager, GameUI.
/// </summary>
public class PowerUpManager : MonoBehaviour
{
    [Header("Configuración de Balance")]
    [Tooltip("Asset de configuración con duraciones y multiplicadores de poderes.")]
    public GameConfig config;

    [Header("Referencias")]
    [Tooltip("ComboSystem del que se consume la carga de poder.")]
    public ComboSystem comboSystem;

    // Seguimiento de poder activo por jugador
    private PowerType activePower1 = PowerType.None;
    private PowerType activePower2 = PowerType.None;
    private Coroutine expireCoroutine1 = null;
    private Coroutine expireCoroutine2 = null;

    // Propiedades de solo lectura para tests / UI
    public PowerType ActivePower1 { get { return activePower1; } }
    public PowerType ActivePower2 { get { return activePower2; } }

    // ── Ciclo de vida ──────────────────────────────────────────────────────

    private void Awake()
    {
        if (config == null)
        {
            config = ScriptableObject.CreateInstance<GameConfig>();
            Debug.LogWarning("[PowerUpManager] No se asignó GameConfig. Usando valores predeterminados.");
        }
    }

    private void OnEnable()
    {
        GameEvents.OnPowerUnlocked += HandlePowerUnlocked;
        GameEvents.OnRoundStarted  += HandleRoundStarted;
    }

    private void OnDisable()
    {
        GameEvents.OnPowerUnlocked -= HandlePowerUnlocked;
        GameEvents.OnRoundStarted  -= HandleRoundStarted;
    }

    // ── Manejadores de eventos ─────────────────────────────────────────────

    /// <summary>
    /// Cuando ComboSystem desbloquea un poder, asigna el tipo correspondiente
    /// según la prioridad del GDD (primera carga = Papa Cohete para simplicidad de jam).
    /// El tipo concreto puede extenderse con lógica de selección de poder.
    /// </summary>
    private void HandlePowerUnlocked(PlayerID player, PowerType suggestedPower)
    {
        // Si el sugerido es None, asignamos Papa Cohete como poder por defecto (prioridad 1 del GDD).
        // PowerUpManager no activa el poder aquí: solo reserva el tipo.
        // La activación ocurre cuando el jugador presiona su tecla (llamada a TryActivatePower).
        Debug.Log(string.Format("[PowerUpManager] Poder listo para {0}: {1}",
            player,
            suggestedPower == PowerType.None ? PowerType.RocketPotato : suggestedPower));
    }

    /// <summary>
    /// Al iniciar una nueva ronda, cancela efectos activos y reinicia el estado.
    /// </summary>
    private void HandleRoundStarted(int roundNumber)
    {
        CancelActivePower(PlayerID.Player1);
        CancelActivePower(PlayerID.Player2);
    }

    // ── API Pública ────────────────────────────────────────────────────────

    /// <summary>
    /// Intenta activar el poder del jugador.
    /// Llamado desde PlayerInput al detectar la tecla de poder (Enter = P1, Espacio = P2).
    /// </summary>
    /// <param name="player">Jugador que intenta activar.</param>
    /// <param name="powerType">Tipo de poder a activar.</param>
    /// <returns>True si la activación fue exitosa; false si no hay carga o ya hay poder activo.</returns>
    public bool TryActivatePower(PlayerID player, PowerType powerType)
    {
        if (powerType == PowerType.None) return false;
        if (!HasPowerReady(player)) return false;
        if (HasActivePower(player)) return false;

        // Consumir carga en ComboSystem
        if (comboSystem != null)
            comboSystem.ConsumePower(player);

        ActivatePower(player, powerType);
        return true;
    }

    // ── Lógica interna ─────────────────────────────────────────────────────

    private void ActivatePower(PlayerID player, PowerType powerType)
    {
        float duration = GetDuration(powerType);

        if (player == PlayerID.Player1)
        {
            activePower1 = powerType;
            if (expireCoroutine1 != null) StopCoroutine(expireCoroutine1);
            expireCoroutine1 = StartCoroutine(ExpireAfter(player, powerType, duration));
        }
        else if (player == PlayerID.Player2)
        {
            activePower2 = powerType;
            if (expireCoroutine2 != null) StopCoroutine(expireCoroutine2);
            expireCoroutine2 = StartCoroutine(ExpireAfter(player, powerType, duration));
        }

        Debug.Log(string.Format("[PowerUpManager] {0} activó {1} (duración: {2}s)", player, powerType, duration));
        GameEvents.TriggerPowerActivated(player, powerType);
    }

    private IEnumerator ExpireAfter(PlayerID player, PowerType powerType, float duration)
    {
        yield return new WaitForSeconds(duration);
        ExpirePower(player, powerType);
    }

    private void ExpirePower(PlayerID player, PowerType powerType)
    {
        if (player == PlayerID.Player1)
        {
            activePower1 = PowerType.None;
            expireCoroutine1 = null;
        }
        else if (player == PlayerID.Player2)
        {
            activePower2 = PowerType.None;
            expireCoroutine2 = null;
        }

        Debug.Log(string.Format("[PowerUpManager] {0} expiró para {1}", powerType, player));
        GameEvents.TriggerPowerExpired(powerType);
    }

    /// <summary>
    /// Cancela inmediatamente el poder activo de un jugador (ej. al reiniciar ronda).
    /// </summary>
    private void CancelActivePower(PlayerID player)
    {
        if (player == PlayerID.Player1)
        {
            if (activePower1 != PowerType.None)
            {
                PowerType cancelled = activePower1;
                activePower1 = PowerType.None;
                if (expireCoroutine1 != null) { StopCoroutine(expireCoroutine1); expireCoroutine1 = null; }
                GameEvents.TriggerPowerExpired(cancelled);
            }
        }
        else if (player == PlayerID.Player2)
        {
            if (activePower2 != PowerType.None)
            {
                PowerType cancelled = activePower2;
                activePower2 = PowerType.None;
                if (expireCoroutine2 != null) { StopCoroutine(expireCoroutine2); expireCoroutine2 = null; }
                GameEvents.TriggerPowerExpired(cancelled);
            }
        }
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private bool HasPowerReady(PlayerID player)
    {
        if (comboSystem == null) return false;
        return player == PlayerID.Player1 ? comboSystem.PowerReady1 : comboSystem.PowerReady2;
    }

    private bool HasActivePower(PlayerID player)
    {
        return player == PlayerID.Player1
            ? activePower1 != PowerType.None
            : activePower2 != PowerType.None;
    }

    /// <summary>
    /// Devuelve la duración en segundos para cada tipo de poder según el GDD y GameConfig.
    /// Papa Cohete: dura <see cref="GameConfig.rocketDurationMeasures"/> compases (aprox segundos en jam).
    /// Papa Fantasma: dura <see cref="GameConfig.ghostEffectDuration"/> segundos.
    /// Papa Eléctrica: sin duración en tiempo, se aplica por 4 notas (aquí usamos 4s como proxy).
    /// Papa Helada: efecto instantáneo en calor; aquí duración 0 (se resuelve en PotatoHeatSystem).
    /// </summary>
    private float GetDuration(PowerType powerType)
    {
        float cfg_ghost   = (config != null) ? config.ghostEffectDuration   : 3.0f;
        int   cfg_rocket  = (config != null) ? config.rocketDurationMeasures : 4;

        switch (powerType)
        {
            case PowerType.RocketPotato:   return cfg_rocket * 2f; // 4 compases × ~2s/compás como proxy
            case PowerType.GhostPotato:    return cfg_ghost;
            case PowerType.ElectricPotato: return 4f;              // 4 notas proxy
            case PowerType.FreezePotato:   return 0.1f;            // Efecto instantáneo
            default:                       return 3f;
        }
    }
}
