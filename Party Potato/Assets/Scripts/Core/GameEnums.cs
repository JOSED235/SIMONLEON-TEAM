using System;

/// <summary>
/// Identificador de los jugadores en Papa Vóley.
/// </summary>
public enum PlayerID
{
    None = 0,
    Player1 = 1,
    Player2 = 2
}

/// <summary>
/// Resultado y precisión temporal del golpe según la ventana de acierto.
/// </summary>
public enum HitResult
{
    Miss = 0,      // Fuera de ventana / No golpeado
    Good = 1,      // Golpe acertado dentro de tolerancia estándar (±150 ms)
    Perfect = 2    // Golpe perfecto con máxima precisión (±70 ms)
}

/// <summary>
/// Poderes disponibles que alteran el próximo intercambio o potencian al jugador.
/// </summary>
public enum PowerType
{
    None = 0,
    RocketPotato = 1,    // ¡Papa Cohete!: el próximo pase viaja más rápido (20-25%)
    ElectricPotato = 2,  // Papa Eléctrica: reduce la ventana de acierto del rival
    GhostPotato = 3,     // Papa Fantasma: desaparece y reaparece cerca del impacto
    FreezePotato = 4,    // Papa Congelada: desfasa levemente la anticipación rítmica
    GoldenPotato = 5     // Papa Dorada: bonificación de puntos y recarga acelerada
}

/// <summary>
/// Estado general del flujo de la partida.
/// </summary>
public enum MatchState
{
    Idle = 0,            // En espera en menú o preparación
    Countdown = 1,       // Conteo inicial antes de lanzar la papa
    Playing = 2,         // Papa en juego e intercambios activos
    RoundOver = 3,       // Final de una ronda tras una explosión
    GameOver = 4         // Final de la partida (ej. al mejor de 3 rondas)
}

/// <summary>
/// Estado físico y de interacción de la papa.
/// </summary>
public enum PotatoState
{
    Idle = 0,            // Estática esperando saque
    Traveling = 1,       // En trayectoria visual hacia el jugador objetivo
    InHitZone = 2,       // Dentro de la zona de golpeo activa
    HitResolved = 3,     // Golpe resuelto con éxito
    Exploded = 4         // Explotó por fallo del jugador
}
