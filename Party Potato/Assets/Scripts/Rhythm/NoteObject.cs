using UnityEngine;

public class NoteObject : MonoBehaviour
{
    [Header("Configuración de Nota")]
    public float beatOfThisNote; // El beat exacto en el que DEBE ser presionado
    public float beatsOnScreen = 2f; // Cuántos beats dura cayendo
    public int laneID; // 0=Izquierda, 1=Abajo, 2=Arriba, 3=Derecha
    
    [HideInInspector] public Vector3 spawnPos;
    [HideInInspector] public Vector3 targetPos; // La HitZone (donde debe golpearse)

    private void Update()
    {
        if (RhythmManager.Instance == null) return;

        // Obtenemos en qué beat va la canción
        float currentBeat = RhythmManager.Instance.songPositionInBeats;
        
        // Calculamos el progreso. 
        // 0 = acaba de aparecer. 
        // 1 = está EXACTAMENTE en la zona de impacto (HitZone).
        float spawnBeat = beatOfThisNote - beatsOnScreen;
        float progress = (currentBeat - spawnBeat) / beatsOnScreen;

        // Lerp mueve la nota. Al basarse en el tiempo de la canción, ¡jamás se desincroniza!
        transform.position = Vector3.Lerp(spawnPos, targetPos, progress);

        // Si pasa la zona de impacto y el jugador no la tocó (ej. progress > 1.2)
        if (progress > 1.2f)
        {
            Debug.Log($"¡Nota del Beat {beatOfThisNote} perdida! (Miss)");
            // TODO: Notificar al sistema que se rompió la racha/combo
            Destroy(gameObject);
        }
    }
}
