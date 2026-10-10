using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerInputController : MonoBehaviour
{
    [Header("Legacy - Use PlayerInput.cs instead")]
    public string playerName = "Jugador 1";
    public PlayerID playerID = PlayerID.Player1;
    
    private void Update()
    {
        // Legacy class. Replaced by Rhythm/PlayerInput.cs
    }
}
