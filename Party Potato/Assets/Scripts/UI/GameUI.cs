using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gestiona el HUD principal del juego (Puntajes, Rondas, Combos, Mensajes centrales).
/// Escucha el bus de eventos GameEvents para actualizar la interfaz automáticamente.
/// </summary>
public class GameUI : MonoBehaviour
{
    [Header("Puntajes y Rondas")]
    public Text p1ScoreText;
    public Text p2ScoreText;
    public Text roundText;
    public Text centerMessageText;

    [Header("Combos")]
    public Text p1ComboText;
    public Text p2ComboText;

    private void OnEnable()
    {
        // Suscripción al bus de eventos
        GameEvents.OnRoundStarted += HandleRoundStarted;
        GameEvents.OnRoundEnded += HandleRoundEnded;
        GameEvents.OnMatchEnded += HandleMatchEnded;
        GameEvents.OnStreakUpdated += HandleStreakUpdated;
        GameEvents.OnStreakReset += HandleStreakReset;
        GameEvents.OnMatchReset += HandleMatchReset;
    }

    private void OnDisable()
    {
        // Limpieza de suscripciones
        GameEvents.OnRoundStarted -= HandleRoundStarted;
        GameEvents.OnRoundEnded -= HandleRoundEnded;
        GameEvents.OnMatchEnded -= HandleMatchEnded;
        GameEvents.OnStreakUpdated -= HandleStreakUpdated;
        GameEvents.OnStreakReset -= HandleStreakReset;
        GameEvents.OnMatchReset -= HandleMatchReset;
    }

    private void Start()
    {
        // Estado inicial de la UI
        UpdateScore(0, 0);
        if (centerMessageText != null) centerMessageText.text = "¡PREPARADOS!";
        ResetCombos();
    }

    private void HandleRoundStarted(int roundNumber)
    {
        if (roundText != null) roundText.text = $"RONDA {roundNumber}";
        if (centerMessageText != null) centerMessageText.text = ""; // Limpiar mensaje central
        ResetCombos();
    }

    private void HandleRoundEnded(PlayerID winner, int p1Score, int p2Score)
    {
        UpdateScore(p1Score, p2Score);

        if (centerMessageText != null)
        {
            centerMessageText.text = winner == PlayerID.None ? "¡EMPATE!" : $"¡RONDA PARA {winner}!";
        }
    }

    private void HandleMatchEnded(PlayerID matchWinner)
    {
        if (centerMessageText != null)
        {
            centerMessageText.text = $"¡VICTORIA DE {matchWinner}!";
        }
    }

    private void HandleStreakUpdated(PlayerID player, int streak, bool powerReady)
    {
        Text targetText = (player == PlayerID.Player1) ? p1ComboText : p2ComboText;
        if (targetText != null)
        {
            if (streak >= 2) // Solo mostrar combos a partir de 2 aciertos
            {
                targetText.text = $"{streak} COMBO!" + (powerReady ? "\n¡PODER LISTO!" : "");
                targetText.color = powerReady ? Color.yellow : Color.white;
            }
            else
            {
                targetText.text = "";
            }
        }
    }

    private void HandleStreakReset(PlayerID player)
    {
        Text targetText = (player == PlayerID.Player1) ? p1ComboText : p2ComboText;
        if (targetText != null) targetText.text = "";
    }

    private void HandleMatchReset()
    {
        UpdateScore(0, 0);
        if (roundText != null) roundText.text = "RONDA 1";
        if (centerMessageText != null) centerMessageText.text = "¡PREPARADOS!";
        ResetCombos();
    }

    private void UpdateScore(int p1, int p2)
    {
        if (p1ScoreText != null) p1ScoreText.text = p1.ToString();
        if (p2ScoreText != null) p2ScoreText.text = p2.ToString();
    }

    private void ResetCombos()
    {
        if (p1ComboText != null) p1ComboText.text = "";
        if (p2ComboText != null) p2ComboText.text = "";
    }
}
