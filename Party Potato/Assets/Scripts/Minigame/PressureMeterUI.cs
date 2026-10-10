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

        // Convertimos el 0-100% de la presión a 0-1 para el Slider
        float normalizedPressure = pressureMeter.currentPressure / 100f;
        
        // Suavizamos el movimiento del slider para que se vea más fluido
        uiSlider.value = Mathf.Lerp(uiSlider.value, normalizedPressure, Time.deltaTime * 10f);

        // Cambiamos el color de amarillo a rojo conforme aumenta la presión
        if (fillImage != null)
        {
            fillImage.color = Color.Lerp(coolColor, hotColor, normalizedPressure);
        }
    }
}
