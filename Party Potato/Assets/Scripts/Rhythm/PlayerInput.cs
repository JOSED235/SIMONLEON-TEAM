using System;
using UnityEngine;

/// <summary>
/// Mapeo de teclas de entrada para un jugador en Beat Potato (Papa Beat).
/// </summary>
[System.Serializable]
public class PlayerKeyBindings
{
    public KeyCode leftKey = KeyCode.LeftArrow;
    public KeyCode downKey = KeyCode.DownArrow;
    public KeyCode upKey = KeyCode.UpArrow;
    public KeyCode rightKey = KeyCode.RightArrow;
    public KeyCode powerKey = KeyCode.Return;
    public KeyCode altPowerKey = KeyCode.RightShift;
}

/// <summary>
/// Procesa las entradas del teclado para ambos jugadores en el mismo PC,
/// detecta la nota mas cercana en el carril correspondiente, solicita la evaluacion temporal a RhythmManager,
/// previene doble consumo y notifica activacion de poderes.
/// </summary>
public class PlayerInput : MonoBehaviour
{
    [Header("Modo de Juego")]
    [Tooltip("Modo actual: LocalPVP (ambos jugadores en teclado) o SoloVsCPU (solo lee P1).")]
    public GameMode gameMode = GameMode.LocalPVP;

    [Header("Mapeo de Teclas")]
    [Tooltip("Configuracion de teclas para el Jugador 1 (por defecto: Flechas + Enter).")]
    public PlayerKeyBindings player1Keys = new PlayerKeyBindings();

    [Tooltip("Configuracion de teclas para el Jugador 2 (por defecto: WASD + Espacio).")]
    public PlayerKeyBindings player2Keys = new PlayerKeyBindings
    {
        leftKey = KeyCode.A,
        downKey = KeyCode.S,
        upKey = KeyCode.W,
        rightKey = KeyCode.D,
        powerKey = KeyCode.Space,
        altPowerKey = KeyCode.LeftShift
    };

    [Header("Reglas de Evaluacion")]
    [Tooltip("Si esta activo, pulsar una tecla cuando no hay notas en la ventana se penaliza como fallo (anti-spam).")]
    public bool penalizeGhostTaps = true;

    [Tooltip("Tiempo de tolerancia extra en segundos para considerar una nota golpeable.")]
    public float extraWindowTolerance = 0.02f;

    // Estado de poderes activos que alteran la ventana (Papa Electrica)
    private bool p1ElectricActive = false;
    private bool p2ElectricActive = false;

    // Control de poderes disponibles para activar
    private PowerType p1AvailablePower = PowerType.None;
    private PowerType p2AvailablePower = PowerType.None;

    private void OnEnable()
    {
        GameEvents.OnPowerUnlocked += HandlePowerUnlocked;
        GameEvents.OnPowerActivated += HandlePowerActivated;
        GameEvents.OnPowerExpired += HandlePowerExpired;
        GameEvents.OnRoundStarted += HandleRoundStarted;
    }

    private void OnDisable()
    {
        GameEvents.OnPowerUnlocked -= HandlePowerUnlocked;
        GameEvents.OnPowerActivated -= HandlePowerActivated;
        GameEvents.OnPowerExpired -= HandlePowerExpired;
        GameEvents.OnRoundStarted -= HandleRoundStarted;
    }

    private void Update()
    {
        if (RhythmManager.Instance == null || !RhythmManager.Instance.isPlaying || RhythmManager.Instance.isPaused)
        {
            return;
        }

        // Procesar entradas de Jugador 1
        ProcessPlayerInputs(PlayerID.Player1, player1Keys);

        // Procesar entradas de Jugador 2 (solo en modo LocalPVP)
        if (gameMode == GameMode.LocalPVP)
        {
            ProcessPlayerInputs(PlayerID.Player2, player2Keys);
        }
    }

    private void ProcessPlayerInputs(PlayerID player, PlayerKeyBindings bindings)
    {
        if (Input.GetKeyDown(bindings.leftKey))
        {
            HandleLanePress(player, 0); // Lane 0 = Left
        }
        if (Input.GetKeyDown(bindings.downKey))
        {
            HandleLanePress(player, 1); // Lane 1 = Down
        }
        if (Input.GetKeyDown(bindings.upKey))
        {
            HandleLanePress(player, 2); // Lane 2 = Up
        }
        if (Input.GetKeyDown(bindings.rightKey))
        {
            HandleLanePress(player, 3); // Lane 3 = Right
        }

        // Activacion de Poder Especial
        if (Input.GetKeyDown(bindings.powerKey) || Input.GetKeyDown(bindings.altPowerKey))
        {
            HandlePowerPress(player);
        }
    }

    /// <summary>
    /// Procesa la pulsacion de un carril para un jugador, evaluando la nota mas cercana.
    /// </summary>
    public void HandleLanePress(PlayerID player, int laneID)
    {
        if (RhythmManager.Instance == null) return;

        float currentBeat = RhythmManager.Instance.songPositionInBeats;
        float secPerBeat = RhythmManager.Instance.secPerBeat;
        if (secPerBeat <= 0f) return;

        // Tolerancia maxima (Good tolerance de GameConfig + margen extra)
        float goodTol = (RhythmManager.Instance.gameConfig != null) ? RhythmManager.Instance.gameConfig.goodTolerance : 0.15f;
        float maxBeatTolerance = (goodTol + extraWindowTolerance) / secPerBeat;

        // Buscar la nota activa mas cercana en este carril
        NoteObject targetNote = NoteObject.GetClosestActiveNote(player, laneID, currentBeat, maxBeatTolerance);

        bool isElectric = (player == PlayerID.Player1) ? p1ElectricActive : p2ElectricActive;

        if (targetNote != null && !targetNote.isResolved)
        {
            // Evaluar la precision exacta respecto al beat objetivo de la nota
            float timeDiff;
            float accuracy;
            HitResult result = RhythmManager.Instance.EvaluateHit(targetNote.beatOfThisNote, out timeDiff, out accuracy, isElectric);

            if (result != HitResult.Miss)
            {
                targetNote.ResolveHit(result, accuracy);
                Debug.Log(string.Format("[PlayerInput] {0} acerto carril {1} ({2}). Desfase: {3:F3}s, Precision: {4:F0}%", player, laneID, result, timeDiff, accuracy * 100f));
            }
            else
            {
                targetNote.ResolveMiss();
            }
        }
        else
        {
            // Pulsacion en vacio (Ghost Tap)
            if (penalizeGhostTaps)
            {
                GameEvents.TriggerPlayerMiss(player);
                GameEvents.TriggerHitResolved(player, HitResult.Miss, 0f);
                Debug.Log(string.Format("[PlayerInput] Ghost Tap (Miss) para {0} en carril {1} sin nota valida.", player, laneID));
            }
        }
    }

    /// <summary>
    /// Activa el poder especial disponible si el jugador posee una carga.
    /// </summary>
    private void HandlePowerPress(PlayerID player)
    {
        PowerType powerToActivate = (player == PlayerID.Player1) ? p1AvailablePower : p2AvailablePower;

        if (powerToActivate != PowerType.None)
        {
            if (player == PlayerID.Player1) p1AvailablePower = PowerType.None;
            else p2AvailablePower = PowerType.None;

            GameEvents.TriggerPowerActivated(player, powerToActivate);
            Debug.Log(string.Format("[PlayerInput] {0} activo su poder especial: {1}.", player, powerToActivate));
        }
        else
        {
            Debug.Log(string.Format("[PlayerInput] {0} intento usar poder, pero no tiene cargas disponibles.", player));
        }
    }

    #region Manejo de Eventos

    private void HandlePowerUnlocked(PlayerID player, PowerType power)
    {
        if (player == PlayerID.Player1) p1AvailablePower = power;
        else p2AvailablePower = power;

        Debug.Log(string.Format("[PlayerInput] {0} desbloqueo poder: {1}.", player, power));
    }

    private void HandlePowerActivated(PlayerID attacker, PowerType power)
    {
        PlayerID victim = (attacker == PlayerID.Player1) ? PlayerID.Player2 : PlayerID.Player1;

        if (power == PowerType.ElectricPotato)
        {
            if (victim == PlayerID.Player1) p1ElectricActive = true;
            else p2ElectricActive = true;
        }
    }

    private void HandlePowerExpired(PowerType power)
    {
        if (power == PowerType.ElectricPotato)
        {
            p1ElectricActive = false;
            p2ElectricActive = false;
        }
    }

    private void HandleRoundStarted(int roundNumber)
    {
        p1ElectricActive = false;
        p2ElectricActive = false;
        p1AvailablePower = PowerType.None;
        p2AvailablePower = PowerType.None;
    }

    #endregion
}
