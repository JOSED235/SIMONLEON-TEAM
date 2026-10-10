using UnityEngine;
using UnityEditor;

public class SceneRebuilder : EditorWindow
{
    [MenuItem("Tools/Rebuild Beat Potato Scene")]
    public static void RebuildScene()
    {
        // 1. Rhythm Manager
        GameObject rhythmObj = new GameObject("RhythmManager");
        RhythmManager rm = rhythmObj.AddComponent<RhythmManager>();
        rm.musicSource = rhythmObj.AddComponent<AudioSource>();
        rm.songBpm = 117f;

        // Load Assets
        GameObject notePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Note_Prefab.prefab");
        Material hitMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Mat_HitZone.mat");
        
        if (notePrefab == null) Debug.LogWarning("No se encontró Note_Prefab en Assets/Prefab/");
        if (hitMat == null) Debug.LogWarning("No se encontró Mat_HitZone en Assets/");

        // 2. Player 1
        BuildPlayer("Player 1", -6f, KeyCode.LeftArrow, KeyCode.DownArrow, KeyCode.UpArrow, KeyCode.RightArrow, notePrefab, hitMat);
        
        // 3. Player 2
        BuildPlayer("Player 2", 3f, KeyCode.A, KeyCode.S, KeyCode.W, KeyCode.D, notePrefab, hitMat);

        // 4. Potato Bomb
        GameObject bombObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bombObj.name = "Potato Bomb";
        BombController bomb = bombObj.AddComponent<BombController>();
        bomb.player1Pos = GameObject.Find("Player 1").transform;
        bomb.player2Pos = GameObject.Find("Player 2").transform;
        
        Debug.Log("¡Escena multijugador reconstruida con éxito!");
    }

    private static void BuildPlayer(string name, float startX, KeyCode left, KeyCode down, KeyCode up, KeyCode right, GameObject notePrefab, Material hitMat)
    {
        GameObject playerObj = new GameObject(name);
        
        NoteSpawner spawner = playerObj.AddComponent<NoteSpawner>();
        spawner.defaultNotePrefab = notePrefab;
        spawner.beatsOnScreen = 2f;
        
        PlayerInputController input = playerObj.AddComponent<PlayerInputController>();
        input.playerName = name;
        input.keyLeft = left;
        input.keyDown = down;
        input.keyUp = up;
        input.keyRight = right;
        input.mySpawner = spawner;

        SerializedObject soSpawner = new SerializedObject(spawner);
        SerializedProperty lanesProp = soSpawner.FindProperty("lanes");
        lanesProp.arraySize = 4;

        SerializedObject soInput = new SerializedObject(input);
        SerializedProperty hitZoneRenderersProp = soInput.FindProperty("hitZoneRenderers");
        hitZoneRenderersProp.arraySize = 4;

        string[] dirs = { "Left", "Down", "Up", "Right" };
        
        for (int i = 0; i < 4; i++)
        {
            float posX = startX + (i * 1.5f);
            
            // HitZone
            GameObject hitZone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hitZone.name = "HitZone_" + dirs[i];
            hitZone.transform.SetParent(playerObj.transform);
            hitZone.transform.position = new Vector3(posX, -3f, 0f);
            if (hitMat != null) hitZone.GetComponent<Renderer>().sharedMaterial = hitMat;

            // SpawnPoint
            GameObject spawnPoint = new GameObject("SpawnPoint_" + dirs[i]);
            spawnPoint.transform.SetParent(playerObj.transform);
            spawnPoint.transform.position = new Vector3(posX, 8f, 0f);

            // Set Spawner lanes
            SerializedProperty laneStruct = lanesProp.GetArrayElementAtIndex(i);
            laneStruct.FindPropertyRelative("laneName").stringValue = dirs[i];
            laneStruct.FindPropertyRelative("spawnPoint").objectReferenceValue = spawnPoint.transform;
            laneStruct.FindPropertyRelative("hitZone").objectReferenceValue = hitZone.transform;

            // Set Input renderers
            hitZoneRenderersProp.GetArrayElementAtIndex(i).objectReferenceValue = hitZone.GetComponent<Renderer>();
        }
        soSpawner.ApplyModifiedProperties();
        soInput.ApplyModifiedProperties();
    }
}
