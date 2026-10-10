using UnityEngine;

/// <summary>
/// Gestiona el medidor de calor por jugador. 
/// Acumula calor en los fallos (Miss), lo reduce con los aciertos (Perfect) y poderes (Helada).
/// Cuando un jugador llega al 100%, emite el evento de explosin que finaliza la ronda.
/// </summary>
public class PotatoHeatSystem : MonoBehaviour
{
    [Header("Configuracin")]
    [Tooltip("Referencia a la configuracin balanceada del juego")]
    public GameConfig config;

    // Estado del calor por jugador (0.0f a 1.0f)
    private float player1Heat = 0f;
    private float player2Heat = 0f;

    // Prevenir dobles explosiones en la misma ronda
    private bool hasExploded = false;

    private void Awake()
    {
        if (config == null)
        {
            Debug.LogWarning("[PotatoHeatSystem] GameConfig no asignado. Asegrese de asignarlo en el Inspector.");
        }
    }

    private void OnEnable()
    {
        // Escuchar eventos de partida para resetear el calor
        GameEvents.OnRoundStarted += HandleRoundStarted;
        
        // Escuchar aciertos y fallos para sumar o restar calor
        GameEvents.OnHitResolved += HandleHitResolved;
        GameEvents.OnPlayerMiss += HandlePlayerMiss;
        GameEvents.OnNoteMiss += HandleNoteMiss;
        
        // Escuchar poderes que alteran el calor (ej. Papa Helada)
        GameEvents.OnPowerActivated += HandlePowerActivated;
    }

    private void OnDisable()
    {
        GameEvents.OnRoundStarted -= HandleRoundStarted;
        GameEvents.OnHitResolved -= HandleHitResolved;
        GameEvents.OnPlayerMiss -= HandlePlayerMiss;
        GameEvents.OnNoteMiss -= HandleNoteMiss;
        GameEvents.OnPowerActivated -= HandlePowerActivated;
    }

    private void HandleRoundStarted(int round)
    {
        ResetHeat();
    }

    private void ResetHeat()
    {
        player1Heat = 0f;
        player2Heat = 0f;
        hasExploded = false;
        
        // Notificar que se resete a 0
        GameEvents.TriggerPlayerHeatUpdated(PlayerID.Player1, 0f);
        GameEvents.TriggerPlayerHeatUpdated(PlayerID.Player2, 0f);
        GameEvents.TriggerHeatUpdated(0f);
    }

    private void HandleHitResolved(PlayerID player, HitResult result, float accuracy)
    {
        if (hasExploded) return;

        // Segn GDD: Si acert Perfect, reduce el calor propio (-5%)
        if (result == HitResult.Perfect)
        {
            float reduction = (config != null) ? config.heatCooldownOnPerfect : 0.05f;
            ModifyHeat(player, -reduction);
        }
        else if (result == HitResult.Miss)
        {
            // Fallback en caso de que un sistema externo enve Miss va OnHitResolved
            AddMissHeat(player);
        }
    }

    private void HandlePlayerMiss(PlayerID player)
    {
        AddMissHeat(player);
    }

    private void HandleNoteMiss(PlayerID player, int laneID)
    {
        AddMissHeat(player);
    }

    private void AddMissHeat(PlayerID player)
    {
        if (hasExploded) return;

        // Segn GDD: Cada fallo incrementa el calor en un 20%
        float increase = (config != null) ? config.heatGainOnMiss : 0.20f;
        ModifyHeat(player, increase);
    }

    private void HandlePowerActivated(PlayerID player, PowerType power)
    {
        if (hasExploded) return;

        // Segn GDD: Papa Helada reduce el calor del jugador que la activa en 40%
        if (power == PowerType.Freeze)
        {
            float reduction = (config != null) ? config.freezePotatoHeatReduction : 0.40f;
            ModifyHeat(player, -reduction);
        }
    }

    private void ModifyHeat(PlayerID player, float amount)
    {
        float explosionThreshold = (config != null) ? config.heatThresholdExplosion : 1.0f;

        if (player == PlayerID.Player1)
        {
            player1Heat = Mathf.Clamp(player1Heat + amount, 0f, explosionThreshold);
            GameEvents.TriggerPlayerHeatUpdated(PlayerID.Player1, player1Heat / explosionThreshold);
            CheckExplosion(PlayerID.Player1, player1Heat, explosionThreshold);
        }
        else if (player == PlayerID.Player2)
        {
            player2Heat = Mathf.Clamp(player2Heat + amount, 0f, explosionThreshold);
            GameEvents.TriggerPlayerHeatUpdated(PlayerID.Player2, player2Heat / explosionThreshold);
            CheckExplosion(PlayerID.Player2, player2Heat, explosionThreshold);
        }
        
        // El calor general puede usarse para efectos de escenario / papa central
        float maxHeat = Mathf.Max(player1Heat, player2Heat);
        GameEvents.TriggerHeatUpdated(maxHeat / explosionThreshold);
    }

    private void CheckExplosion(PlayerID player, float currentHeat, float threshold)
    {
        // Si super o igual el 100%, detona la papa en el campo de ese jugador
        if (currentHeat >= threshold && !hasExploded)
        {
            hasExploded = true;
            Debug.Log($"[PotatoHeatSystem] Calor al mximo (100%) para {player}! La papa ha explotado.");
            GameEvents.TriggerPotatoExploded(player);
        }
    }
}
