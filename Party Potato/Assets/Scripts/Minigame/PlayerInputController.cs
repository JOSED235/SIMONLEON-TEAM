using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerInputController : MonoBehaviour
{
    [Header("Identificación")]
    public string playerName = "Jugador 1";
    
    [Header("Mapeo de Teclas (Solo Sistema Viejo)")]
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
    
    [Header("Estado del Jugador")]
    public int combo = 0;

    [Header("Optimización (Cero Lag)")]
    [Tooltip("El Spawner que genera las notas para este jugador")]
    public NoteSpawner mySpawner;

    // Controladores de tiempo para apagar el brillo
    private float[] colorResetTimers = new float[4];

    private void Update()
    {
        // 1. Manejar el reseteo del color visual de los HitZones
        for (int i = 0; i < 4; i++)
        {
            if (colorResetTimers[i] > 0)
            {
                colorResetTimers[i] -= Time.deltaTime;
                if (colorResetTimers[i] <= 0 && i < hitZoneRenderers.Length && hitZoneRenderers[i] != null)
                {
                    SetBoxColor(hitZoneRenderers[i], defaultColor);
                }
            }
        }

        // 2. Leer Input
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.leftArrowKey.wasPressedThisFrame)  ProcessInputForLane(0);
            if (Keyboard.current.downArrowKey.wasPressedThisFrame)  ProcessInputForLane(1);
            if (Keyboard.current.upArrowKey.wasPressedThisFrame)    ProcessInputForLane(2);
            if (Keyboard.current.rightArrowKey.wasPressedThisFrame) ProcessInputForLane(3);
        }
#else
        if (Input.GetKeyDown(keyLeft))  ProcessInputForLane(0);
        if (Input.GetKeyDown(keyDown))  ProcessInputForLane(1);
        if (Input.GetKeyDown(keyUp))    ProcessInputForLane(2);
        if (Input.GetKeyDown(keyRight)) ProcessInputForLane(3);
#endif
    }

    private void ProcessInputForLane(int pressedLaneID)
    {
        if (mySpawner == null)
        {
            Debug.LogError($"[{playerName}] Falta asignar el NoteSpawner en el Inspector.");
            return;
        }

        NoteObject closestNote = null;
        float closestDistance = float.MaxValue;

        // Bucle súper rápido sin alojar memoria (cero lag)
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

        // Evaluamos solo si la flecha ya está relativamente cerca de la caja (ej. a menos del 30% de distancia)
        // Esto evita que presiones la tecla y el sistema intente golpear una flecha que apenas va naciendo arriba.
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
                
                Destroy(closestNote.gameObject);
            }
            else
            {
                Debug.Log($"[{playerName}] MISS (Cerca de la caja, pero fuera de ritmo). Rompió el combo.");
                combo = 0;
                FlashHitZone(pressedLaneID, Color.red); // Flash ROJO para indicar el fallo
            }
        }
        else
        {
            combo = 0; // Spam = Pierde combo
            FlashHitZone(pressedLaneID, Color.gray); // Flash GRIS para indicar que presionaste sin haber notas cerca
        }
    }

    private void SetBoxColor(Renderer rend, Color color)
    {
        if (rend == null) return;
        
        // Compatible con pipeline Clásico
        rend.material.color = color;
        
        // Compatible con Universal Render Pipeline (URP) y HDRP
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
                colorResetTimers[lane] = 0.15f; // Duración del destello (150ms)
            }
            else
            {
                Debug.LogWarning($"[Input] Presionaste el carril {lane}, pero no hay ningún Cubo asignado en el Inspector (Hit Zone Renderers Element {lane}).");
            }
        }
    }
}
