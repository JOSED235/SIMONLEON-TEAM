using System;
using UnityEngine;

/// <summary>
/// Fuente de tiempo maestro de Beat Potato (Papa Beat).
/// Administra el reloj musical de alta precision basado exclusivamente en AudioSettings.dspTime.
/// Expone conversiones exactas entre compas/beat y tiempo DSP, dispara pulsos rítmicos en el bus de eventos
/// y evalua la precision temporal de las notas de forma independiente al framerate visual.
/// </summary>
public class RhythmManager : MonoBehaviour
{
    public static RhythmManager Instance { get; set; }

    [Header("Configuracion de Pista")]
    [Tooltip("Configuracion de balance global (tolerancias de impacto, duraciones).")]
    public GameConfig gameConfig;

    [Tooltip("Tempo de la cancion en pulsos por minuto (BPM).")]
    public float songBpm = 117f;

    [Tooltip("Pulsos (beats) por cada compas musical (4 en compas de 4/4).")]
    public int beatsPerMeasure = 4;

    [Tooltip("Desfase de calibracion de audio en segundos (latencia).")]
    public float songOffset = 0f;

    [Tooltip("Iniciar reproduccion automaticamente en Start si la fuente de audio esta asignada.")]
    public bool autoPlayOnStart = false;

    [Tooltip("Anticipacion en segundos al programar el inicio del audio (buffer para PlayScheduled).")]
    public float startDelaySeconds = 0.5f;

    [Header("Referencias de Audio")]
    [Tooltip("Fuente de audio que reproduce la pista musical del juego.")]
    public AudioSource musicSource;

    [Header("Estado y Reloj DSP en Tiempo Real")]
    [Tooltip("Duracion en segundos de un pulso/beat (60 / BPM).")]
    [SerializeField] private float secPerBeatField;

    [Tooltip("Instante DSP exacto en el que inicio la pista musical.")]
    [SerializeField] private double dspSongStartTimeField;

    [Tooltip("Posicion actual de reproduccion en segundos.")]
    [SerializeField] private float songPositionField;

    [Tooltip("Posicion actual en pulsos/beats continuos.")]
    [SerializeField] private float songPositionInBeatsField;

    [Tooltip("Indice del pulso entero actual.")]
    [SerializeField] private int currentBeatIndexField;

    [Tooltip("Indice del compas entero actual.")]
    [SerializeField] private int currentMeasureIndexField;

    [SerializeField] private bool isPlayingField;
    [SerializeField] private bool isPausedField;

    // Propiedades publicas de consulta
    public float secPerBeat { get { return secPerBeatField; } }
    public double dspSongStartTime { get { return dspSongStartTimeField; } }
    public float songPosition { get { return songPositionField; } }
    public float songPositionInBeats { get { return songPositionInBeatsField; } }
    public int currentBeatIndex { get { return currentBeatIndexField; } }
    public int currentMeasureIndex { get { return currentMeasureIndexField; } }
    public bool isPlaying { get { return isPlayingField; } }
    public bool isPaused { get { return isPausedField; } }

    // Retrocompatibilidad con campos publicos anteriores
    public float dspSongTime { get { return (float)dspSongStartTimeField; } }

    private int lastReportedBeat = -1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        RecalculateBeatDuration();

        if (gameConfig == null)
        {
            gameConfig = ScriptableObject.CreateInstance<GameConfig>();
        }
    }

    private void Start()
    {
        if (autoPlayOnStart && musicSource != null)
        {
            StartTrack();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (!isPlayingField || isPausedField) return;

        // Calculo de tiempo continuo con el reloj del subsistema de audio
        double currentDspTime = AudioSettings.dspTime;
        songPositionField = (float)(currentDspTime - dspSongStartTimeField - songOffset);
        songPositionInBeatsField = songPositionField / secPerBeatField;

        // Notificacion de beats y compases enteros (evitando saltos por frames lentos)
        int currentFloorBeat = Mathf.FloorToInt(songPositionInBeatsField);
        if (currentFloorBeat > lastReportedBeat)
        {
            for (int b = lastReportedBeat + 1; b <= currentFloorBeat; b++)
            {
                if (b >= 0)
                {
                    currentBeatIndexField = b;
                    GameEvents.TriggerBeat(b);

                    if (b % beatsPerMeasure == 0)
                    {
                        currentMeasureIndexField = b / beatsPerMeasure;
                        GameEvents.TriggerMeasure(currentMeasureIndexField);
                    }
                }
            }

            lastReportedBeat = currentFloorBeat;
        }
    }

    #region Control de Pista

    /// <summary>
    /// Recalcula la duracion de cada beat en segundos segun el tempo actual.
    /// </summary>
    public void RecalculateBeatDuration()
    {
        if (songBpm <= 0f) songBpm = 117f;
        secPerBeatField = 60f / songBpm;
    }

    /// <summary>
    /// Inicia la pista de audio de forma coordinada con PlayScheduled.
    /// Si targetDspTime es menor o igual a cero, se programa con una breve anticipacion (startDelaySeconds).
    /// </summary>
    public void StartTrack(double targetDspTime = 0.0)
    {
        RecalculateBeatDuration();

        double scheduledTime = targetDspTime;
        if (scheduledTime <= 0.0)
        {
            scheduledTime = AudioSettings.dspTime + Mathf.Max(0.05f, startDelaySeconds);
        }

        dspSongStartTimeField = scheduledTime;
        isPlayingField = true;
        isPausedField = false;
        lastReportedBeat = -1;
        songPositionField = (float)(AudioSettings.dspTime - dspSongStartTimeField - songOffset);
        songPositionInBeatsField = songPositionField / secPerBeatField;

        if (musicSource != null)
        {
            musicSource.Stop();
            musicSource.PlayScheduled(scheduledTime);
        }

        Debug.Log(string.Format("[RhythmManager] Pista programada a DSP {0:F3}. Tempo: {1} BPM (secPerBeat: {2:F3}s).", scheduledTime, songBpm, secPerBeatField));
    }

    /// <summary>
    /// Detiene la reproduccion y resetea las posiciones de tiempo y compases.
    /// </summary>
    public void StopTrack()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }

        isPlayingField = false;
        isPausedField = false;
        songPositionField = 0f;
        songPositionInBeatsField = 0f;
        currentBeatIndexField = 0;
        currentMeasureIndexField = 0;
        lastReportedBeat = -1;
    }

    /// <summary>
    /// Pausa la reproduccion de audio.
    /// </summary>
    public void PauseTrack()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Pause();
        }

        isPausedField = true;
    }

    /// <summary>
    /// Reanuda la pista ajustando el desfase temporal al reloj actual de DSP.
    /// </summary>
    public void ResumeTrack()
    {
        if (!isPausedField) return;

        double currentDsp = AudioSettings.dspTime;
        dspSongStartTimeField = currentDsp - (songPositionField + songOffset);
        isPausedField = false;

        if (musicSource != null)
        {
            musicSource.UnPause();
        }
    }

    #endregion

    #region Conversiones Bidireccionales Beat <-> DSP

    /// <summary>
    /// Convierte un indice o fraccion de beat al instante exacto en dspTime en que ocurre.
    /// </summary>
    public double BeatToDspTime(float beat)
    {
        return dspSongStartTimeField + songOffset + (beat * secPerBeatField);
    }

    /// <summary>
    /// Convierte un instante en dspTime al pulso o fraccion de beat correspondiente.
    /// </summary>
    public float DspTimeToBeat(double dspTime)
    {
        if (secPerBeatField <= 0f) return 0f;
        return (float)((dspTime - dspSongStartTimeField - songOffset) / secPerBeatField);
    }

    /// <summary>
    /// Convierte un beat en segundos relativos al inicio de la cancion.
    /// </summary>
    public float BeatToSongPosition(float beat)
    {
        return beat * secPerBeatField;
    }

    /// <summary>
    /// Convierte segundos relativos de la cancion al beat correspondiente.
    /// </summary>
    public float SongPositionToBeat(float songPos)
    {
        if (secPerBeatField <= 0f) return 0f;
        return songPos / secPerBeatField;
    }

    /// <summary>
    /// Devuelve el reloj interno actual de AudioSettings.dspTime.
    /// </summary>
    public double GetCurrentDspTime()
    {
        return AudioSettings.dspTime;
    }

    #endregion

    #region Evaluacion de Precision Ritmica

    /// <summary>
    /// Evalua la precision temporal de un impacto contra el beat objetivo de una nota especifica.
    /// Determina la calificacion (Perfect, Great, Good o Miss) y la precision normalizada (0.0 a 1.0).
    /// </summary>
    /// <param name="targetBeat">Beat exacto en el que debia ser golpeada la nota.</param>
    /// <param name="timeDifference">Diferencia temporal absoluta en segundos respecto al instante ideal.</param>
    /// <param name="accuracyNormalized">Precision porcentual (1.0 = centro exacto, 0.0 = borde de Good o Miss).</param>
    /// <param name="isElectric">Indica si aplica la ventana reducida por el sabotaje Papa Electrica.</param>
    /// <returns>Resultado del impacto segun las ventanas de GameConfig.</returns>
    public HitResult EvaluateHit(float targetBeat, out float timeDifference, out float accuracyNormalized, bool isElectric = false)
    {
        // Calculo de desfase temporal contra el tiempo objetivo de la nota
        float beatDiff = Mathf.Abs(songPositionInBeatsField - targetBeat);
        timeDifference = beatDiff * secPerBeatField;

        float perfTol = (isElectric && gameConfig != null) ? gameConfig.electricPerfectTolerance : (gameConfig != null ? gameConfig.perfectTolerance : 0.05f);
        float greatTol = gameConfig != null ? gameConfig.greatTolerance : 0.10f;
        float goodTol = gameConfig != null ? gameConfig.goodTolerance : 0.15f;

        if (timeDifference <= perfTol)
        {
            accuracyNormalized = Mathf.Clamp01(1f - (timeDifference / perfTol));
            return HitResult.Perfect;
        }

        if (timeDifference <= greatTol)
        {
            accuracyNormalized = Mathf.Clamp01(1f - (timeDifference / greatTol));
            return HitResult.Great;
        }

        if (timeDifference <= goodTol)
        {
            accuracyNormalized = Mathf.Clamp01(1f - (timeDifference / goodTol));
            return HitResult.Good;
        }

        accuracyNormalized = 0f;
        return HitResult.Miss;
    }

    /// <summary>
    /// Metodo retrocompatible que evalua el pulso contra el beat entero mas proximo.
    /// </summary>
    public float CheckHitAccuracy()
    {
        float closestBeat = Mathf.Round(songPositionInBeatsField);
        float diff;
        float accuracy;
        HitResult result = EvaluateHit(closestBeat, out diff, out accuracy);

        if (result != HitResult.Miss)
        {
            Debug.Log(string.Format("[RhythmManager] Acierto generico ({0}). Precision: {1:F0}%", result, accuracy * 100f));
            return accuracy;
        }

        Debug.Log("[RhythmManager] Fallo generico fuera de tiempo.");
        return 0f;
    }

    #endregion
}
