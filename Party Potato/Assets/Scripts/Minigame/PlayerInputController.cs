using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerInputController : MonoBehaviour
{
    [Header("Identificacin")]
    public string playerName = "Jugador 1";
    public PlayerID playerID = PlayerID.Player1;
    
    [Header("Mapeo de Teclas")]
    public KeyCode keyLeft = KeyCode.LeftArrow;
    public KeyCode keyDown = KeyCode.DownArrow;
    public KeyCode keyUp = KeyCode.UpArrow;
    public KeyCode keyRight = KeyCode.RightArrow;

    [Header("Visuales de HitZone")]
    [Tooltip("Arrastra los 4 cubos HitZone (0=Left, 1=Down, 2=Up, 3=Right)")]
    public Renderer[] hitZoneRenderers = new Renderer[4];
    public Color perfectColor = Color.cyan;
    public Color goodColor = Color.green;
    public Color badColor = new Color(1f, 0.5f, 0f); // Naranja
    public Color defaultColor = Color.white; 
    public float popScaleMultiplier = 1.3f; // Que tan grande se hace el hitzone al presionarlo
    
    [Header("Palabras de Feedback (Perfect, Miss...)")]
    public GameObject feedbackWordPrefab;
    public Transform feedbackSpawnPoint;
    public Sprite perfectSprite;
    public Sprite goodSprite;
    public Sprite badSprite;
    public Sprite missSprite;

    [Header("Estado del Jugador")]
    public int combo = 0;
    public int score = 0;

    [Header("Puntuacion por Nota")]
    public int scorePerfect = 100;
    public int scoreGood = 50;
    public int scoreBad = 10;
    public int comboBonusMultiplier = 5;


    [Header("Optimizacin (Cero Lag)")]
    [Tooltip("El Spawner que genera las notas para este jugador")]
    public NoteSpawner mySpawner;

    // Controladores de tiempo para apagar el brillo
    private float[] colorResetTimers = new float[4];
    private Vector3[] originalScales = new Vector3[4]; // Guardamos la escala original para el efecto pop

    private void Start()
    {
        // Forzamos a usar el componente del MISMO GameObject para evitar que el Jugador 2 lea al Spawner del Jugador 1
        mySpawner = GetComponent<NoteSpawner>();
        
        for (int i = 0; i < hitZoneRenderers.Length; i++)
        {
            if (hitZoneRenderers[i] != null)
            {
                originalScales[i] = hitZoneRenderers[i].transform.localScale;
            }
        }
    }

    private void Update()
    {
        // 1. Manejar el reseteo del color visual y el tamao (efecto pop) de los HitZones
        for (int i = 0; i < 4; i++)
        {
            if (hitZoneRenderers[i] != null)
            {
                if (colorResetTimers[i] > 0)
                {
                    colorResetTimers[i] -= Time.deltaTime;
                    if (colorResetTimers[i] <= 0)
                    {
                        SetBoxColor(hitZoneRenderers[i], defaultColor);
                    }
                }
                
                // Suavizado del tamao (Lerp) para volver a la escala normal rpidamente
                hitZoneRenderers[i].transform.localScale = Vector3.Lerp(hitZoneRenderers[i].transform.localScale, originalScales[i], Time.deltaTime * 15f);
            }
        }

        // 2. Leer Input
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (IsKeyPressed(keyLeft))  ProcessInputForLane(0);
            if (IsKeyPressed(keyDown))  ProcessInputForLane(1);
            if (IsKeyPressed(keyUp))    ProcessInputForLane(2);
            if (IsKeyPressed(keyRight)) ProcessInputForLane(3);
        }
#else
        if (Input.GetKeyDown(keyLeft))  ProcessInputForLane(0);
        if (Input.GetKeyDown(keyDown))  ProcessInputForLane(1);
        if (Input.GetKeyDown(keyUp))    ProcessInputForLane(2);
        if (Input.GetKeyDown(keyRight)) ProcessInputForLane(3);
#endif
    }

#if ENABLE_INPUT_SYSTEM
    private bool IsKeyPressed(KeyCode code)
    {
        if (Keyboard.current == null) return false;
        switch (code)
        {
            case KeyCode.LeftArrow: return Keyboard.current.leftArrowKey.wasPressedThisFrame;
            case KeyCode.DownArrow: return Keyboard.current.downArrowKey.wasPressedThisFrame;
            case KeyCode.UpArrow: return Keyboard.current.upArrowKey.wasPressedThisFrame;
            case KeyCode.RightArrow: return Keyboard.current.rightArrowKey.wasPressedThisFrame;
            case KeyCode.W: return Keyboard.current.wKey.wasPressedThisFrame;
            case KeyCode.A: return Keyboard.current.aKey.wasPressedThisFrame;
            case KeyCode.S: return Keyboard.current.sKey.wasPressedThisFrame;
            case KeyCode.D: return Keyboard.current.dKey.wasPressedThisFrame;
            default: return false;
        }
    }
#endif

    private void ProcessInputForLane(int pressedLaneID)
    {
        if (mySpawner == null)
        {
            Debug.LogError($"[{playerName}] Falta asignar el NoteSpawner en el Inspector.");
            return;
        }

        NoteObject closestNote = null;
        float closestDistance = float.MaxValue;

        // Bucle sper rpido sin alojar memoria (cero lag)
        foreach (NoteObject note in mySpawner.activeNotes)
        {
            if (note.laneID == pressedLaneID)
            {
                float distance = Mathf.Abs(1f - note.GetCurrentProgress()); 
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestNote = note;
                }
            }
        }

        // Evaluamos solo si la flecha ya est relativamente cerca de la caja
        if (closestNote != null && closestDistance <= 0.3f)
        {
            HitResult result = RhythmManager.Instance.CheckHitResult();
            
            if (result != HitResult.Miss) 
            {
                combo++;
                Debug.Log($"[{playerName}] {result.ToString().ToUpper()}! Combo: {combo}");
                
                // Brillo visual dependiendo de la calidad del golpe
                Color flashColor = result == HitResult.Perfect ? perfectColor : (result == HitResult.Good ? goodColor : badColor);
                FlashHitZone(pressedLaneID, flashColor);
                SpawnFeedbackWord(result);
                
                GameEvents.TriggerHitResolved(playerID, result, closestDistance);
                Destroy(closestNote.gameObject);
            }
            else
            {
                Debug.Log($"[{playerName}] MISS (Cerca de la caja, pero fuera de ritmo). Rompi el combo.");
                combo = 0;
                FlashHitZone(pressedLaneID, Color.red); // Flash ROJO para indicar el fallo
                SpawnFeedbackWord(HitResult.Miss);
                GameEvents.TriggerPlayerMiss(playerID);
            }
        }
        else
        {
            combo = 0; // Spam = Pierde combo
            FlashHitZone(pressedLaneID, Color.gray); // Flash GRIS para indicar spam
            SpawnFeedbackWord(HitResult.Miss);
            GameEvents.TriggerPlayerMiss(playerID);
        }
    }

    private void SetBoxColor(Renderer rend, Color color)
    {
        if (rend == null) return;
        
        if (rend is SpriteRenderer spriteRend)
        {
            spriteRend.color = color;
            return;
        }

        rend.material.color = color;
        if (rend.material.HasProperty("_BaseColor"))
        {
            rend.material.SetColor("_BaseColor", color);
        }
    }

    private void FlashHitZone(int lane, Color color)
    {
        if (lane < hitZoneRenderers.Length)
        {
            if (hitZoneRenderers[lane] != null)
            {
                SetBoxColor(hitZoneRenderers[lane], color);
                colorResetTimers[lane] = 0.15f; // Duracin del destello (150ms)
                
                // Hacemos que el HitZone "salte" visualmente aumentando su escala
                hitZoneRenderers[lane].transform.localScale = originalScales[lane] * popScaleMultiplier;
            }
            else
            {
                Debug.LogWarning($"[Input] Presionaste el carril {lane}, pero no hay ningn Cubo asignado en el Inspector.");
            }
        }
    }

    private void SpawnFeedbackWord(HitResult result)
    {
        if (feedbackWordPrefab == null || feedbackSpawnPoint == null) return;

        GameObject fwObj = Instantiate(feedbackWordPrefab, feedbackSpawnPoint.position, Quaternion.identity);
        FeedbackWord fw = fwObj.GetComponent<FeedbackWord>();
        if (fw != null)
        {
            Sprite spriteToUse = null;
            switch (result)
            {
                case HitResult.Perfect: spriteToUse = perfectSprite; break;
                case HitResult.Good: spriteToUse = goodSprite; break;
                case HitResult.Bad: spriteToUse = badSprite; break;
                case HitResult.Miss: spriteToUse = missSprite; break;
            }
            if (spriteToUse != null)
            {
                fw.SetSprite(spriteToUse);
            }
        }
    }


    public void RegisterPassiveMiss(int laneID)
    {
        combo = 0;
        FlashHitZone(laneID, Color.red);
        SpawnFeedbackWord(HitResult.Miss);
        GameEvents.TriggerPlayerMiss(playerID);
        GameEvents.TriggerNoteMiss(playerID, laneID);
    }

}

