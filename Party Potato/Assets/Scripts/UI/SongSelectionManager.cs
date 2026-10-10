using System.Collections;
using UnityEngine;

namespace BeatPotato.UI
{
    /// <summary>
    /// Gestiona la reproducción de los adelantos musicales en el menú de selección de niveles.
    /// Utiliza un AudioSource para hacer un "Fade In" y "Fade Out" suave de la canción.
    /// </summary>
    public class SongSelectionManager : MonoBehaviour
    {
        public static SongSelectionManager Instance { get; private set; }

        [Header("Audio Settings")]
        [Tooltip("El AudioSource que reproducirá el adelanto. Si se deja vacío, se creará uno automáticamente.")]
        [SerializeField] private AudioSource previewAudioSource;
        
        [Tooltip("Duración en segundos del efecto Fade In / Fade Out")]
        [SerializeField] private float fadeDuration = 0.5f;
        
        [Tooltip("Volumen máximo del adelanto")]
        [Range(0f, 1f)]
        [SerializeField] private float maxVolume = 0.8f;

        private Coroutine fadeCoroutine;
        private AudioClip currentClip;

        private void Awake()
        {
            // Configurar Singleton para que los botones lo puedan encontrar fácilmente
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (previewAudioSource == null)
            {
                previewAudioSource = gameObject.AddComponent<AudioSource>();
            }
            
            previewAudioSource.loop = true; // Que el adelanto se repita
            previewAudioSource.playOnAwake = false;
        }

        /// <summary>
        /// Reproduce el clip desde un segundo específico con un fade in.
        /// </summary>
        public void PlayPreview(AudioClip clip, float startTime = 0f)
        {
            if (clip == null) return;
            
            // Si ya está sonando la misma canción, no la reiniciamos
            if (currentClip == clip && previewAudioSource.isPlaying) return;

            currentClip = clip;

            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeInPreview(clip, startTime));
        }

        /// <summary>
        /// Detiene el adelanto actual con un fade out.
        /// </summary>
        public void StopPreview()
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeOutPreview());
            currentClip = null;
        }

        private IEnumerator FadeInPreview(AudioClip clip, float startTime)
        {
            // Si hay algo sonando actualmente, primero hacemos fade out suave
            if (previewAudioSource.isPlaying)
            {
                yield return FadeOutRoutine();
            }

            previewAudioSource.clip = clip;
            previewAudioSource.time = startTime;
            previewAudioSource.volume = 0f;
            previewAudioSource.Play();

            // Fade In
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                previewAudioSource.volume = Mathf.Lerp(0f, maxVolume, timer / fadeDuration);
                yield return null;
            }
            previewAudioSource.volume = maxVolume;
        }

        private IEnumerator FadeOutPreview()
        {
            yield return FadeOutRoutine();
            previewAudioSource.Stop();
            previewAudioSource.clip = null;
        }

        private IEnumerator FadeOutRoutine()
        {
            float startVolume = previewAudioSource.volume;
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                previewAudioSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeDuration);
                yield return null;
            }
            previewAudioSource.volume = 0f;
        }
    }
}
