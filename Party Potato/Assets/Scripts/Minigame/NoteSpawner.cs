using UnityEngine;

[System.Serializable]
public class NoteLane
{
    public string laneName;       // Ej: "Left", "Down", "Up", "Right"
    public Transform spawnPoint;  // Punto de origen arriba
    public Transform hitZone;     // Zona de impacto abajo
    
    // Opcional: Si quieres que la flecha izquierda sea de un color/forma distinta a la derecha.
    // Si lo dejas vacío, usará el notePrefab general.
    public GameObject specificNotePrefab; 
}

public class NoteSpawner : MonoBehaviour
{
    [Header("Configuración General")]
    public GameObject defaultNotePrefab; // Prefab base si el carril no tiene uno específico

    [Header("Carriles (Lanes)")]
    [Tooltip("Configura aquí tus 4 carriles (0=Izq, 1=Abajo, 2=Arriba, 3=Der)")]
    public NoteLane[] lanes; 

    [Header("Configuración de Caída")]
    public float beatsOnScreen = 2f; 
    
    private int nextBeatToSpawn = 1;

    private void Update()
    {
        if (RhythmManager.Instance == null || lanes == null || lanes.Length == 0) return;

        float currentBeat = RhythmManager.Instance.songPositionInBeats;

        // Generar notas anticipadamente
        if (currentBeat >= (nextBeatToSpawn - beatsOnScreen))
        {
            // Elegir un carril al azar para generar la nota
            int randomLaneIndex = Random.Range(0, lanes.Length);
            
            SpawnNoteForBeat(nextBeatToSpawn, randomLaneIndex);
            
            nextBeatToSpawn++; 
        }
    }

    private void SpawnNoteForBeat(int beatTarget, int laneIndex)
    {
        NoteLane lane = lanes[laneIndex];
        
        GameObject prefabToUse = lane.specificNotePrefab != null ? lane.specificNotePrefab : defaultNotePrefab;
        if (prefabToUse == null) return;

        GameObject newNote = Instantiate(prefabToUse, lane.spawnPoint.position, Quaternion.identity, transform);
        NoteObject noteObj = newNote.GetComponent<NoteObject>();
        
        noteObj.beatOfThisNote = beatTarget;
        noteObj.beatsOnScreen = beatsOnScreen;
        noteObj.laneID = laneIndex; // 0, 1, 2 o 3
        
        noteObj.spawnPos = lane.spawnPoint.position;
        noteObj.targetPos = lane.hitZone.position;
    }
}
