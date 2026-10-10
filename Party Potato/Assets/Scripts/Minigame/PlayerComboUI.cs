using UnityEngine;
using TMPro;

public class PlayerComboUI : MonoBehaviour
{
    [Header("Referencias")]
    public PlayerInputController playerInput;
    public TextMeshProUGUI comboText;

    [Header("Configuracion")]
    public string comboPrefix = "Combo x";
    public bool hideWhenZero = true; // Oculta el texto si el combo se rompe (0)
    public float popScaleMultiplier = 1.3f; // Que tan grande se hace al aumentar
    public float popSpeed = 15f; // Que tan rapido vuelve a su tamaño normal
    public float showMissDuration = 1f; // Cuanto tiempo se muestra el texto en rojo antes de ocultarse

    private int lastCombo = 0;
    private Vector3 originalScale;
    private float hideTimer = 0f;

    private void Start()
    {
        if (comboText == null)
            comboText = GetComponent<TextMeshProUGUI>();
            
        if (comboText != null)
            originalScale = comboText.transform.localScale;
            
        UpdateUI();
    }

    private void OnEnable()
    {
        GameEvents.OnHitResolved += HandleHit;
        GameEvents.OnPlayerMiss += HandleMiss;
    }

    private void OnDisable()
    {
        GameEvents.OnHitResolved -= HandleHit;
        GameEvents.OnPlayerMiss -= HandleMiss;
    }

    private void HandleHit(PlayerID pID, HitResult result, float accuracy)
    {
        if (playerInput != null && playerInput.playerID == pID && comboText != null)
        {
            if (result == HitResult.Perfect) comboText.color = playerInput.perfectColor;
            else if (result == HitResult.Good) comboText.color = playerInput.goodColor;
            else if (result == HitResult.Bad) comboText.color = playerInput.badColor;
        }
    }

    private void HandleMiss(PlayerID pID)
    {
        if (playerInput != null && playerInput.playerID == pID && comboText != null)
        {
            comboText.color = Color.red;
            hideTimer = showMissDuration; // Activar el temporizador para mostrar el fallo
        }
    }

    private void Update()
    {
        if (playerInput != null && comboText != null)
        {
            // Detectar si el combo cambió en este frame
            if (playerInput.combo != lastCombo)
            {
                bool comboAumento = playerInput.combo > lastCombo;
                lastCombo = playerInput.combo;
                
                // Efecto Pop siempre que cambie (sea aumento o fallo)
                comboText.transform.localScale = originalScale * popScaleMultiplier;
                
                UpdateUI();

                // Si aumentó el combo o volvió a uno mayor a 0, nos aseguramos que el alfa esté en 1
                if (lastCombo > 0)
                {
                    Color c = comboText.color;
                    c.a = 1f;
                    comboText.color = c;
                }
            }

            // Ocultar texto gradualmente o al terminar el temporizador
            if (lastCombo == 0 && hideWhenZero)
            {
                if (hideTimer > 0)
                {
                    hideTimer -= Time.deltaTime;
                    
                    // Efecto de desvanecimiento (Fade Out) en la segunda mitad del temporizador
                    if (hideTimer < showMissDuration * 0.5f)
                    {
                        Color c = comboText.color;
                        c.a = Mathf.Lerp(0f, 1f, hideTimer / (showMissDuration * 0.5f));
                        comboText.color = c;
                    }

                    if (hideTimer <= 0)
                    {
                        comboText.text = "";
                        Color c = comboText.color;
                        c.a = 1f; // Restaurar para la próxima vez
                        comboText.color = c;
                    }
                }
            }

            // Retornar suavemente a la escala original (Lerp)
            comboText.transform.localScale = Vector3.Lerp(comboText.transform.localScale, originalScale, Time.deltaTime * popSpeed);
        }
    }

    private void UpdateUI()
    {
        if (comboText == null) return;

        if (lastCombo == 0)
        {
            if (hideWhenZero)
            {
                if (hideTimer > 0)
                {
                    comboText.text = "Miss!";
                }
                else
                {
                    comboText.text = "";
                }
            }
            else
            {
                comboText.text = "Combo 0";
            }
        }
        else
        {
            comboText.text = comboPrefix + lastCombo.ToString();
        }
    }
}
