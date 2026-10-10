using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BeatPotato.UI
{
    /// <summary>
    /// Se adjunta al Botón de cada Nivel/Canción en la interfaz.
    /// Detecta cuando el usuario pone el cursor encima o navega con el teclado
    /// para avisar al SongSelectionManager que reproduzca el adelanto.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class SongSelectionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [Header("Song Preview Settings")]
        [Tooltip("El archivo de audio de la canción para este nivel")]
        [SerializeField] private AudioClip previewClip;
        
        [Tooltip("Tiempo en segundos donde empieza el adelanto (ej. ir directo al estribillo)")]
        [SerializeField] private float previewStartTime = 0f;

        [Header("Scene Transition")]
        [Tooltip("Nombre de la escena a cargar al presionar el botón (ej. 'Level_1')")]
        [SerializeField] private string levelSceneName;
        
        // Referencia al SceneLoader si ya existe en el proyecto (opcional)
        private SceneLoader sceneLoader;
        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnButtonClicked);
            
            // Intenta encontrar el SceneLoader en la escena
            sceneLoader = FindObjectOfType<SceneLoader>();
        }

        // --- INTERACCION CON MOUSE ---
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            // Cuando el mouse entra, reproducir adelanto
            PlayPreview();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // Cuando el mouse sale, detener adelanto
            StopPreview();
        }

        // --- INTERACCION CON TECLADO / MANDO ---

        public void OnSelect(BaseEventData eventData)
        {
            // Cuando se selecciona con el teclado (flechas/WASD), reproducir adelanto
            PlayPreview();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            // Cuando se deselecciona con el teclado, detener adelanto
            StopPreview();
        }

        // --- METODOS INTERNOS ---

        private void PlayPreview()
        {
            if (SongSelectionManager.Instance != null && previewClip != null)
            {
                SongSelectionManager.Instance.PlayPreview(previewClip, previewStartTime);
            }
        }

        private void StopPreview()
        {
            if (SongSelectionManager.Instance != null)
            {
                SongSelectionManager.Instance.StopPreview();
            }
        }

        private void OnButtonClicked()
        {
            // Aquí puedes agregar lógica si necesitan guardar la canción elegida en un GameConfig.
            // Por ahora, simplemente carga la escena usando SceneLoader o SceneManager.
            
            if (sceneLoader != null && !string.IsNullOrEmpty(levelSceneName))
            {
                sceneLoader.LoadSceneByName(levelSceneName);
            }
            else if (!string.IsNullOrEmpty(levelSceneName))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(levelSceneName);
            }
            else
            {
                Debug.LogWarning($"El botón {gameObject.name} no tiene configurado el nombre de la escena a cargar.");
            }
        }
    }
}
