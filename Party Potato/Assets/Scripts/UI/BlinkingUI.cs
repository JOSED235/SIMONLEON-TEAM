using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Image))]
public class BlinkingUI : MonoBehaviour
{
    [Header("Configuracion de Tiempos")]
    public float minWaitTime = 1f;
    public float maxWaitTime = 3f;
    public float visibleDuration = 1.5f;
    public float fadeSpeed = 3f;

    [Header("Movimiento")]
    [Tooltip("Hacia dónde y qué tan rápido se moverá mientras es visible")]
    public Vector2 driftDirection = new Vector2(0, 30f);

    private Image img;
    private Color color;
    private RectTransform rectTransform;
    private Vector2 startPosition;

    private void Awake()
    {
        img = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        color = img.color;
        
        // Guardar la posición inicial para no perderla
        startPosition = rectTransform.anchoredPosition;

        // Iniciar invisible
        color.a = 0f;
        img.color = color;
    }

    private void Start()
    {
        StartCoroutine(BlinkRoutine());
    }

    private void Update()
    {
        // Solo moverlo si se está viendo (aunque sea un poco)
        if (color.a > 0f)
        {
            rectTransform.anchoredPosition += driftDirection * Time.deltaTime;
        }
    }

    private IEnumerator BlinkRoutine()
    {
        while (true)
        {
            // 1. Esperar un tiempo aleatorio estando invisible
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(waitTime);

            // Reiniciar su posición antes de aparecer
            rectTransform.anchoredPosition = startPosition;

            // 2. Aparecer suavemente (Fade In)
            while (color.a < 1f)
            {
                color.a += fadeSpeed * Time.deltaTime;
                if (color.a > 1f) color.a = 1f;
                img.color = color;
                yield return null;
            }

            // 3. Quedarse visible
            yield return new WaitForSeconds(visibleDuration);

            // 4. Desaparecer suavemente (Fade Out)
            while (color.a > 0f)
            {
                color.a -= fadeSpeed * Time.deltaTime;
                if (color.a < 0f) color.a = 0f;
                img.color = color;
                yield return null;
            }
        }
    }
}
