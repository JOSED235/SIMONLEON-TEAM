using UnityEngine;

public class FeedbackWord : MonoBehaviour
{
    [Header("Configuracin de Animacin")]
    public float floatSpeed = 2f;
    public float lifetime = 0.6f;
    public float fadeSpeed = 3f;
    public float initialPopScale = 1.2f;

    private SpriteRenderer spriteRenderer;
    private Color currentColor;
    private float timer = 0f;
    private Vector3 targetScale;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            currentColor = spriteRenderer.color;
        }
        
        // Empezamos un poco mǭs pequeos para hacer el efecto de "Pop"
        targetScale = transform.localScale;
        transform.localScale = targetScale * 0.5f;
    }

    public void SetSprite(Sprite newSprite)
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
            
        spriteRenderer.sprite = newSprite;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        // 1. Movimiento hacia arriba
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        // 2. Efecto "Pop" de escala (crece rpido, luego se estabiliza)
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * 15f);

        // 3. Efecto de difuminado (Fade Out) en la segunda mitad de su vida
        if (timer > lifetime * 0.5f && spriteRenderer != null)
        {
            currentColor.a -= fadeSpeed * Time.deltaTime;
            spriteRenderer.color = currentColor;
        }

        // 4. Destruccin
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
