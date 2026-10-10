using UnityEngine;
using UnityEngine.UI;

public class PressureMeterUI : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("El script que controla la lógica de la presión")]
    public HotPotatoPressureMeter pressureMeter;
    
    [Tooltip("El Slider de la interfaz gráfica")]
    public Slider uiSlider;
    
    [Tooltip("Color del slider cuando está frío")]
    public Color coolColor = Color.yellow;
    
    [Tooltip("Color del slider cuando está a punto de explotar")]
    public Color hotColor = Color.red;

    [Header("Opcional")]
    [Tooltip("La imagen de relleno del slider para cambiarle el color")]
    public Image fillImage;

    private void Update()
    {
        if (pressureMeter == null || uiSlider == null) return;

        // Convertimos el 0-100% de la presión a un valor entre 0 y 1 para el Slider
        // 0.0 = Jugador 1 (Izquierda) al borde de la explosión
        // 0.5 = Centro (Traspaso de la papa)
        // 1.0 = Jugador 2 (Derecha) al borde de la explosión
        
        float normalizedPressure = pressureMeter.currentPressure / 100f;
        float targetSliderValue = 0.5f;

        if (pressureMeter.playerHoldingPotato == PlayerID.Player1)
        {
            // Si el Jugador 1 tiene la papa, 100% de presión = 0.0 (Izquierda extrema)
            // 0% de presión = 0.5 (Centro)
            targetSliderValue = 0.5f - (normalizedPressure * 0.5f);
        }
        else
        {
            // Si el Jugador 2 tiene la papa, 100% de presión = 1.0 (Derecha extrema)
            // 0% de presión = 0.5 (Centro)
            targetSliderValue = 0.5f + (normalizedPressure * 0.5f);
        }
        
        // Suavizamos el movimiento del slider para que se vea más fluido
        uiSlider.value = Mathf.Lerp(uiSlider.value, targetSliderValue, Time.deltaTime * 10f);

        // Cambiamos el color de amarillo a rojo conforme aumenta la presión real (independiente de la posición)
        if (fillImage != null)
        {
            fillImage.color = Color.Lerp(coolColor, hotColor, normalizedPressure);
        }
    }
}
