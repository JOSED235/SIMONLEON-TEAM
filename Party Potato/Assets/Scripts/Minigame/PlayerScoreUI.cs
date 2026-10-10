using UnityEngine;
using TMPro; // Requerido para TextMeshPro

public class PlayerScoreUI : MonoBehaviour
{
    [Header("Referencias")]
    public PlayerInputController playerInput;
    public TextMeshProUGUI scoreText;

    [Header("Configuración")]
    public string playerPrefix = "Player1: ";
    public string scoreFormat = "000000"; // Para que el puntaje siempre tenga 6 dígitos (ej. 000150)

    private Color originalColor;
    private Vector3 originalPosition;
    private float shakeTimer = 0f;

    private void Start()
    {
        if (scoreText == null)
            scoreText = GetComponent<TextMeshProUGUI>();
            
        if (scoreText != null)
        {
            originalColor = scoreText.color;
            originalPosition = scoreText.rectTransform.anchoredPosition;
        }
            
        UpdateUI();
    }

    private void OnEnable()
    {
        GameEvents.OnPotatoExploded += HandleExplosion;
    }

    private void OnDisable()
    {
        GameEvents.OnPotatoExploded -= HandleExplosion;
    }

    private void HandleExplosion(PlayerID loser)
    {
        if (playerInput != null && playerInput.playerID == loser)
        {
            shakeTimer = 1.0f; // 1 segundo de shake y color rojo
        }
    }

    private void Update()
    {
        // Actualizamos el número de puntos
        UpdateUI();

        // Efecto de Explosión: Shake y Color Rojo
        if (shakeTimer > 0 && scoreText != null)
        {
            shakeTimer -= Time.deltaTime;
            
            // Agitación (Shake)
            float shakeAmount = shakeTimer * 10f;
            scoreText.rectTransform.anchoredPosition = originalPosition + new Vector3(Random.Range(-shakeAmount, shakeAmount), Random.Range(-shakeAmount, shakeAmount), 0);

            // Interpolamos de rojo de vuelta a su color original
            scoreText.color = Color.Lerp(originalColor, Color.red, shakeTimer);
        }
        else if (scoreText != null)
        {
            // Restaurar estado base para evitar que se quede desfasado
            scoreText.rectTransform.anchoredPosition = originalPosition;
            scoreText.color = originalColor;
        }
    }

    private void UpdateUI()
    {
        if (playerInput != null && scoreText != null)
        {
            scoreText.text = playerPrefix + playerInput.score.ToString(scoreFormat);
        }
    }
}

