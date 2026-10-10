using UnityEngine;

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
    public Material potatoMaterial; // Para cambiarla a rojo cuando se calienta

    private void Update()
    {
        if (isExploded || RhythmManager.Instance == null) return;

        // Obtenemos el progreso actual en beats (ej: va por el beat 15.34)
        float currentBeatProgress = RhythmManager.Instance.songPositionInBeats;

        // Dividimos por la cantidad de beats que toma cruzar para escalar la velocidad
        float journeyProgress = currentBeatProgress / beatsToCross;

        // Mathf.PingPong hace que el valor rebote suavemente entre 0 y 1 eternamente.
        // 0 = Jugador 1, 1 = Jugador 2
        float pingPongValue = Mathf.PingPong(journeyProgress, 1f);

        // Interpolamos la posición entre el Jugador 1 y el Jugador 2 usando el pingpong
        transform.position = Vector3.Lerp(player1Pos.position, player2Pos.position, pingPongValue);
    }

    /// <summary>
    /// Cambia el estado visual de la papa cuando un jugador falla el ritmo
    /// </summary>
    public void HeatUpPotato()
    {
        if(potatoMaterial != null)
        {
            // La hace un poco más roja cada vez que alguien falla (simulación simple)
            Color currentColor = potatoMaterial.color;
            potatoMaterial.color = Color.Lerp(currentColor, Color.red, 0.4f);
        }
    }

    public void Explode()
    {
        isExploded = true;
        Debug.Log("💥 ¡BOOM! La papa explotó.");
        // TODO: Instanciar sistema de partículas de explosión
    }
}
