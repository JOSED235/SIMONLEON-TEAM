using UnityEngine;

/// <summary>
/// Cambia los sprites del jugador en base a sus acciones (Idle, Direcciones, Error).
/// Se comunica escuchando el componente PlayerInputController y los eventos de GameEvents.
/// </summary>
public class PlayerAnimator : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("El SpriteRenderer que muestra al personaje (la 'caja' que quieres cambiar).")]
    public SpriteRenderer spriteRenderer;
    public PlayerInputController playerInput;

    [Header("Sprites del Personaje")]
    public Sprite idleSprite;
    public Sprite leftSprite;
    public Sprite downSprite;
    public Sprite upSprite;
    public Sprite rightSprite;
    public Sprite errorSprite;

    [Header("Configuración")]
    [Tooltip("Tiempo que se mantiene el sprite de una acción antes de volver a Idle.")]
    public float actionDuration = 0.3f;

    private float timer = 0f;
    private bool isError = false;

    private void Start()
    {
        // Forzamos a usar los componentes correctos aunque el usuario haya duplicado el objeto
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // El PlayerInputController suele estar en el objeto padre (ej. Player2), mientras que este Animator estǭ en el hijo (P2_Pos)
        playerInput = GetComponentInParent<PlayerInputController>();

        ShowIdle();
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerMiss += HandlePlayerMiss;
        GameEvents.OnNoteMiss += HandleNoteMiss;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerMiss -= HandlePlayerMiss;
        GameEvents.OnNoteMiss -= HandleNoteMiss;
    }

    private void Update()
    {
        // 1. Manejar el temporizador para volver a Idle
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                isError = false;
                ShowIdle();
            }
        }

        // 2. Leer las teclas pulsadas directamente para reaccionar al instante
        // Solo cambiamos el sprite si no estamos en un frame de "Error" bloqueante
        if (!isError && playerInput != null)
        {
#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                if (IsKeyPressed(playerInput.keyLeft)) ShowDirection(leftSprite);
                else if (IsKeyPressed(playerInput.keyDown)) ShowDirection(downSprite);
                else if (IsKeyPressed(playerInput.keyUp)) ShowDirection(upSprite);
                else if (IsKeyPressed(playerInput.keyRight)) ShowDirection(rightSprite);
            }
#else
            if (Input.GetKeyDown(playerInput.keyLeft)) ShowDirection(leftSprite);
            else if (Input.GetKeyDown(playerInput.keyDown)) ShowDirection(downSprite);
            else if (Input.GetKeyDown(playerInput.keyUp)) ShowDirection(upSprite);
            else if (Input.GetKeyDown(playerInput.keyRight)) ShowDirection(rightSprite);
#endif
        }
    }

    public void ShowDirection(Sprite directionSprite)
    {
        if (spriteRenderer != null && directionSprite != null)
        {
            spriteRenderer.sprite = directionSprite;
            timer = actionDuration;
        }
    }

    public void ShowError()
    {
        if (spriteRenderer != null && errorSprite != null)
        {
            spriteRenderer.sprite = errorSprite;
            isError = true;
            timer = actionDuration * 1.5f; // El error dura un poquito más
        }
    }

    public void ShowIdle()
    {
        if (spriteRenderer != null && idleSprite != null)
        {
            spriteRenderer.sprite = idleSprite;
        }
    }

    private void HandlePlayerMiss(PlayerID id)
    {
        if (playerInput != null && id == playerInput.playerID)
        {
            ShowError();
        }
    }

    private void HandleNoteMiss(PlayerID id, int lane)
    {
        if (playerInput != null && id == playerInput.playerID)
        {
            ShowError();
        }
    }

#if ENABLE_INPUT_SYSTEM
    private bool IsKeyPressed(KeyCode code)
    {
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        if (keyboard == null) return false;
        switch (code)
        {
            case KeyCode.LeftArrow: return keyboard.leftArrowKey.wasPressedThisFrame;
            case KeyCode.DownArrow: return keyboard.downArrowKey.wasPressedThisFrame;
            case KeyCode.UpArrow: return keyboard.upArrowKey.wasPressedThisFrame;
            case KeyCode.RightArrow: return keyboard.rightArrowKey.wasPressedThisFrame;
            case KeyCode.W: return keyboard.wKey.wasPressedThisFrame;
            case KeyCode.A: return keyboard.aKey.wasPressedThisFrame;
            case KeyCode.S: return keyboard.sKey.wasPressedThisFrame;
            case KeyCode.D: return keyboard.dKey.wasPressedThisFrame;
            default: return false;
        }
    }
#endif
}
