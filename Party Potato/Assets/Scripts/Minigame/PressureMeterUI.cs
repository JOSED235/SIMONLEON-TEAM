using UnityEngine;
using UnityEngine.UI;

public class PressureMeterUI : MonoBehaviour
{
    [Header("Referencias a la Logica")]
    public HotPotatoPressureMeter pressureMeter;

    [Header("UI del Slider Principal")]
    [Tooltip("El Slider de la interfaz gráfica")]
    public Slider uiSlider;
    [Tooltip("La imagen de relleno del slider para cambiarle el color")]
    public Image fillImage;
    [Tooltip("Color del slider cuando está frío")]
    public Color coolColor = Color.yellow;
    [Tooltip("Color del slider cuando está a punto de explotar")]
    public Color hotColor = Color.red;

    [Header("La Papa (Slider Handle)")]
    [Tooltip("La imagen única de la papa (puede ser el Handle del Slider o una imagen aparte)")]
    public Image potatoImage;
    [Tooltip("Arrastra aquí los sprites de potato_iddle (desde el más tranquilo al más enojado)")]
    public Sprite[] potatoStates;
    [Tooltip("Sprite cuando la papa explota (ej. Potato_happy_03)")]
    public Sprite explodedSprite;

    [Header("Papa Flotante (Puntajes)")]
    [Tooltip("La imagen de la papa que saltará de un puntaje a otro")]
    public Image floatingPotatoImage;
    [Tooltip("Punto (Transform vacío o UI) debajo del puntaje del Jugador 1")]
    public RectTransform p1TargetPosition;
    [Tooltip("Punto (Transform vacío o UI) debajo del puntaje del Jugador 2")]
    public RectTransform p2TargetPosition;
    public float teleportSpeed = 25f;

    private void Update()
    {
        if (pressureMeter == null) return;

        // 1. Calcular el valor del Slider y el nivel de presión (0 a 1)
        float normalizedPressure = Mathf.Clamp01(pressureMeter.currentPressure / 100f);
        float targetSliderValue = 0.5f;

        if (pressureMeter.playerHoldingPotato == PlayerID.Player1)
        {
            // Jugador 1 (Izquierda): 100% presión = 0.0, 0% presión = 0.5
            targetSliderValue = 0.5f - (normalizedPressure * 0.5f);
        }
        else
        {
            // Jugador 2 (Derecha): 100% presión = 1.0, 0% presión = 0.5
            targetSliderValue = 0.5f + (normalizedPressure * 0.5f);
        }

        // 2. Actualizar el Slider (Posición y Color)
        if (uiSlider != null)
        {
            uiSlider.value = Mathf.Lerp(uiSlider.value, targetSliderValue, Time.deltaTime * 10f);
        }
        
        if (fillImage != null)
        {
            fillImage.color = Color.Lerp(coolColor, hotColor, normalizedPressure);
        }

        // 3. Determinar el Sprite correcto de la Papa
        Sprite activeSprite = null;

        if (pressureMeter.isExploded)
        {
            activeSprite = explodedSprite;
        }
        else if (potatoStates != null && potatoStates.Length > 0)
        {
            int stateIndex = Mathf.FloorToInt(normalizedPressure * potatoStates.Length);
            if (stateIndex >= potatoStates.Length) stateIndex = potatoStates.Length - 1;
            activeSprite = potatoStates[stateIndex];
        }

        // Determinar escala X para voltearla
        float scaleX = (pressureMeter.playerHoldingPotato == PlayerID.Player1) ? 1f : -1f;

        // 4. Actualizar la Papa del Slider
        if (potatoImage != null)
        {
            if (activeSprite != null) potatoImage.sprite = activeSprite;
            Vector3 currentScale = potatoImage.rectTransform.localScale;
            potatoImage.rectTransform.localScale = new Vector3(scaleX, currentScale.y, currentScale.z);
        }

        // 5. Actualizar la Papa Flotante (Teletransporte / Salto rápido)
        if (floatingPotatoImage != null)
        {
            if (activeSprite != null) 
            {
                floatingPotatoImage.sprite = activeSprite;
            }

            Vector3 floatScale = floatingPotatoImage.rectTransform.localScale;
            floatingPotatoImage.rectTransform.localScale = new Vector3(scaleX, floatScale.y, floatScale.z);

            RectTransform targetTransform = (pressureMeter.playerHoldingPotato == PlayerID.Player1) ? p1TargetPosition : p2TargetPosition;

            if (targetTransform != null)
            {
                // Lerp rápido para crear efecto de salto/teletransporte
                floatingPotatoImage.rectTransform.position = Vector3.Lerp(floatingPotatoImage.rectTransform.position, targetTransform.position, Time.deltaTime * teleportSpeed);
            }
        }
    }
}
