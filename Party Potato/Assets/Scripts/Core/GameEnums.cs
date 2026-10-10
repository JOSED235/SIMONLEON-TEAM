using System;

/// <summary>
/// Identificador de los jugadores en Beat Potato (Papa Beat).
/// </summary>
public enum PlayerID
{
    None = 0,
    Player1 = 1,
    Player2 = 2
}

/// <summary>
/// Resultado y precisión temporal del golpe según la ventana de acierto en Beat Potato.
/// Margen según GDD: Perfect (±50 ms), Great (±100 ms), Good (±150 ms), Miss (>±150 ms).
/// </summary>
public enum HitResult
{
    Miss = 0,      // Fuera de ventana (>±150 ms) o nota no pulsada
    Good = 1,      // Golpe acertado dentro de margen amplio (±150 ms)
    Great = 2,     // Buen golpe dentro de margen intermedio (±100 ms)
    Perfect = 3    // Golpe exacto dentro de máxima precisión (±50 ms)
}

/// <summary>
/// Dirección de los 4 carriles (Lanes) al estilo FNF para Beat Potato.
/// </summary>
public enum LaneDirection
{
    Left = 0,      // Carril Izquierdo (P1: Flecha Izq / P2: A)
    Down = 1,      // Carril Abajo (P1: Flecha Abajo / P2: S)
    Up = 2,        // Carril Arriba (P1: Flecha Arriba / P2: W)
    Right = 3      // Carril Derecha (P1: Flecha Der / P2: D)
}

/// <summary>
/// Modos de juego soportados en Beat Potato.
/// </summary>
public enum GameMode
{
    LocalPVP = 0,  // Duelo local 1v1 en el mismo teclado
    SoloVsCPU = 1  // Modo solitario vs Inteligencia Artificial
}

/// <summary>
/// Niveles de dificultad para la inteligencia artificial en modo Solo.
/// </summary>
public enum AIDifficulty
{
    Easy = 0,      // Papa Tibia: ~70% precisión, predomina Good y fallos frecuentes
    Normal = 1,    // Papa Caliente: ~85% precisión, combina Great y Perfect
    Hard = 2       // Papa Volcánica: ~96% precisión, altas rachas y uso activo de poderes
}

/// <summary>
/// Catálogo de poderes especiales de sabotaje y soporte desbloqueables tras racha de 5 aciertos.
/// </summary>
public enum PowerType
{
    None = 0,
    RocketPotato = 1,    // Papa Cohete: acelera notas del rival durante 4 compases
    GhostPotato = 2,     // Papa Fantasma: oculta notas rivales en el tramo medio por 3s
    ElectricPotato = 3,  // Papa Eléctrica: estrecha la ventana de acierto del rival durante 4 notas
    FreezePotato = 4,    // Papa Helada: enfría calor en 40% y anula penalización del próximo fallo
    GoldenPotato = 5     // Papa Dorada: bonificación especial / compatibilidad
}

/// <summary>
/// Estado general del flujo de la partida.
/// </summary>
public enum MatchState
{
    Idle = 0,            // En espera en menú o preparación
    Countdown = 1,       // Conteo inicial antes de habilitar el flujo rítmico
    Playing = 2,         // Partida en curso y notas activas
    RoundOver = 3,       // Final de una ronda tras detonación por calor crítico (100%)
    GameOver = 4         // Final de la partida (ej. al mejor de 3 rondas)
}

/// <summary>
/// Estado físico y visual de la papa en el duelo.
/// </summary>
public enum PotatoState
{
    Idle = 0,            // Estática en centro o preparación
    Traveling = 1,       // Desplazamiento oscilatorio rítmico entre ambos campos
    InHitZone = 2,       // Cercana a zona de peligro
    HitResolved = 3,     // Empujada con éxito hacia el rival
    Exploded = 4         // Detonó por alcanzar 100% de calor en manos de un jugador
}
