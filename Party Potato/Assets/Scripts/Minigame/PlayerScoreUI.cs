using UnityEngine;
using TMPro; // Requerido para TextMeshPro

public class PlayerScoreUI : MonoBehaviour
{
    [Header("Referencias")]
    public PlayerInputController playerInput;
    public TextMeshProUGUI scoreText;

    [Header("Configuracin")]
    public string playerPrefix = "Player1: ";
    public string scoreFormat = "000000"; // Para que el puntaje siempre tenga 6 dgitos (ej. 000150)

    private void Start()
    {
        if (scoreText == null)
            scoreText = GetComponent<TextMeshProUGUI>();
            
        UpdateUI();
    }

    private void Update()
    {
        // Podramos usar Eventos para optimizar, pero Update est bien para la UI de Jam.
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (playerInput != null && scoreText != null)
        {
            scoreText.text = playerPrefix + playerInput.score.ToString(scoreFormat);
        }
    }
}
