using UnityEngine;

public class Rotator : MonoBehaviour
{
    [Header("Configuracion de Rotacion")]
    [Tooltip("Velocidad de rotacion en el eje Z (positivo = antihorario, negativo = horario)")]
    public float rotationSpeed = -100f;

    private void Update()
    {
        // Rota el objeto en el eje Z de forma constante
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}
