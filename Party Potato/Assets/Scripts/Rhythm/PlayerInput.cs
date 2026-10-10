using System;
using UnityEngine;

/// <summary>
/// Mapeo de teclas de entrada para un jugador en Beat Potato (Papa Beat).
/// Serializable para poder configurarlo desde el Inspector de Unity.
/// </summary>
[System.Serializable]
public class PlayerKeyBindings
{
    public KeyCode leftKey    = KeyCode.LeftArrow;
    public KeyCode downKey    = KeyCode.DownArrow;
    public KeyCode upKey      = KeyCode.UpArrow;
    public KeyCode rightKey   = KeyCode.RightArrow;
    public KeyCode powerKey   = KeyCode.Return;
    public KeyCode altPowerKey = KeyCode.RightShift;
}

/// <summary>
/// Procesa las entradas de teclado de ambos jugadores en Beat Potato (Papa Beat).
///
/// Responsabilidades segun la division de responsabilidades del backend:
/// 1. Leer Flechas para P1 y WASD para P2 con el mapeo exacto del GDD.
/// 2. Detectar pulsaciones de poder con controles separados por jugador.
/// 3. Localizar la nota activa mas cercana al beat actual en el carril pulsado.
/// 4. Solicitar la evaluacion temporal a RhythmManager.EvaluateHit.
/// 5. Prevenir doble consumo: una nota ya resuelta no puede volver a procesarse.
/// 6. No otorgar aciertos cuando se pulsa fuera de la ventana valida (ghost tap).
/// 7. Ignorar pulsaciones extremadamente tempranas (nota aun fuera de la ventana de busqueda).
///
/// Invariante: toda la evaluacion de precision (Perfect/Great/Good/Miss) ocurre en
/// RhythmManager.EvaluateHit — PlayerInput solo orquesta la busqueda, la llamada y el despacho.
/// </summary>
public class PlayerInput : MonoBehaviour
{
    [Header("Modo de Juego")]
    [Tooltip("Modo actual: LocalPVP (ambos jugadores en teclado) o SoloVsCPU (solo lee P1).")]
    public GameMode gameMode = GameMode.LocalPVP;

    [Header("Mapeo de Teclas")]
    [Tooltip("Configuracion de teclas para el Jugador 1 (por defecto: Flechas + Enter/RShift).")]
    public PlayerKeyBindings player1Keys = new PlayerKeyBindings();

    [Tooltip("Configuracion de teclas para el Jugador 2 (por defecto: WASD + Space/LShift).")]
    public PlayerKeyBindings player2Keys = new PlayerKeyBindings
    {
        leftKey    = KeyCode.A,
        downKey    = KeyCode.S,
        upKey      = KeyCode.W,
        rightKey   = KeyCode.D,
        powerKey   = KeyCode.Space,
        altPowerKey = KeyCode.LeftShift
    };

    [Header("Reglas de Evaluacion")]
    [Tooltip("Si esta activo, pulsar sin nota valida en ventana dispara OnPlayerMiss (anti-spam).")]
    public bool penalizeGhostTaps = true;

    [Tooltip("Margen adicional en segundos sobre goodTolerance para ampliar la busqueda de nota.")]
    public float extraWindowTolerance = 0.02f;

    // -----------------------------------------------------------------------
    // Estado de poderes activos que alteran la ventana de precision
    // -----------------------------------------------------------------------

    // Rastreo por victima: ElectricPotato estrecha la ventana del jugador afectado.
    // Se almacena quien fue la victima para revertirla correctamente al expirar
    // (igual que rocketVictim/ghostVictim en NoteSpawner).
    private bool p1ElectricActive = false;
    private bool p2ElectricActive = false;
    private PlayerID electricVictim = PlayerID.None;

    // Poder disponible para activar (maximo 1 carga por jugador segun GDD).
    private PowerType p1AvailablePower = PowerType.None;
    private PowerType p2AvailablePower = PowerType.None;

    // -----------------------------------------------------------------------
    // Ciclo de vida de Unity
    // -----------------------------------------------------------------------

    private void OnEnable()
    {
        GameEvents.OnPowerUnlocked  += HandlePowerUnlocked;
        GameEvents.OnPowerActivated += HandlePowerActivated;
        GameEvents.OnPowerExpired   += HandlePowerExpired;
        GameEvents.OnRoundStarted   += HandleRoundStarted;
    }

    private void OnDisable()
    {
        GameEvents.OnPowerUnlocked  -= HandlePowerUnlocked;
        GameEvents.OnPowerActivated -= HandlePowerActivated;
        GameEvents.OnPowerExpired   -= HandlePowerExpired;
        GameEvents.OnRoundStarted   -= HandleRoundStarted;
    }

    private void Update()
    {
        if (RhythmManager.Instance == null
            || !RhythmManager.Instance.isPlaying
            || RhythmManager.Instance.isPaused)
        {
            return;
        }

        // Jugador 1: siempre activo (humano en ambos modos).
        ProcessPlayerInputs(PlayerID.Player1, player1Keys);

        // Jugador 2: solo en modo Duelo Local (en SoloVsCPU lo gestiona AIController).
        if (gameMode == GameMode.LocalPVP)
        {
            ProcessPlayerInputs(PlayerID.Player2, player2Keys);
        }
    }

    // -----------------------------------------------------------------------
    // Lectura de teclas por jugador
    // -----------------------------------------------------------------------

    private void ProcessPlayerInputs(PlayerID player, PlayerKeyBindings bindings)
    {
        // Cuatro carriles segun el GDD: Left=0, Down=1, Up=2, Right=3
        if (Input.GetKeyDown(bindings.leftKey))  HandleLanePress(player, (int)LaneDirection.Left);
        if (Input.GetKeyDown(bindings.downKey))  HandleLanePress(player, (int)LaneDirection.Down);
        if (Input.GetKeyDown(bindings.upKey))    HandleLanePress(player, (int)LaneDirection.Up);
        if (Input.GetKeyDown(bindings.rightKey)) HandleLanePress(player, (int)LaneDirection.Right);

        // Activacion del poder especial (tecla principal o alternativa)
        if (Input.GetKeyDown(bindings.powerKey) || Input.GetKeyDown(bindings.altPowerKey))
        {
            HandlePowerPress(player);
        }
    }

    // -----------------------------------------------------------------------
    // Evaluacion de un carril pulsado
    // -----------------------------------------------------------------------

    /// <summary>
    /// Procesa la pulsacion de un carril para un jugador dado.
    /// Flujo:
    ///   1. Buscar nota activa dentro de la ventana maxima (goodTolerance + extraWindowTolerance).
    ///   2a. Si existe nota: pedir evaluacion a RhythmManager y resolver como hit o miss segun resultado.
    ///   2b. Si no existe nota: ghost tap — penalizar si penalizeGhostTaps esta activo.
    /// Expuesto como publico para que los tests unitarios e IA invoquen la logica directamente.
    /// Retorna el HitResult obtenido (Perfect, Great, Good o Miss).
    /// </summary>
    public HitResult HandleLanePress(PlayerID player, int laneID)
    {
        if (RhythmManager.Instance == null) return HitResult.Miss;

        GameConfig config = RhythmManager.Instance.gameConfig;
        float goodTol    = (config != null) ? config.goodTolerance : 0.15f;

        // Ventana de busqueda: Good tolerance + margen extra, expresado en segundos.
        // Usar GetClosestActiveNoteByTime para mantener coherencia con las unidades de GameConfig.
        float searchWindowSec = goodTol + extraWindowTolerance;
        NoteObject targetNote = NoteObject.GetClosestActiveNoteByTime(player, laneID, searchWindowSec);

        bool isElectric = (player == PlayerID.Player1) ? p1ElectricActive : p2ElectricActive;

        if (targetNote != null && !targetNote.isResolved)
        {
            // Evaluar la precision exacta contra el tiempo programado de la nota.
            // La posicion visual de la nota no influye en esta decision.
            float timeDiff;
            float accuracy;
            HitResult result = RhythmManager.Instance.EvaluateHit(
                targetNote.beatOfThisNote, out timeDiff, out accuracy, isElectric);

            if (result != HitResult.Miss)
            {
                // Acierto valido: resolver la nota y publicar el resultado.
                targetNote.ResolveHit(result, accuracy);
                Debug.Log(string.Format(
                    "[PlayerInput] {0} acerto carril {1} ({2}). Desfase: {3:F3}s, Precision: {4:F0}%",
                    player, laneID, result, timeDiff, accuracy * 100f));
                return result;
            }
            else
            {
                // La nota esta en la ventana de busqueda pero el timing exacto es Miss
                // (diferencia > goodTolerance). La nota se consume como fallo intencional
                // para evitar que el jugador reintente la misma nota indefinidamente.
                targetNote.ResolveMiss();
                Debug.Log(string.Format(
                    "[PlayerInput] {0} pulso carril {1} fuera de tiempo (Miss forzado). Desfase: {2:F3}s",
                    player, laneID, timeDiff));
                return HitResult.Miss;
            }
        }
        else
        {
            // Ghost Tap: pulsacion sin nota valida en la ventana de busqueda.
            // Segun el GDD: "Presionar sin notas validas puede ser penalizado".
            if (penalizeGhostTaps)
            {
                GameEvents.TriggerPlayerMiss(player);
                GameEvents.TriggerHitResolved(player, HitResult.Miss, 0f);
                Debug.Log(string.Format(
                    "[PlayerInput] Ghost Tap para {0} en carril {1} sin nota activa en ventana.",
                    player, laneID));
            }
            return HitResult.Miss;
        }
    }

    // -----------------------------------------------------------------------
    // Activacion de Poder Especial
    // -----------------------------------------------------------------------

    /// <summary>
    /// Consume la carga de poder disponible del jugador y publica el evento de activacion.
    /// PowerUpManager (Jose David) recibe el evento y aplica el efecto sobre el oponente.
    /// </summary>
    private void HandlePowerPress(PlayerID player)
    {
        PowerType powerToActivate = (player == PlayerID.Player1) ? p1AvailablePower : p2AvailablePower;

        if (powerToActivate != PowerType.None)
        {
            // Consumir la carga antes de publicar para evitar doble activacion
            if (player == PlayerID.Player1) p1AvailablePower = PowerType.None;
            else                            p2AvailablePower = PowerType.None;

            GameEvents.TriggerPowerActivated(player, powerToActivate);
            Debug.Log(string.Format("[PlayerInput] {0} activo su poder: {1}.", player, powerToActivate));
        }
        else
        {
            Debug.Log(string.Format(
                "[PlayerInput] {0} intento usar poder sin carga disponible.", player));
        }
    }

    // -----------------------------------------------------------------------
    // Manejo de eventos del bus GameEvents
    // -----------------------------------------------------------------------

    #region Eventos

    /// <summary>
    /// Recibe el poder desbloqueado por ComboSystem cuando el jugador alcanza 5 aciertos consecutivos.
    /// Almacena el tipo para activarlo cuando el jugador presione su tecla de poder.
    /// Solo se guarda si el jugador aun no tiene una carga (maximo 1 por jugador segun GDD).
    /// </summary>
    private void HandlePowerUnlocked(PlayerID player, PowerType power)
    {
        if (player == PlayerID.Player1 && p1AvailablePower == PowerType.None)
        {
            p1AvailablePower = power;
            Debug.Log(string.Format("[PlayerInput] P1 desbloqueo poder: {0}.", power));
        }
        else if (player == PlayerID.Player2 && p2AvailablePower == PowerType.None)
        {
            p2AvailablePower = power;
            Debug.Log(string.Format("[PlayerInput] P2 desbloqueo poder: {0}.", power));
        }
    }

    /// <summary>
    /// Detecta si el poder activado es ElectricPotato, que reduce la ventana de precision
    /// del jugador victima mientras el efecto este activo.
    /// Registra la victima para revertir correctamente al expirar.
    /// </summary>
    private void HandlePowerActivated(PlayerID attacker, PowerType power)
    {
        if (power != PowerType.ElectricPotato) return;

        PlayerID victim = (attacker == PlayerID.Player1) ? PlayerID.Player2 : PlayerID.Player1;

        if (victim == PlayerID.Player1) p1ElectricActive = true;
        else                            p2ElectricActive = true;

        electricVictim = victim;
        Debug.Log(string.Format("[PlayerInput] Papa Electrica activa contra {0}.", victim));
    }

    /// <summary>
    /// Revierte el efecto de ElectricPotato SOLO en el jugador que fue victima del poder.
    /// Evita apagar el estado del otro jugador si en el futuro hubiera efectos solapados.
    /// </summary>
    private void HandlePowerExpired(PowerType power)
    {
        if (power != PowerType.ElectricPotato) return;

        if (electricVictim == PlayerID.Player1)      p1ElectricActive = false;
        else if (electricVictim == PlayerID.Player2) p2ElectricActive = false;

        electricVictim = PlayerID.None;
        Debug.Log("[PlayerInput] Papa Electrica expirada. Ventana de precision restaurada.");
    }

    /// <summary>
    /// Limpia todo el estado de poderes al iniciar una nueva ronda.
    /// Garantiza que efectos de rondas anteriores no persistan.
    /// </summary>
    private void HandleRoundStarted(int roundNumber)
    {
        p1ElectricActive = false;
        p2ElectricActive = false;
        electricVictim   = PlayerID.None;
        p1AvailablePower = PowerType.None;
        p2AvailablePower = PowerType.None;
    }

    #endregion
}
