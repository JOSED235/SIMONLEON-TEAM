using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Representa una nota fisica en movimiento hacia la HitZone en Beat Potato (Papa Beat).
///
/// Responsabilidades:
/// - Interpolar visualmente la posicion entre spawnPoint y hitZone usando beats como unidad de tiempo.
/// - Aplicar el efecto visual del sabotaje Papa Fantasma (ocultamiento parcial a mitad de camino).
/// - Declarar un Miss automatico una sola vez cuando la nota supera el umbral de vencimiento.
/// - Exponer ResolveHit para que PlayerInput / AIController lo invoquen al detectar un golpe.
/// - Proveer busqueda de notas activas (por jugador y carril) para PlayerInput / AIController.
///
/// Invariante: cada nota se resuelve exactamente una vez (isResolved garantiza esto).
/// La evaluacion temporal (Perfect/Great/Good/Miss) ocurre en RhythmManager.EvaluateHit,
/// NO en esta clase. Esta clase solo notifica el resultado ya calculado.
/// </summary>
public class NoteObject : MonoBehaviour
{
    // Registro global de notas activas en pantalla.
    // PlayerInput y AIController lo usan para localizar la nota mas proxima al presionar un carril.
    public static readonly List<NoteObject> ActiveNotes = new List<NoteObject>();

    // -----------------------------------------------------------------------
    // Campos serializados — Inspector de Unity
    // -----------------------------------------------------------------------

    [Header("Identificacion de Jugador y Carril")]
    [Tooltip("Jugador al que pertenece esta nota (Player1 o Player2).")]
    public PlayerID targetPlayer = PlayerID.Player1;

    [Tooltip("Indice del carril (0=Left, 1=Down, 2=Up, 3=Right).")]
    public int laneID = 0;

    [Tooltip("Direccion del carril correspondiente.")]
    public LaneDirection laneDirection = LaneDirection.Left;

    [Header("Sincronizacion de Ritmo")]
    [Tooltip("Beat exacto en el que esta nota debe alcanzar la HitZone.")]
    public float beatOfThisNote;

    [Tooltip("Cantidad de beats que dura la nota desplazandose en pantalla.")]
    public float beatsOnScreen = 2f;

    [Tooltip("Multiplicador de velocidad de caida (ej. 1.5x bajo efecto de Papa Cohete).")]
    public float speedMultiplier = 1f;

    [Header("Posiciones en Pantalla")]
    [HideInInspector] public Vector3 spawnPos;
    [HideInInspector] public Vector3 targetPos;

    [Header("Efectos Especiales de Poderes")]
    [Tooltip("Si true, la nota se oculta en el tramo medio por efecto de Papa Fantasma.")]
    public bool isGhost = false;

    [Tooltip("SpriteRenderer para efectos visuales de transparencia (Papa Fantasma).")]
    public SpriteRenderer spriteRenderer;

    [Header("Configuracion de Vencimiento")]
    [Tooltip("Progreso normalizado (1.0 = HitZone exacta) a partir del cual se declara Miss automatico.")]
    public float missThreshold = 1.25f;

    // -----------------------------------------------------------------------
    // Estado de resolucion (backing field + propiedad publica de solo lectura)
    // -----------------------------------------------------------------------

    private bool isResolvedField = false;

    /// <summary>
    /// Indica si la nota ya fue evaluada (por impacto o por miss automatico).
    /// Una vez true, ningun sistema puede volver a procesarla.
    /// </summary>
    public bool isResolved
    {
        get { return isResolvedField; }
        private set { isResolvedField = value; }
    }

    // -----------------------------------------------------------------------
    // Ciclo de vida de Unity
    // -----------------------------------------------------------------------

    private void OnEnable()
    {
        if (!ActiveNotes.Contains(this))
        {
            ActiveNotes.Add(this);
        }
    }

    private void OnDisable()
    {
        ActiveNotes.Remove(this);
    }

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private void Update()
    {
        if (RhythmManager.Instance == null || isResolvedField) return;

        float currentBeat = RhythmManager.Instance.songPositionInBeats;

        // Calculo de progreso normalizado (0 = spawn, 1 = HitZone).
        // speedMultiplier reduce el tiempo efectivo en pantalla: a mayor velocidad,
        // la nota tarda menos beats en recorrer el mismo espacio visual.
        float effectiveBeatsOnScreen = beatsOnScreen / Mathf.Max(0.1f, speedMultiplier);
        float spawnBeat = beatOfThisNote - effectiveBeatsOnScreen;
        float progress  = (currentBeat - spawnBeat) / effectiveBeatsOnScreen;

        // Movimiento visual: interpolacion lineal entre origen y HitZone.
        // La precision del golpe NO depende de esta posicion; usa beatOfThisNote y dspTime.
        transform.position = Vector3.Lerp(spawnPos, targetPos, progress);

        // Efecto Papa Fantasma: nota invisible en el tramo medio (35%–85% del recorrido).
        ApplyGhostEffect(progress);

        // Auto-Miss: la nota supero la HitZone sin ser pulsada.
        if (progress >= missThreshold)
        {
            ResolveMiss();
        }
    }

    // -----------------------------------------------------------------------
    // Logica de resolucion — llamadas externas de PlayerInput / AIController
    // -----------------------------------------------------------------------

    /// <summary>
    /// Resuelve la nota como un golpe acertado.
    /// Solo produce efecto la primera vez; llamadas posteriores se ignoran (isResolved).
    /// Si por error se envia HitResult.Miss, se redirige a ResolveMiss para preservar la coherencia de eventos.
    /// </summary>
    /// <param name="result">Calificacion del golpe (Perfect, Great, Good). Miss se redirige a ResolveMiss.</param>
    /// <param name="accuracy">Precision normalizada 0..1 calculada por RhythmManager.EvaluateHit.</param>
    public void ResolveHit(HitResult result, float accuracy)
    {
        if (isResolvedField) return;

        if (result == HitResult.Miss)
        {
            ResolveMiss();
            return;
        }

        isResolvedField = true;

        GameEvents.TriggerNoteHit(targetPlayer, laneID, result, accuracy);
        GameEvents.TriggerHitResolved(targetPlayer, result, accuracy);

        Destroy(gameObject);
    }

    /// <summary>
    /// Resuelve la nota como un fallo automatico por haber rebasado la HitZone o pulsacion a destiempo.
    /// Solo produce efecto la primera vez; llamadas posteriores se ignoran (isResolved).
    /// </summary>
    public void ResolveMiss()
    {
        if (isResolvedField) return;
        isResolvedField = true;

        GameEvents.TriggerNoteMiss(targetPlayer, laneID);
        GameEvents.TriggerPlayerMiss(targetPlayer);
        GameEvents.TriggerHitResolved(targetPlayer, HitResult.Miss, 0f);

        Debug.Log(string.Format("[NoteObject] Miss registrado: {0} carril {1} (Beat objetivo: {2:F2}).",
                                targetPlayer, laneID, beatOfThisNote));

        Destroy(gameObject);
    }

    /// <summary>
    /// Evalua y resuelve esta nota contra el tiempo musical actual del RhythmManager.
    /// Determina la calificacion exacta segun el GDD, despacha los eventos en GameEvents
    /// y consume la nota. Permite que tanto PlayerInput como AIController utilicen el mismo flujo.
    /// </summary>
    /// <param name="isElectric">Indica si aplica la reduccion de ventana por Papa Electrica.</param>
    /// <returns>Resultado de la evaluacion (Perfect, Great, Good o Miss).</returns>
    public HitResult EvaluateAndResolve(bool isElectric = false)
    {
        if (isResolvedField || RhythmManager.Instance == null) return HitResult.Miss;

        float timeDiff;
        float accuracy;
        HitResult result = RhythmManager.Instance.EvaluateHit(beatOfThisNote, out timeDiff, out accuracy, isElectric);

        if (result != HitResult.Miss)
        {
            ResolveHit(result, accuracy);
        }
        else
        {
            ResolveMiss();
        }

        return result;
    }

    // -----------------------------------------------------------------------
    // Busqueda de notas activas (API para PlayerInput y AIController)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Busca la nota activa mas cercana al beat actual para un jugador y carril especificos,
    /// dentro de una ventana maxima expresada en BEATS.
    ///
    /// Uso tipico: cuando PlayerInput no tiene acceso al RhythmManager para convertir segundos a beats.
    /// </summary>
    /// <param name="player">Jugador cuyas notas se buscan.</param>
    /// <param name="lane">Indice del carril (0-3).</param>
    /// <param name="currentBeat">Beat actual segun RhythmManager.songPositionInBeats.</param>
    /// <param name="maxBeatTolerance">Ventana de busqueda maxima en beats.</param>
    /// <returns>La NoteObject no resuelta mas cercana, o null si ninguna cumple el criterio.</returns>
    public static NoteObject GetClosestActiveNote(PlayerID player, int lane,
                                                   float currentBeat, float maxBeatTolerance)
    {
        NoteObject closest       = null;
        float      minDifference = float.MaxValue;

        for (int i = 0; i < ActiveNotes.Count; i++)
        {
            NoteObject note = ActiveNotes[i];
            if (note == null || note.isResolvedField)     continue;
            if (note.targetPlayer != player || note.laneID != lane) continue;

            float diff = Mathf.Abs(note.beatOfThisNote - currentBeat);
            if (diff <= maxBeatTolerance && diff < minDifference)
            {
                minDifference = diff;
                closest       = note;
            }
        }

        return closest;
    }

    /// <summary>
    /// Busca la nota activa mas cercana al tiempo DSP actual para un jugador y carril especificos,
    /// dentro de una ventana maxima expresada en SEGUNDOS.
    ///
    /// Uso recomendado desde PlayerInput y AIController porque la ventana de tolerancia de
    /// GameConfig (perfectTolerance, greatTolerance, goodTolerance) esta en segundos, igual
    /// que el resultado de RhythmManager.EvaluateHit. Usar esta sobrecarga evita conversiones
    /// manuales y mantiene la coherencia con el reloj musical.
    /// </summary>
    /// <param name="player">Jugador cuyas notas se buscan.</param>
    /// <param name="lane">Indice del carril (0-3).</param>
    /// <param name="maxToleranceSeconds">Ventana de busqueda maxima en segundos (ej. goodTolerance = 0.15f).</param>
    /// <returns>La NoteObject no resuelta mas cercana dentro de la ventana, o null si ninguna cumple.</returns>
    public static NoteObject GetClosestActiveNoteByTime(PlayerID player, int lane,
                                                         float maxToleranceSeconds)
    {
        if (RhythmManager.Instance == null) return null;

        float secPerBeat    = RhythmManager.Instance.secPerBeat;
        float currentBeat   = RhythmManager.Instance.songPositionInBeats;

        // Convertir la ventana de tolerancia de segundos a beats para comparar con beatOfThisNote.
        float maxBeatTol = (secPerBeat > 0f) ? maxToleranceSeconds / secPerBeat : 0f;

        return GetClosestActiveNote(player, lane, currentBeat, maxBeatTol);
    }

    // -----------------------------------------------------------------------
    // Helpers privados
    // -----------------------------------------------------------------------

    private void ApplyGhostEffect(float progress)
    {
        if (!isGhost || spriteRenderer == null) return;

        Color c = spriteRenderer.color;

        // Rango de ocultamiento: 35%–85% del recorrido visual (tramo medio).
        if (progress >= 0.35f && progress <= 0.85f)
        {
            c.a = 0.05f; // Casi invisible
        }
        else
        {
            c.a = 1.0f;  // Totalmente visible en los extremos
        }

        spriteRenderer.color = c;
    }
}
