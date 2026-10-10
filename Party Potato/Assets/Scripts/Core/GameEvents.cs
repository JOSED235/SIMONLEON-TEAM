using System;

/// <summary>
/// Bus de eventos global y desacoplado para Papa Vóley.
/// Todos los sistemas de backend, frontend, audio y VFX se comunican a través de estos eventos.
/// </summary>
public static class GameEvents
{
    #region Ritmo y Tiempo
    /// <summary>
    /// Disparado en cada pulso musical (Beat). Parámetro: índice del beat.
    /// </summary>
    public static event Action<int> OnBeat;

    /// <summary>
    /// Disparado en cada compás musical (Measure). Parámetro: índice del compás.
    /// </summary>
    public static event Action<int> OnMeasure;
    #endregion

    #region Dinámica de la Papa y Lanzamiento
    /// <summary>
    /// Disparado al enviar la papa hacia un jugador.
    /// Parámetros: (origen, destino, targetDspTimeDeImpacto, duracionEnBeats).
    /// </summary>
    public static event Action<PlayerID, PlayerID, double, float> OnPotatoLaunched;

    /// <summary>
    /// Disparado cuando la papa ingresa a la zona de golpeo del jugador objetivo.
    /// Parámetro: jugador que debe responder al golpe.
    /// </summary>
    public static event Action<PlayerID> OnPotatoReachedHitZone;

    /// <summary>
    /// Disparado cuando la papa explota en manos de un jugador por fallo.
    /// Parámetro: jugador que falló y recibió la explosión.
    /// </summary>
    public static event Action<PlayerID> OnPotatoExploded;
    #endregion

    #region Detección de Golpes y Precisión
    /// <summary>
    /// Disparado cuando un jugador presiona a tiempo y se evalúa el golpe.
    /// Parámetros: (jugador, resultado [Perfect/Good/Miss], precisión normalizada 0..1).
    /// </summary>
    public static event Action<PlayerID, HitResult, float> OnHitResolved;

    /// <summary>
    /// Disparado cuando el jugador activo no golpea a tiempo (ventana expirada) o falla completamente.
    /// Parámetro: jugador que cometió el fallo.
    /// </summary>
    public static event Action<PlayerID> OnPlayerMiss;
    #endregion

    #region Rachas y Medidores
    /// <summary>
    /// Disparado cuando cambia la racha de aciertos de un jugador.
    /// Parámetros: (jugador, rachaActual, tienePoderDisponible).
    /// </summary>
    public static event Action<PlayerID, int, bool> OnStreakUpdated;

    /// <summary>
    /// Disparado cuando un jugador reinicia su racha por cometer un fallo.
    /// Parámetro: jugador cuya racha se reinició a cero.
    /// </summary>
    public static event Action<PlayerID> OnStreakReset;
    #endregion

    #region Poderes y Habilidades
    /// <summary>
    /// Disparado cuando un jugador acumula la racha requerida y obtiene una carga de poder.
    /// Parámetros: (jugador, tipoDePoderDesbloqueado).
    /// </summary>
    public static event Action<PlayerID, PowerType> OnPowerUnlocked;

    /// <summary>
    /// Disparado cuando un jugador presiona el botón de habilidad y consume su poder.
    /// Parámetros: (jugadorQueLoUsa, tipoDePoderActivado).
    /// </summary>
    public static event Action<PlayerID, PowerType> OnPowerActivated;

    /// <summary>
    /// Disparado cuando el efecto temporal de un poder finaliza.
    /// Parámetro: tipo de poder que expiró.
    /// </summary>
    public static event Action<PowerType> OnPowerExpired;
    #endregion

    #region Sistema de Calor
    /// <summary>
    /// Disparado cuando el nivel de calor y tensión de la papa cambia.
    /// Parámetro: nivel de calor normalizado (0.0f = frío/normal, 1.0f = crítico/a punto de explotar).
    /// </summary>
    public static event Action<float> OnHeatUpdated;
    #endregion

    #region Flujo de Partida y Rondas
    /// <summary>
    /// Disparado al iniciar una nueva ronda.
    /// Parámetro: número de ronda actual (1, 2, 3...).
    /// </summary>
    public static event Action<int> OnRoundStarted;

    /// <summary>
    /// Disparado cuando una ronda concluye tras una explosión.
    /// Parámetros: (ganadorDeRonda, rondasGanadasP1, rondasGanadasP2).
    /// </summary>
    public static event Action<PlayerID, int, int> OnRoundEnded;

    /// <summary>
    /// Disparado cuando un jugador alcanza la victoria definitiva de la partida.
    /// Parámetro: jugador ganador del duelo.
    /// </summary>
    public static event Action<PlayerID> OnMatchEnded;

    /// <summary>
    /// Disparado para reiniciar la partida completa (revancha / play again).
    /// </summary>
    public static event Action OnMatchReset;
    #endregion

    #region Invocadores Seguros (Trigger Methods)
    public static void TriggerBeat(int beatIndex) => OnBeat?.Invoke(beatIndex);
    public static void TriggerMeasure(int measureIndex) => OnMeasure?.Invoke(measureIndex);

    public static void TriggerPotatoLaunched(PlayerID from, PlayerID to, double targetDspTime, float travelBeats)
        => OnPotatoLaunched?.Invoke(from, to, targetDspTime, travelBeats);

    public static void TriggerPotatoReachedHitZone(PlayerID targetPlayer)
        => OnPotatoReachedHitZone?.Invoke(targetPlayer);

    public static void TriggerPotatoExploded(PlayerID loser)
        => OnPotatoExploded?.Invoke(loser);

    public static void TriggerHitResolved(PlayerID player, HitResult result, float accuracy)
        => OnHitResolved?.Invoke(player, result, accuracy);

    public static void TriggerPlayerMiss(PlayerID player)
        => OnPlayerMiss?.Invoke(player);

    public static void TriggerStreakUpdated(PlayerID player, int streak, bool powerReady)
        => OnStreakUpdated?.Invoke(player, streak, powerReady);

    public static void TriggerStreakReset(PlayerID player)
        => OnStreakReset?.Invoke(player);

    public static void TriggerPowerUnlocked(PlayerID player, PowerType power)
        => OnPowerUnlocked?.Invoke(player, power);

    public static void TriggerPowerActivated(PlayerID player, PowerType power)
        => OnPowerActivated?.Invoke(player, power);

    public static void TriggerPowerExpired(PowerType power)
        => OnPowerExpired?.Invoke(power);

    public static void TriggerHeatUpdated(float heatNormalized)
        => OnHeatUpdated?.Invoke(heatNormalized);

    public static void TriggerRoundStarted(int roundNumber)
        => OnRoundStarted?.Invoke(roundNumber);

    public static void TriggerRoundEnded(PlayerID roundWinner, int p1Score, int p2Score)
        => OnRoundEnded?.Invoke(roundWinner, p1Score, p2Score);

    public static void TriggerMatchEnded(PlayerID matchWinner)
        => OnMatchEnded?.Invoke(matchWinner);

    public static void TriggerMatchReset()
        => OnMatchReset?.Invoke();
    #endregion

    /// <summary>
    /// Limpia todos los delegados suscritos para evitar fugas de memoria y callbacks residuales al cambiar de escena.
    /// </summary>
    public static void ResetAllEvents()
    {
        OnBeat = null;
        OnMeasure = null;
        OnPotatoLaunched = null;
        OnPotatoReachedHitZone = null;
        OnPotatoExploded = null;
        OnHitResolved = null;
        OnPlayerMiss = null;
        OnStreakUpdated = null;
        OnStreakReset = null;
        OnPowerUnlocked = null;
        OnPowerActivated = null;
        OnPowerExpired = null;
        OnHeatUpdated = null;
        OnRoundStarted = null;
        OnRoundEnded = null;
        OnMatchEnded = null;
        OnMatchReset = null;
    }
}
