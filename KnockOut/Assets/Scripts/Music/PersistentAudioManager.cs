using UnityEngine;

public class PersistentAudioManager : MonoBehaviour
{
    private static PersistentAudioManager instance;
    private AudioSource audioSource;

    //Ponemos la cancion de menu y la de combate
    public AudioClip menuMusic;
    public AudioClip combatMusic;

    void Awake()
    {
        // Comprobar si ya existe una instancia del AudioManager
        if (instance != null && instance != this)
        {
            Destroy(gameObject); // Si ya existe, destruye este nuevo objeto
            return;
        }

        // Hacer que este objeto sea persistente entre escenas
        instance = this;
        DontDestroyOnLoad(gameObject);

        // Configurar el AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    /// <summary>
    /// Pausa la reproducción de la música.
    /// </summary>
    public void Pause()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Pause();
        }
    }

    /// <summary>
    /// Reanuda la reproducción de la música.
    /// </summary>
    public void Play()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
    public void PlayMenuMusic()
    {
        if (audioSource != null)
        {
            audioSource.clip = menuMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
    public void PlayCombatMusic()
    {
        if (audioSource != null)
        {
            audioSource.clip = combatMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
}
