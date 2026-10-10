using UnityEngine;

public class RhythmManager : MonoBehaviour
{
    public static RhythmManager Instance;

    [Header("Configuración de la Pista")]
    [Tooltip("Billie Jean tiene un tempo de aprox 117 BPM")]
    public float songBpm = 117f; 
    
    [Tooltip("Margen de error en segundos para acertar el golpe (ej. 0.15 = 150ms)")]
    public float inputTolerance = 0.15f; 

    [Header("Datos en Tiempo Real")]
    public float secPerBeat;
    public float songPosition;
    public float songPositionInBeats;
    public float dspSongTime;

    [Header("Referencias")]
    public AudioSource musicSource;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Calcular cuántos segundos dura un "Beat" (golpe)
        secPerBeat = 60f / songBpm;
        
        // dspTime es el reloj interno del motor de audio de Unity. 
        // Es muchísimo más preciso que Time.time para juegos de ritmo.
        dspSongTime = (float)AudioSettings.dspTime;
        
        if (musicSource != null)
        {
            musicSource.Play();
        }
    }

    private int lastReportedBeat = 0;

    private void Update()
    {
        // Tiempo actual de la canción usando el reloj de alta precisión
        songPosition = (float)(AudioSettings.dspTime - dspSongTime);
        
        // Calcular en qué beat (pulso) exacto nos encontramos
        songPositionInBeats = songPosition / secPerBeat;

        // Imprimir información cada vez que haya un pulso exacto (Beat)
        int currentBeat = Mathf.FloorToInt(songPositionInBeats);
        if (currentBeat > lastReportedBeat)
        {
            lastReportedBeat = currentBeat;
            Debug.Log($"[RhythmManager] 🎵 BEAT: {currentBeat} | Canción: {songPosition:F2}s | Reloj DSP: {AudioSettings.dspTime:F2}");
        }
    }

    /// <summary>
    /// Verifica si el jugador pulsó el botón a tiempo con el ritmo de la canción.
    /// Devuelve un valor de 0 a 1 (1 = perfecto, 0 = fallo catastrófico).
    /// </summary>
    public float CheckHitAccuracy()
    {
        // Buscar el beat entero más cercano al momento de pulsar
        float closestBeat = Mathf.Round(songPositionInBeats);
        
        // Calcular la diferencia en segundos entre nuestro pulso y el beat perfecto
        float timeDifference = Mathf.Abs(songPositionInBeats - closestBeat) * secPerBeat;

        // Si la diferencia está dentro de la tolerancia, es un acierto
        if (timeDifference <= inputTolerance)
        {
            // Retorna un porcentaje de precisión. 1.0f es el centro exacto.
            float accuracy = 1f - (timeDifference / inputTolerance);
            Debug.Log($"[RhythmManager] ¡ACIERTO RÍTMICO! Precisión: {accuracy * 100:F0}%");
            return accuracy;
        }
        
        Debug.Log("[RhythmManager] ¡FALLO! (Out of Sync)");
        return 0f;
    }
}
