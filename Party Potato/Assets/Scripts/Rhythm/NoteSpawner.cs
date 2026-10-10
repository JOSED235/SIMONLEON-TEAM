using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Representa un carril individual de notas al estilo FNF (Left, Down, Up, Right).
/// </summary>
[System.Serializable]
public class NoteLane
{
    public string laneName;
    public LaneDirection direction;
    public Transform spawnPoint;
    public Transform hitZone;
    public GameObject specificNotePrefab;
}

/// <summary>
/// Paso individual dentro de un patron ritmico estructurado.
/// </summary>
[System.Serializable]
public struct NotePatternStep
{
    public float beat;
    public LaneDirection direction;
    /// <summary>Si es None, se genera para el jugador asignado al spawner o ambos en dualPlayerMode.</summary>
    public PlayerID targetPlayer;
}

/// <summary>
/// Modo de generacion de notas por el Spawner.
/// </summary>
public enum SpawnerMode
{
    Procedural  = 0,   // Genera notas ritmicas procedurales cada intervalo de beats
    PatternList = 1    // Genera notas a partir de una secuencia o lista predefinida
}

/// <summary>
/// Orquesta la generacion anticipada de notas para ambos jugadores en Beat Potato (Papa Beat).
/// Soporta configuracion independiente por jugador o modo dual simultaneo,
/// patrones procedimentales o estructurados y reacciona a sabotajes de poderes
/// (Papa Cohete acelera notas, Papa Fantasma las oculta a mitad de camino).
///
/// Reglas clave:
/// - RhythmManager es la unica fuente de tiempo: toda la programacion usa songPositionInBeats.
/// - Cada nota se instancia con su beatOfThisNote exacto; el movimiento visual ocurre en NoteObject.
/// - Los modificadores de sabotaje se rastrean por jugador VICTIMA, no por jugador atacante.
/// - Al expirar un poder, solo se restablece el modificador del jugador que fue afectado.
/// </summary>
public class NoteSpawner : MonoBehaviour
{
    [Header("Identificacion de Jugador")]
    [Tooltip("Jugador al que pertenece este Spawner (ignorado en dualPlayerMode).")]
    public PlayerID player = PlayerID.Player1;

    [Tooltip("Si esta activo, administra simultaneamente los carriles de Player1 y Player2 desde un unico spawner.")]
    public bool dualPlayerMode = false;

    [Header("Configuracion General")]
    public GameObject defaultNotePrefab;

    [Tooltip("Cantidad de beats que dura una nota en pantalla antes de llegar a la HitZone.")]
    public float beatsOnScreen = 2f;

    [Tooltip("Intervalo en beats entre notas en modo procedural (ej. 1.0 = cada beat, 0.5 = corcheas).")]
    public float proceduralBeatInterval = 1f;

    [Header("Carriles de Notas")]
    [Tooltip("Carriles primarios (P1 en modo dual o del jugador asignado).")]
    public NoteLane[] lanes;

    [Tooltip("Carriles del Jugador 2 (utilizados unicamente si dualPlayerMode esta activo).")]
    public NoteLane[] player2Lanes;

    [Header("Patrones de Generacion")]
    public SpawnerMode spawnerMode = SpawnerMode.Procedural;

    [Tooltip("Lista de notas predefinidas para modo PatternList.")]
    public List<NotePatternStep> patternSequence = new List<NotePatternStep>();

    // -----------------------------------------------------------------------
    // Estado interno de generacion
    // -----------------------------------------------------------------------
    private float nextBeatToSpawn = 1f;
    private int currentPatternIndex = 0;

    // -----------------------------------------------------------------------
    // Modificadores activos por jugador VICTIMA (no por atacante).
    // Se rastrean individualmente para que la expiracion sea precisa.
    // -----------------------------------------------------------------------
    private float p1SpeedMultiplier = 1f;
    private float p2SpeedMultiplier = 1f;
    private bool p1GhostActive = false;
    private bool p2GhostActive = false;

    // Registro de a quien se le aplico cada poder activo (para revertirlo al expirar).
    private PlayerID rocketVictim = PlayerID.None;
    private PlayerID ghostVictim  = PlayerID.None;

    // -----------------------------------------------------------------------
    // Ciclo de vida de Unity
    // -----------------------------------------------------------------------

    private void OnEnable()
    {
        GameEvents.OnPowerActivated += HandlePowerActivated;
        GameEvents.OnPowerExpired   += HandlePowerExpired;
        GameEvents.OnRoundStarted   += HandleRoundStarted;
    }

    private void OnDisable()
    {
        GameEvents.OnPowerActivated -= HandlePowerActivated;
        GameEvents.OnPowerExpired   -= HandlePowerExpired;
        GameEvents.OnRoundStarted   -= HandleRoundStarted;
    }

    private void Start()
    {
        ResetSpawner();
    }

    // -----------------------------------------------------------------------
    // API publica
    // -----------------------------------------------------------------------

    /// <summary>
    /// Reinicia los contadores de spawn y los modificadores de sabotaje.
    /// Se llama al inicio de cada ronda y al arrancar el componente.
    /// </summary>
    public void ResetSpawner()
    {
        nextBeatToSpawn      = 1f;
        currentPatternIndex  = 0;
        p1SpeedMultiplier    = 1f;
        p2SpeedMultiplier    = 1f;
        p1GhostActive        = false;
        p2GhostActive        = false;
        rocketVictim         = PlayerID.None;
        ghostVictim          = PlayerID.None;
    }

    // -----------------------------------------------------------------------
    // Update: despacha el modo de spawn activo
    // -----------------------------------------------------------------------

    private void Update()
    {
        if (RhythmManager.Instance == null) return;

        float currentBeat = RhythmManager.Instance.songPositionInBeats;

        if (spawnerMode == SpawnerMode.Procedural)
        {
            UpdateProceduralSpawning(currentBeat);
        }
        else if (spawnerMode == SpawnerMode.PatternList)
        {
            UpdatePatternSpawning(currentBeat);
        }
    }

    // -----------------------------------------------------------------------
    // Modo Procedural
    // -----------------------------------------------------------------------

    private void UpdateProceduralSpawning(float currentBeat)
    {
        if (lanes == null || lanes.Length == 0) return;

        // Generar notas con anticipacion suficiente segun beatsOnScreen.
        // La condicion garantiza que la nota sea instanciada antes de que
        // su beat objetivo llegue a pantalla.
        while (currentBeat >= (nextBeatToSpawn - beatsOnScreen))
        {
            if (dualPlayerMode)
            {
                int laneP1 = UnityEngine.Random.Range(0, lanes.Length);
                SpawnNoteForPlayer(PlayerID.Player1, laneP1, nextBeatToSpawn,
                                   lanes, p1SpeedMultiplier, p1GhostActive);

                if (player2Lanes != null && player2Lanes.Length > 0)
                {
                    int laneP2 = UnityEngine.Random.Range(0, player2Lanes.Length);
                    SpawnNoteForPlayer(PlayerID.Player2, laneP2, nextBeatToSpawn,
                                       player2Lanes, p2SpeedMultiplier, p2GhostActive);
                }
            }
            else
            {
                int laneIndex = UnityEngine.Random.Range(0, lanes.Length);
                float speed = (player == PlayerID.Player1) ? p1SpeedMultiplier : p2SpeedMultiplier;
                bool  ghost = (player == PlayerID.Player1) ? p1GhostActive    : p2GhostActive;
                SpawnNoteForPlayer(player, laneIndex, nextBeatToSpawn, lanes, speed, ghost);
            }

            nextBeatToSpawn += Mathf.Max(0.25f, proceduralBeatInterval);
        }
    }

    // -----------------------------------------------------------------------
    // Modo PatternList
    // -----------------------------------------------------------------------

    private void UpdatePatternSpawning(float currentBeat)
    {
        if (patternSequence == null || patternSequence.Count == 0) return;

        while (currentPatternIndex < patternSequence.Count)
        {
            NotePatternStep step = patternSequence[currentPatternIndex];

            // Aun no es tiempo de instanciar esta nota (falta anticipacion).
            if (currentBeat < (step.beat - beatsOnScreen))
            {
                break;
            }

            PlayerID target = (step.targetPlayer != PlayerID.None) ? step.targetPlayer : player;
            int laneIdx = (int)step.direction;

            if (dualPlayerMode)
            {
                if (target == PlayerID.Player1 || target == PlayerID.None)
                {
                    SpawnNoteForPlayer(PlayerID.Player1, laneIdx, step.beat,
                                       lanes, p1SpeedMultiplier, p1GhostActive);
                }
                if (target == PlayerID.Player2 || target == PlayerID.None)
                {
                    NoteLane[] p2L = (player2Lanes != null && player2Lanes.Length > 0)
                                     ? player2Lanes : lanes;
                    SpawnNoteForPlayer(PlayerID.Player2, laneIdx, step.beat,
                                       p2L, p2SpeedMultiplier, p2GhostActive);
                }
            }
            else
            {
                float speed = (target == PlayerID.Player1) ? p1SpeedMultiplier : p2SpeedMultiplier;
                bool  ghost = (target == PlayerID.Player1) ? p1GhostActive    : p2GhostActive;
                SpawnNoteForPlayer(target, laneIdx, step.beat, lanes, speed, ghost);
            }

            currentPatternIndex++;
        }
    }

    // -----------------------------------------------------------------------
    // Instanciacion de una nota individual
    // -----------------------------------------------------------------------

    private void SpawnNoteForPlayer(PlayerID targetPlayer, int laneIndex, float beatTarget,
                                    NoteLane[] laneSet, float speedMult, bool ghost)
    {
        if (laneSet == null || laneIndex < 0 || laneIndex >= laneSet.Length) return;

        NoteLane lane = laneSet[laneIndex];
        if (lane == null || lane.spawnPoint == null || lane.hitZone == null) return;

        GameObject prefabToUse = (lane.specificNotePrefab != null) ? lane.specificNotePrefab
                                                                    : defaultNotePrefab;
        if (prefabToUse == null) return;

        GameObject newNote = Instantiate(prefabToUse, lane.spawnPoint.position,
                                         Quaternion.identity, transform);
        NoteObject noteObj = newNote.GetComponent<NoteObject>();
        if (noteObj == null)
        {
            noteObj = newNote.AddComponent<NoteObject>();
        }

        noteObj.targetPlayer   = targetPlayer;
        noteObj.laneID         = laneIndex;
        noteObj.laneDirection  = lane.direction;
        noteObj.beatOfThisNote = beatTarget;
        noteObj.beatsOnScreen  = beatsOnScreen;
        noteObj.speedMultiplier = speedMult;
        noteObj.isGhost        = ghost;
        noteObj.spawnPos       = lane.spawnPoint.position;
        noteObj.targetPos      = lane.hitZone.position;
    }

    // -----------------------------------------------------------------------
    // Manejo de Poderes y Sabotajes
    // -----------------------------------------------------------------------

    #region Poderes

    private void HandlePowerActivated(PlayerID attacker, PowerType power)
    {
        // La victima es siempre el contrario del que activa el poder.
        PlayerID victim = (attacker == PlayerID.Player1) ? PlayerID.Player2 : PlayerID.Player1;

        if (power == PowerType.RocketPotato)
        {
            float mult = 1.5f;
            if (RhythmManager.Instance != null && RhythmManager.Instance.gameConfig != null)
            {
                mult = RhythmManager.Instance.gameConfig.rocketSpeedMultiplier;
            }

            // Aplicar el multiplicador SOLO a la victima y registrarla.
            if (victim == PlayerID.Player1) p1SpeedMultiplier = mult;
            else                            p2SpeedMultiplier = mult;
            rocketVictim = victim;

            Debug.Log(string.Format("[NoteSpawner] Papa Cohete activada contra {0}. Multiplicador: {1}x",
                                    victim, mult));
        }
        else if (power == PowerType.GhostPotato)
        {
            // Activar el efecto fantasma SOLO en la victima y registrarla.
            if (victim == PlayerID.Player1) p1GhostActive = true;
            else                            p2GhostActive = true;
            ghostVictim = victim;

            Debug.Log(string.Format("[NoteSpawner] Papa Fantasma activada contra {0}.", victim));
        }
    }

    private void HandlePowerExpired(PowerType power)
    {
        // Revertir SOLO el modificador del jugador que fue victima del efecto.
        // Esto evita desactivar efectos activos sobre el otro jugador si expirar
        // coincide con un poder distinto en curso.
        if (power == PowerType.RocketPotato)
        {
            if (rocketVictim == PlayerID.Player1) p1SpeedMultiplier = 1f;
            else if (rocketVictim == PlayerID.Player2) p2SpeedMultiplier = 1f;
            rocketVictim = PlayerID.None;

            Debug.Log("[NoteSpawner] Papa Cohete expirada. Velocidad de notas restaurada.");
        }
        else if (power == PowerType.GhostPotato)
        {
            if (ghostVictim == PlayerID.Player1) p1GhostActive = false;
            else if (ghostVictim == PlayerID.Player2) p2GhostActive = false;
            ghostVictim = PlayerID.None;

            Debug.Log("[NoteSpawner] Papa Fantasma expirada. Visibilidad de notas restaurada.");
        }
    }

    private void HandleRoundStarted(int roundNumber)
    {
        ResetSpawner();
    }

    #endregion
}
