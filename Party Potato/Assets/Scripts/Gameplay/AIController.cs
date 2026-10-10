using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controlador de Inteligencia Artificial para el modo 1 Jugador en Beat Potato (Papa Beat).
/// Opera como rival virtual (por defecto Jugador 2) sincronizado con el compas musical.
///
/// Responsabilidades segun la division de responsabilidades del backend:
/// 1. Responder a las notas de su lado usando los mismos tiempos objetivo y flujo de evaluacion que una entrada humana.
/// 2. Ajustar la probabilidad de acierto y precision segun AIDifficulty:
///    - Facil (~70% precision, predominan Good y fallos frecuentes).
///    - Normal (~85% precision, combina Great y Perfect, fallos ocasionales).
///    - Dificil (~96% precision, altas rachas de Perfect/Great y uso activo de poderes).
/// 3. Activar poderes especiales desbloqueados segun el perfil de dificultad.
/// 4. No duplicar el motor de notas ni la logica de puntuacion (delega en NoteObject y RhythmManager).
/// </summary>
public class AIController : MonoBehaviour
{
    [Header("Configuracion de Jugador y Modo")]
    [Tooltip("Jugador operado por esta IA (por defecto Player2).")]
    public PlayerID aiPlayer = PlayerID.Player2;

    [Tooltip("Dificultad de la IA segun el GDD: Easy (70%), Normal (85%), Hard (96%).")]
    public AIDifficulty difficulty = AIDifficulty.Normal;

    [Tooltip("Habilita o deshabilita la actividad de la IA.")]
    public bool isAIActive = true;

    [Header("Poderes Especiales")]
    [Tooltip("Si true, la IA activa los poderes que desbloquea tras alcanzar la racha requerida.")]
    public bool autoUsePowers = true;

    // Estado interno de poderes
    private PowerType availablePower = PowerType.None;
    private float beatToActivatePower = -1f;
    private bool isElectricActive = false;

    // Propiedades de solo lectura para consulta y pruebas
    public PowerType currentAvailablePower { get { return availablePower; } }
    public float scheduledPowerBeat { get { return beatToActivatePower; } }
    public bool isElectricEffectActive { get { return isElectricActive; } }

    // Notas programadas para golpear: mapea NoteObject -> targetBeat (en songPositionInBeats).
    // Si targetBeat es float.MaxValue, la IA decidio omitir la nota (auto-miss intencional).
    private readonly Dictionary<NoteObject, float> scheduledNotes = new Dictionary<NoteObject, float>();
    private readonly List<NoteObject> notesToCleanup = new List<NoteObject>();

    public int scheduledNotesCount { get { return scheduledNotes.Count; } }

    private void OnEnable()
    {
        GameEvents.OnPowerUnlocked  += HandlePowerUnlocked;
        GameEvents.OnPowerActivated += HandlePowerActivated;
        GameEvents.OnPowerExpired   += HandlePowerExpired;
        GameEvents.OnRoundStarted   += HandleRoundStarted;
        GameEvents.OnRoundEnded     += HandleRoundEnded;
    }

    private void OnDisable()
    {
        GameEvents.OnPowerUnlocked  -= HandlePowerUnlocked;
        GameEvents.OnPowerActivated -= HandlePowerActivated;
        GameEvents.OnPowerExpired   -= HandlePowerExpired;
        GameEvents.OnRoundStarted   -= HandleRoundStarted;
        GameEvents.OnRoundEnded     -= HandleRoundEnded;
    }

    private void Update()
    {
        if (!isAIActive) return;
        if (RhythmManager.Instance == null || !RhythmManager.Instance.isPlaying || RhythmManager.Instance.isPaused) return;

        ProcessNotes();
        ProcessPower();
    }

    /// <summary>
    /// Escanea las notas activas para el jugador de la IA, programa los tiempos de impacto
    /// y resuelve aquellas cuyo instante programado ha llegado.
    /// </summary>
    public void ProcessNotes()
    {
        if (RhythmManager.Instance == null) return;
        float currentBeat = RhythmManager.Instance.songPositionInBeats;
        float secPerBeat = RhythmManager.Instance.secPerBeat;

        // 1. Programar notas nuevas que aun no han sido registradas
        for (int i = 0; i < NoteObject.ActiveNotes.Count; i++)
        {
            NoteObject note = NoteObject.ActiveNotes[i];
            if (note == null || note.isResolved) continue;
            if (note.targetPlayer != aiPlayer) continue;

            if (!scheduledNotes.ContainsKey(note))
            {
                float offsetSec = GenerateTimingOffset(difficulty, isElectricActive);
                if (offsetSec == float.MaxValue)
                {
                    // La IA decidio fallar esta nota: se marca con MaxValue para dejarla expirar en auto-miss
                    scheduledNotes[note] = float.MaxValue;
                }
                else
                {
                    float offsetBeats = (secPerBeat > 0f) ? offsetSec / secPerBeat : 0f;
                    scheduledNotes[note] = note.beatOfThisNote + offsetBeats;
                }
            }
        }

        // 2. Ejecutar impactos cuando el reloj musical alcanza el beat programado
        notesToCleanup.Clear();
        foreach (var kvp in scheduledNotes)
        {
            NoteObject note = kvp.Key;
            float targetBeat = kvp.Value;

            if (note == null || note.isResolved)
            {
                notesToCleanup.Add(note);
                continue;
            }

            if (targetBeat != float.MaxValue && currentBeat >= targetBeat)
            {
                // Ha llegado el momento programado: resolver la nota con el mismo flujo del jugador humano
                note.EvaluateAndResolve(isElectricActive);
                notesToCleanup.Add(note);
            }
            else if (targetBeat == float.MaxValue && currentBeat > note.beatOfThisNote + 1.0f)
            {
                // La nota ya paso ampliamente y debio ser procesada por auto-miss
                notesToCleanup.Add(note);
            }
        }

        for (int i = 0; i < notesToCleanup.Count; i++)
        {
            scheduledNotes.Remove(notesToCleanup[i]);
        }
    }

    /// <summary>
    /// Genera el desfase temporal simulado (en segundos) segun la dificultad configurada.
    /// Retorna float.MaxValue si la IA decide omitir la nota (Miss intencional/auto-miss).
    /// Permite inyeccion de forcedRoll (0..100) y forcedSign (-1 o +1) para pruebas unitarias deterministas.
    /// </summary>
    public float GenerateTimingOffset(AIDifficulty diff, bool isElectric = false, float? forcedRoll = null, float? forcedSign = null)
    {
        float roll = forcedRoll.HasValue ? forcedRoll.Value : UnityEngine.Random.Range(0f, 100f);
        float sign = forcedSign.HasValue ? forcedSign.Value : (UnityEngine.Random.Range(0f, 1f) > 0.5f ? 1f : -1f);

        // Tolerancias de referencia de GameConfig
        GameConfig cfg = (RhythmManager.Instance != null) ? RhythmManager.Instance.gameConfig : null;
        float perfTol = (isElectric && cfg != null) ? cfg.electricPerfectTolerance : (cfg != null ? cfg.perfectTolerance : 0.05f);
        float greatTol = (cfg != null) ? cfg.greatTolerance : 0.10f;
        float goodTol  = (cfg != null) ? cfg.goodTolerance  : 0.15f;

        switch (diff)
        {
            case AIDifficulty.Easy:
                // Easy: ~70% precision, predominan Good y fallos frecuentes
                if (roll < 30f)
                {
                    return float.MaxValue; // 30% Miss
                }
                else if (roll < 75f)
                {
                    // 45% Good: entre greatTol y goodTol
                    return sign * UnityEngine.Random.Range(greatTol + 0.005f, goodTol - 0.005f);
                }
                else if (roll < 95f)
                {
                    // 20% Great: entre perfTol y greatTol
                    return sign * UnityEngine.Random.Range(perfTol + 0.005f, greatTol - 0.005f);
                }
                else
                {
                    // 5% Perfect: menor a perfTol
                    return sign * UnityEngine.Random.Range(0f, perfTol * 0.8f);
                }

            case AIDifficulty.Normal:
                // Normal: ~85% precision, combina Great y Perfect, fallos ocasionales
                if (roll < 15f)
                {
                    return float.MaxValue; // 15% Miss
                }
                else if (roll < 30f)
                {
                    // 15% Good
                    return sign * UnityEngine.Random.Range(greatTol + 0.005f, goodTol - 0.005f);
                }
                else if (roll < 70f)
                {
                    // 40% Great
                    return sign * UnityEngine.Random.Range(perfTol + 0.005f, greatTol - 0.005f);
                }
                else
                {
                    // 30% Perfect
                    return sign * UnityEngine.Random.Range(0f, perfTol * 0.8f);
                }

            case AIDifficulty.Hard:
            default:
                // Hard: ~96% precision, altas rachas de Perfect/Great
                if (roll < 4f)
                {
                    return float.MaxValue; // 4% Miss
                }
                else if (roll < 10f)
                {
                    // 6% Good
                    return sign * UnityEngine.Random.Range(greatTol + 0.005f, goodTol - 0.005f);
                }
                else if (roll < 40f)
                {
                    // 30% Great
                    return sign * UnityEngine.Random.Range(perfTol + 0.005f, greatTol - 0.005f);
                }
                else
                {
                    // 60% Perfect
                    return sign * UnityEngine.Random.Range(0f, perfTol * 0.8f);
                }
        }
    }

    /// <summary>
    /// Metodo de utilidad para pruebas y simulaciones: programa y ejecuta inmediatamente
    /// el impacto de una nota dada segun la distribucion de la dificultad.
    /// </summary>
    public HitResult SimulateHitForNote(NoteObject note, float? forcedRoll = null, float? forcedSign = null)
    {
        if (note == null || note.isResolved) return HitResult.Miss;

        float offsetSec = GenerateTimingOffset(difficulty, isElectricActive, forcedRoll, forcedSign);
        if (offsetSec == float.MaxValue)
        {
            note.ResolveMiss();
            return HitResult.Miss;
        }

        float acc = 0f;
        HitResult result = HitResult.Miss;
        if (RhythmManager.Instance != null)
        {
            result = RhythmManager.Instance.ClassifyTimeDifference(Mathf.Abs(offsetSec), out acc, isElectricActive);
        }

        if (result != HitResult.Miss)
        {
            note.ResolveHit(result, acc);
        }
        else
        {
            note.ResolveMiss();
        }

        return result;
    }

    /// <summary>
    /// Procesa la activacion automatica de poderes acumulados segun el retraso programado.
    /// </summary>
    public void ProcessPower()
    {
        if (!autoUsePowers || availablePower == PowerType.None || beatToActivatePower < 0f) return;
        if (RhythmManager.Instance == null) return;

        if (RhythmManager.Instance.songPositionInBeats >= beatToActivatePower)
        {
            UseAvailablePower();
        }
    }

    /// <summary>
    /// Activa y consume el poder actualmente disponible de la IA.
    /// </summary>
    public void UseAvailablePower()
    {
        if (availablePower == PowerType.None) return;

        PowerType powerToTrigger = availablePower;
        availablePower = PowerType.None;
        beatToActivatePower = -1f;

        GameEvents.TriggerPowerActivated(aiPlayer, powerToTrigger);
        Debug.Log(string.Format("[AIController] {0} (CPU {1}) activo poder: {2}.", aiPlayer, difficulty, powerToTrigger));
    }

    /// <summary>
    /// Retorna el retraso en beats antes de usar un poder segun la dificultad configurada.
    /// Hard usa el poder casi inmediatamente (0.25 beats), Normal espera 2 beats (2.0),
    /// y Easy espera 4 beats (4.0).
    /// </summary>
    public float GetPowerDelayForDifficulty(AIDifficulty diff)
    {
        switch (diff)
        {
            case AIDifficulty.Hard:   return 0.25f;
            case AIDifficulty.Normal: return 2.0f;
            case AIDifficulty.Easy:
            default:                  return 4.0f;
        }
    }

    #region Eventos

    private void HandlePowerUnlocked(PlayerID player, PowerType power)
    {
        if (player != aiPlayer) return;

        // Maximo una carga almacenada segun GDD
        if (availablePower == PowerType.None)
        {
            availablePower = power;
            float currentBeat = (RhythmManager.Instance != null) ? RhythmManager.Instance.songPositionInBeats : 0f;
            beatToActivatePower = currentBeat + GetPowerDelayForDifficulty(difficulty);
            Debug.Log(string.Format("[AIController] {0} desbloqueo poder {1}. Programado para beat {2:F2}.",
                                    aiPlayer, power, beatToActivatePower));
        }
    }

    private void HandlePowerActivated(PlayerID attacker, PowerType power)
    {
        if (power != PowerType.ElectricPotato) return;

        // Si el rival activo ElectricPotato, la IA es la victima
        if (attacker != aiPlayer)
        {
            isElectricActive = true;
            Debug.Log(string.Format("[AIController] {0} bajo efecto de Papa Electrica.", aiPlayer));
        }
    }

    private void HandlePowerExpired(PowerType power)
    {
        if (power == PowerType.ElectricPotato)
        {
            isElectricActive = false;
            Debug.Log(string.Format("[AIController] Papa Electrica expirada para {0}.", aiPlayer));
        }
    }

    private void HandleRoundStarted(int roundNumber)
    {
        scheduledNotes.Clear();
        notesToCleanup.Clear();
        availablePower = PowerType.None;
        beatToActivatePower = -1f;
        isElectricActive = false;
    }

    private void HandleRoundEnded(PlayerID winner, int p1Score, int p2Score)
    {
        scheduledNotes.Clear();
        notesToCleanup.Clear();
    }

    #endregion
}
