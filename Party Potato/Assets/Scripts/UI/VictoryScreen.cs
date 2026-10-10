using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla la pantalla final de victoria.
/// Aparece cuando el MatchManager dispara OnMatchEnded y permite pedir revancha.
/// </summary>
public class VictoryScreen : MonoBehaviour
{
    [Header("Elementos de UI")]
    [Tooltip("El panel oscuro que cubre la pantalla al final del juego")]
    public GameObject victoryPanel;
    
    [Tooltip("Texto donde se anunciará al ganador")]
    public Text winnerText;
    
    [Tooltip("Botón para volver a jugar")]
    public Button rematchButton;

    private void Start()
    {
        // Asegurarnos de que el panel esté apagado al arrancar
        if (victoryPanel != null) victoryPanel.SetActive(false);
        
        // Conectar el clic del botón a nuestro método
        if (rematchButton != null) rematchButton.onClick.AddListener(OnRematchClicked);
    }

    private void OnEnable()
    {
        GameEvents.OnMatchEnded += HandleMatchEnded;
        GameEvents.OnMatchReset += HandleMatchReset;
    }

    private void OnDisable()
    {
        GameEvents.OnMatchEnded -= HandleMatchEnded;
        GameEvents.OnMatchReset -= HandleMatchReset;
    }

    private void HandleMatchEnded(PlayerID matchWinner)
    {
        if (victoryPanel != null) victoryPanel.SetActive(true);
        if (winnerText != null) winnerText.text = $"¡VICTORIA PARA {matchWinner}!";
    }

    private void HandleMatchReset()
    {
        // Si alguien pide revancha, escondemos el panel
        if (victoryPanel != null) victoryPanel.SetActive(false);
    }

    private void OnRematchClicked()
    {
        // Disparar el evento global de reinicio (MatchManager lo escuchará)
        GameEvents.TriggerMatchReset();
    }
}
