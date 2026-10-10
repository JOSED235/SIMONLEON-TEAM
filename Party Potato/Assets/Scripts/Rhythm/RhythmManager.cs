using UnityEngine;

public class RhythmManager : MonoBehaviour
{
    public static RhythmManager Instance;

    [Header("Configuración de la Pista")]
    public float songBpm = 117f; 
    
    [Header("Ventanas de Precisión (en Segundos)")]
    public float perfectWindow = 0.045f; // 45ms
    public float goodWindow = 0.090f;    // 90ms
    public float badWindow = 0.135f;     // 135ms

    [Header("Datos en Tiempo Real")]
    public float secPerBeat;
    public float songPosition;
    public float songPositionInBeats;
    public float dspSongTime;

    [Header("Referencias")]
    public AudioSource musicSource;

    private int lastReportedBeat = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        secPerBeat = 60f / songBpm;
        dspSongTime = (float)AudioSettings.dspTime;
        
        if (musicSource != null)
        {
            musicSource.Play();
        }
    }

    private void Update()
    {
        songPosition = (float)(AudioSettings.dspTime - dspSongTime);
        songPositionInBeats = songPosition / secPerBeat;

        // Imprimir información cada vez que haya un pulso exacto (Beat)
        int currentBeat = Mathf.FloorToInt(songPositionInBeats);
        if (currentBeat > lastReportedBeat)
        {
            lastReportedBeat = currentBeat;
            // Quitamos el Debug.Log excesivo de la consola para evitar que ralentice el juego (lag de editor)
            // Si quieres ver los beats, descomenta la siguiente línea:
            // Debug.Log($"[Metrónomo] Beat: {currentBeat}");
        }
    }

    /// <summary>
    /// Evalúa la precisión matemática del impacto según ventanas de tiempo realistas.
    /// </summary>
    public HitResult CheckHitResult()
    {
        float closestBeat = Mathf.Round(songPositionInBeats);
        float timeDifference = Mathf.Abs(songPositionInBeats - closestBeat) * secPerBeat;

        if (timeDifference <= perfectWindow) return HitResult.Perfect;
        if (timeDifference <= goodWindow) return HitResult.Good;
        if (timeDifference <= badWindow) return HitResult.Bad;
        
        return HitResult.Miss;
    }
}
