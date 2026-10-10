using UnityEngine;

/// <summary>
/// Gestiona la presión de la Papa Caliente (0 a 100%).
/// </summary>
public class HotPotatoPressureMeter : MonoBehaviour
{
    [Header("Estado de la Papa")]
    public float currentPressure = 50f;
    public PlayerID playerHoldingPotato = PlayerID.Player1;
    public bool isExploded = false;

    [Header("Configuración de Presión")]
    public float basePressureIncreaseRate = 10f; // Sube 10% por segundo
    public float overheatMultiplier = 1.0f; // Aumenta con cada explosión

    [Header("Reducción (Con Papa)")]
    public float perfectReduction = 15f;
    public float goodReduction = 8f;
    
    [Header("Aumento (Con Papa)")]
    public float badIncrease = 5f;
    public float missIncrease = 20f;

    [Header("Ataque (Sin Papa)")]
    public float perfectAttack = 10f;
    public float goodAttack = 5f;

    private void OnEnable()
    {
        GameEvents.OnHitResolved += HandleHitResolved;
        GameEvents.OnPlayerMiss += HandlePlayerMiss;
        GameEvents.OnNoteMiss += HandleNoteMiss;
    }

    private void OnDisable()
    {
        GameEvents.OnHitResolved -= HandleHitResolved;
        GameEvents.OnPlayerMiss -= HandlePlayerMiss;
        GameEvents.OnNoteMiss -= HandleNoteMiss;
    }

    private void Update()
    {
        if (isExploded) return;

        // Aumenta la presión gradualmente con el tiempo
        currentPressure += basePressureIncreaseRate * overheatMultiplier * Time.deltaTime;
        
        CheckLimits();
    }

    private void HandleHitResolved(PlayerID player, HitResult result, float accuracy)
    {
        if (isExploded) return;

        if (player == playerHoldingPotato)
        {
            // Defensa: Reducir presión propia o penalizarse
            switch (result)
            {
                case HitResult.Perfect: currentPressure -= perfectReduction; break;
                case HitResult.Good: currentPressure -= goodReduction; break;
                case HitResult.Bad: currentPressure += badIncrease; break;
                case HitResult.Miss: currentPressure += missIncrease; break;
            }
        }
        else
        {
            // Ataque: Subir la presión del rival
            switch (result)
            {
                case HitResult.Perfect: currentPressure += perfectAttack; break;
                case HitResult.Good: currentPressure += goodAttack; break;
            }
        }

        CheckLimits();
    }

    private void HandlePlayerMiss(PlayerID player) { ApplyMiss(player); }
    private void HandleNoteMiss(PlayerID player, int laneID) { ApplyMiss(player); }

    private void ApplyMiss(PlayerID player)
    {
        if (isExploded) return;
        
        if (player == playerHoldingPotato)
        {
            currentPressure += missIncrease;
            CheckLimits();
        }
    }

    private void CheckLimits()
    {
        // Limitar valores temporalmente
        currentPressure = Mathf.Clamp(currentPressure, -1f, 101f);

        // Traspaso de la Papa (Llega a 0 o menos)
        if (currentPressure <= 0f)
        {
            TransferPotato();
        }
        // Explosión (Llega a 100 o más)
        else if (currentPressure >= 100f)
        {
            ExplodePotato();
        }
    }

    private void TransferPotato()
    {
        playerHoldingPotato = (playerHoldingPotato == PlayerID.Player1) ? PlayerID.Player2 : PlayerID.Player1;
        currentPressure = 50f;
        
        Debug.Log($"[Hot Potato] ¡TRASPASO! La papa ahora la tiene {playerHoldingPotato}.");
        
        // Emitir evento si la UI o modelo 3D necesita saber
        // GameEvents.TriggerPotatoLaunched(...)
    }

    private void ExplodePotato()
    {
        isExploded = true;
        currentPressure = 100f;
        
        Debug.Log($"[Hot Potato] ¡EXPLOSIÓN! El {playerHoldingPotato} perdió.");
        GameEvents.TriggerPotatoExploded(playerHoldingPotato);

        // Handicap: Reiniciar después de un momento con Overheat
        Invoke(nameof(RestartAfterExplosion), 3f);
    }

    private void RestartAfterExplosion()
    {
        overheatMultiplier += 0.5f; // Handicap (Sube 50% más rápido)
        currentPressure = 50f;
        isExploded = false;
        
        Debug.Log($"[Hot Potato] Reinicio con Handicap. Overheat Multiplier: {overheatMultiplier}x");
    }
}
