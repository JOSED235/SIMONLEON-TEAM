using UnityEngine;

public class NoteObject : MonoBehaviour
{
    [Header("Configuración de Nota")]
    public float beatOfThisNote; // El beat exacto en el que DEBE ser presionado
    public float beatsOnScreen = 2f; // Cuántos beats dura cayendo
    public int laneID; // 0=Izquierda, 1=Abajo, 2=Arriba, 3=Derecha
    
    [HideInInspector] public Vector3 spawnPos;
    [HideInInspector] public Vector3 targetPos; // La HitZone (donde debe golpearse)
    [HideInInspector] public NoteSpawner mySpawner;

    private void Start()
    {
        if (mySpawner != null) mySpawner.activeNotes.Add(this);
    }

    private void OnDestroy()
    {
        if (mySpawner != null) mySpawner.activeNotes.Remove(this);
    }

    public float GetCurrentProgress()
    {
        if (RhythmManager.Instance == null) return 0f;
        float currentBeat = RhythmManager.Instance.songPositionInBeats;
        float spawnBeat = beatOfThisNote - beatsOnScreen;
        return (currentBeat - spawnBeat) / beatsOnScreen;
    }

    private void Update()
    {
        if (RhythmManager.Instance == null) return;

        // Calculamos el progreso (0 = origen, 1 = zona de impacto)
        float progress = GetCurrentProgress();

        // Lerp mueve la nota. Al basarse en el tiempo de la canción, ¡jamás se desincroniza!
        transform.position = Vector3.Lerp(spawnPos, targetPos, progress);

        // Si pasa la zona de impacto y el jugador no la tocó (ej. progress > 1.2)
        if (progress > 1.2f)
        {
            if (mySpawner != null)
            {
                PlayerInputController playerInput = mySpawner.GetComponent<PlayerInputController>();
                if (playerInput != null)
                {
                    playerInput.RegisterPassiveMiss(laneID);
                }
            }
            Destroy(gameObject);
        }

    }
}

