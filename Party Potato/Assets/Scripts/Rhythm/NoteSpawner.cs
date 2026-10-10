using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NoteLane
{
    public string laneName;       
    public Transform spawnPoint;  
    public Transform hitZone;     
    public GameObject specificNotePrefab; 
}

public class NoteSpawner : MonoBehaviour
{
    [Header("Configuración General")]
    public GameObject defaultNotePrefab; 

    [Header("Carriles (Lanes)")]
    public NoteLane[] lanes; 

    [Header("Configuración de Caída")]
    public float beatsOnScreen = 2f; 
    
    // OPTIMIZACIÓN: Lista centralizada de notas vivas para no usar FindObjectsOfType
    [HideInInspector] public List<NoteObject> activeNotes = new List<NoteObject>();
    
    private int nextBeatToSpawn = 1;

    private void Update()
    {
        if (RhythmManager.Instance == null || lanes == null || lanes.Length < 2) return;

        float currentBeat = RhythmManager.Instance.songPositionInBeats;

        if (currentBeat >= (nextBeatToSpawn - beatsOnScreen))
        {
            // TRUCO MULTIJUGADOR: Usamos una semilla matemática basada en el Beat.
            // Si el Jugador 1 y el Jugador 2 tienen Spawners distintos, AMBOS generarán
            // la flecha exactamente en el mismo carril al mismo tiempo de forma "aleatoria".
            Random.InitState(nextBeatToSpawn * 12345);

            bool isDoubleNote = (nextBeatToSpawn % 4 == 0);

            if (isDoubleNote)
            {
                int lane1 = Random.Range(0, lanes.Length);
                int lane2 = Random.Range(0, lanes.Length);
                
                while (lane2 == lane1) 
                {
                    lane2 = Random.Range(0, lanes.Length);
                }
                
                SpawnNoteForBeat(nextBeatToSpawn, lane1);
                SpawnNoteForBeat(nextBeatToSpawn, lane2);
            }
            else
            {
                int randomLaneIndex = Random.Range(0, lanes.Length);
                SpawnNoteForBeat(nextBeatToSpawn, randomLaneIndex);
            }
            
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
        noteObj.laneID = laneIndex; 
        noteObj.spawnPos = lane.spawnPoint.position;
        noteObj.targetPos = lane.hitZone.position;
        noteObj.mySpawner = this; // Se inscribe en el Spawner local
    }
}
