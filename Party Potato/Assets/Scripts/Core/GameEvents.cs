using System;

/// <summary>
/// Bus de eventos global y desacoplado para Beat Potato (Papa Beat).
/// Todos los sistemas de ritmo, jugabilidad, calor, poderes, UI y audio se comunican mediante este bus.
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

    #region Dinámica de la Papa y Posicionamiento
    /// <summary>
    /// Disparado al desplazar la papa entre jugadores.
    /// Parámetros: (origen, destino, targetDspTimeDeImpacto, duracionEnBeats).
    /// </summary>
    public static event Action<PlayerID, PlayerID, double, float> OnPotatoLaunched;

    /// <summary>
    /// Disparado cuando la papa ingresa a la zona de peligro de un jugador.
    /// Parámetro: jugador en cuyo lado se encuentra la papa.
    /// </summary>
    public static event Action<PlayerID> OnPotatoReachedHitZone;

    /// <summary>
    /// Disparado cuando la posición horizontal normalizada de la papa cambia.
    /// Parámetro: posición normalizada (-1.0f = Jugador 1, 0.0f = Centro, 1.0f = Jugador 2).
    /// </summary>
    public static event Action<float> OnPotatoPositionUpdated;

    /// <summary>
    /// Disparado cuando la papa alcanza el 100% de calor y estalla en el campo de un jugador.
    /// Parámetro: jugador que recibió la explosión (perdedor de la ronda).
    /// </summary>
    public static event Action<PlayerID> OnPotatoExploded;
    #endregion

    #region Detección de Golpes y Notas (FNF)
    /// <summary>
    /// Disparado cuando un jugador presiona a tiempo y se evalúa el golpe.
    /// Parámetros: (jugador, resultado [Perfect/Great/Good/Miss], precisión normalizada 0..1).
    /// </summary>
    public static event Action<PlayerID, HitResult, float> OnHitResolved;

    /// <summary>
    /// Disparado cuando se resuelve una nota en un carril específico.
    /// Parámetros: (jugador, laneID [0=Left, 1=Down, 2=Up, 3=Right], resultado, precisión 0..1).
    /// </summary>
    public static event Action<PlayerID, int, HitResult, float> OnNoteHit;

    /// <summary>
    /// Disparado cuando el jugador no presiona a tiempo o comete un fallo.
    /// Parámetro: jugador que cometió el fallo.
    /// </summary>
    public static event Action<PlayerID> OnPlayerMiss;

    /// <summary>
    /// Disparado cuando se pierde una nota en un carril específico (auto-miss).
    /// Parámetros: (jugador, laneID).
    /// </summary>
    public static event Action<PlayerID, int> OnNoteMiss;
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

    #region Poderes y Sabotaje
    /// <summary>
    /// Disparado cuando un jugador acumula 5 aciertos consecutivos y obtiene un poder.
    /// Parámetros: (jugador, tipoDePoderDesbloqueado).
    /// </summary>
    public static event Action<PlayerID, PowerType> OnPowerUnlocked;

    /// <summary>
    /// Disparado cuando un jugador activa y consume su poder especial.
    /// Parámetros: (jugadorQueLoUsa, tipoDePoderActivado).
    /// </summary>
    public static event Action<PlayerID, PowerType> OnPowerActivated;

    /// <summary>
    /// Disparado cuando el efecto de un poder finaliza.
    /// Parámetro: tipo de poder que expiró.
    /// </summary>
    public static event Action<PowerType> OnPowerExpired;
    #endregion

    #region Sistema de Calor
    /// <summary>
    /// Disparado cuando el nivel global de calor de la papa cambia (0.0f a 1.0f).
    /// </summary>
    public static event Action<float> OnHeatUpdated;

    /// <summary>
    /// Disparado cuando el nivel de calor del campo de un jugador específico cambia.
    /// Parámetros: (jugador, calorNormalizado 0..1).
    /// </summary>
    public static event Action<PlayerID, float> OnPlayerHeatUpdated;
    #endregion

    #region Flujo de Partida y Rondas
    /// <summary>
    /// Disparado al iniciar una nueva ronda. Parámetro: número de ronda actual.
    /// </summary>
    public static event Action<int> OnRoundStarted;

    /// <summary>
    /// Disparado cuando una ronda concluye tras la explosión de la papa.
    /// Parámetros: (ganadorDeRonda, rondasGanadasP1, rondasGanadasP2).
    /// </summary>
    public static event Action<PlayerID, int, int> OnRoundEnded;

    /// <summary>
    /// Disparado cuando un jugador alcanza la victoria definitiva de la partida.
    /// Parámetro: jugador ganador del duelo.
    /// </summary>
    public static event Action<PlayerID> OnMatchEnded;

    /// <summary>
    /// Disparado para reiniciar la partida completa (revancha).
    /// </summary>
    public static event Action OnMatchReset;
    #endregion

    #region Invocadores Seguros (Trigger Methods)
    public static void TriggerBeat(int beatIndex)
    {
        Action<int> handler = OnBeat;
        if (handler != null) handler(beatIndex);
    }

    public static void TriggerMeasure(int measureIndex)
    {
        Action<int> handler = OnMeasure;
        if (handler != null) handler(measureIndex);
    }

    public static void TriggerPotatoLaunched(PlayerID from, PlayerID to, double targetDspTime, float travelBeats)
    {
        Action<PlayerID, PlayerID, double, float> handler = OnPotatoLaunched;
        if (handler != null) handler(from, to, targetDspTime, travelBeats);
    }

    public static void TriggerPotatoReachedHitZone(PlayerID targetPlayer)
    {
        Action<PlayerID> handler = OnPotatoReachedHitZone;
        if (handler != null) handler(targetPlayer);
    }

    public static void TriggerPotatoPositionUpdated(float normalizedPosition)
    {
        Action<float> handler = OnPotatoPositionUpdated;
        if (handler != null) handler(normalizedPosition);
    }

    public static void TriggerPotatoExploded(PlayerID loser)
    {
        Action<PlayerID> handler = OnPotatoExploded;
        if (handler != null) handler(loser);
    }

    public static void TriggerHitResolved(PlayerID player, HitResult result, float accuracy)
    {
        Action<PlayerID, HitResult, float> handler = OnHitResolved;
        if (handler != null) handler(player, result, accuracy);
    }

    public static void TriggerNoteHit(PlayerID player, int laneID, HitResult result, float accuracy)
    {
        Action<PlayerID, int, HitResult, float> handler = OnNoteHit;
        if (handler != null) handler(player, laneID, result, accuracy);
    }

    public static void TriggerPlayerMiss(PlayerID player)
    {
        Action<PlayerID> handler = OnPlayerMiss;
        if (handler != null) handler(player);
    }

    public static void TriggerNoteMiss(PlayerID player, int laneID)
    {
        Action<PlayerID, int> handler = OnNoteMiss;
        if (handler != null) handler(player, laneID);
    }

    public static void TriggerStreakUpdated(PlayerID player, int streak, bool powerReady)
    {
        Action<PlayerID, int, bool> handler = OnStreakUpdated;
        if (handler != null) handler(player, streak, powerReady);
    }

    public static void TriggerStreakReset(PlayerID player)
    {
        Action<PlayerID> handler = OnStreakReset;
        if (handler != null) handler(player);
    }

    public static void TriggerPowerUnlocked(PlayerID player, PowerType power)
    {
        Action<PlayerID, PowerType> handler = OnPowerUnlocked;
        if (handler != null) handler(player, power);
    }

    public static void TriggerPowerActivated(PlayerID player, PowerType power)
    {
        Action<PlayerID, PowerType> handler = OnPowerActivated;
        if (handler != null) handler(player, power);
    }

    public static void TriggerPowerExpired(PowerType power)
    {
        Action<PowerType> handler = OnPowerExpired;
        if (handler != null) handler(power);
    }

    public static void TriggerHeatUpdated(float heatNormalized)
    {
        Action<float> handler = OnHeatUpdated;
        if (handler != null) handler(heatNormalized);
    }

    public static void TriggerPlayerHeatUpdated(PlayerID player, float heatNormalized)
    {
        Action<PlayerID, float> handler = OnPlayerHeatUpdated;
        if (handler != null) handler(player, heatNormalized);
    }

    public static void TriggerRoundStarted(int roundNumber)
    {
        Action<int> handler = OnRoundStarted;
        if (handler != null) handler(roundNumber);
    }

    public static void TriggerRoundEnded(PlayerID roundWinner, int p1Score, int p2Score)
    {
        Action<PlayerID, int, int> handler = OnRoundEnded;
        if (handler != null) handler(roundWinner, p1Score, p2Score);
    }

    public static void TriggerMatchEnded(PlayerID matchWinner)
    {
        Action<PlayerID> handler = OnMatchEnded;
        if (handler != null) handler(matchWinner);
    }

    public static void TriggerMatchReset()
    {
        Action handler = OnMatchReset;
        if (handler != null) handler();
    }
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
        OnPotatoPositionUpdated = null;
        OnPotatoExploded = null;
        OnHitResolved = null;
        OnNoteHit = null;
        OnPlayerMiss = null;
        OnNoteMiss = null;
        OnStreakUpdated = null;
        OnStreakReset = null;
        OnPowerUnlocked = null;
        OnPowerActivated = null;
        OnPowerExpired = null;
        OnHeatUpdated = null;
        OnPlayerHeatUpdated = null;
        OnRoundStarted = null;
        OnRoundEnded = null;
        OnMatchEnded = null;
        OnMatchReset = null;
    }
}
