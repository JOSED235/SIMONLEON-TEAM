using UnityEngine;

/// <summary>
/// Controla la representación visual de la papa en la escena.
/// Oscila rítmicamente entre los jugadores y reacciona al calor global y explosiones
/// escuchando el bus de GameEvents.
/// </summary>
public class BombController : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Posición del Jugador 1 (Izquierda)")]
    public Transform player1Pos;

    [Tooltip("Posición del Jugador 2 (Derecha)")]
    public Transform player2Pos;

    [Header("Configuración de Ritmo")]
    [Tooltip("Cuántos golpes de ritmo (beats) le toma a la papa ir de un lado al otro")]
    public float beatsToCross = 2f; 

    [Header("Estado de la Papa")]
    public bool isExploded = false;
    public Material potatoMaterial; 

    private Color originalColor = Color.yellow;

    private void Awake()
    {
        if (potatoMaterial != null)
        {
            originalColor = potatoMaterial.color;
        }
    }

    private void OnEnable()
    {
        // Suscribirse a los eventos del ciclo de la papa
        GameEvents.OnHeatUpdated += HandleHeatUpdated;
        GameEvents.OnPotatoExploded += HandleExploded;
        GameEvents.OnRoundStarted += HandleRoundStarted;
    }

    private void OnDisable()
    {
        GameEvents.OnHeatUpdated -= HandleHeatUpdated;
        GameEvents.OnPotatoExploded -= HandleExploded;
        GameEvents.OnRoundStarted -= HandleRoundStarted;
    }

    private void Update()
    {
        if (isExploded || RhythmManager.Instance == null || !RhythmManager.Instance.isPlaying) return;

        // Obtenemos el progreso actual en beats (ej: va por el beat 15.34)
        float currentBeatProgress = RhythmManager.Instance.songPositionInBeats;

        // Dividimos por la cantidad de beats que toma cruzar para escalar la velocidad
        float journeyProgress = currentBeatProgress / beatsToCross;

        // Mathf.PingPong hace que el valor rebote suavemente entre 0 y 1 eternamente.
        // 0 = Jugador 1, 1 = Jugador 2
        float pingPongValue = Mathf.PingPong(journeyProgress, 1f);

        // Interpolamos la posición entre el Jugador 1 y el Jugador 2
        if (player1Pos != null && player2Pos != null)
        {
            transform.position = Vector3.Lerp(player1Pos.position, player2Pos.position, pingPongValue);
        }
    }

    private void HandleHeatUpdated(float heatNormalized)
    {
        if (potatoMaterial != null && !isExploded)
        {
            // Cambia gradualmente al color rojo según el nivel global de calor (0.0 a 1.0)
            potatoMaterial.color = Color.Lerp(originalColor, Color.red, heatNormalized);
        }
    }

    private void HandleExploded(PlayerID loser)
    {
        isExploded = true;
        Debug.Log($"[BombController] 💥 ¡BOOM! La papa explotó en el campo de {loser}.");
        // TODO: Instanciar sistema de partículas de explosión aquí
    }

    private void HandleRoundStarted(int roundNumber)
    {
        isExploded = false;
        if (potatoMaterial != null)
        {
            potatoMaterial.color = originalColor; // Restaurar color
        }
    }
}
