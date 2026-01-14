using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;   // Solo música
    public AudioSource sfxSource;     // Solo efectos

    [Header("Music Tracks")]
    public AudioClip menuMusic;
    public AudioClip gameplayMusic;
    public AudioClip bossMusic;

    private float musicVolume;
    private float sfxVolume;

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Cargar volúmenes antes de configurar las fuentes
        LoadAudioSettings();

        // Configurar música
        if (musicSource != null)
        {
            musicSource.loop = true;
            musicSource.volume = musicVolume;
        }

        // Configurar SFX
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }

        PlayMenuMusic();
    }

    // ==============================
    // VOLUMEN MÚSICA
    // ==============================
    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);

        if (musicSource != null)
            musicSource.volume = musicVolume;

        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.Save();

        Debug.Log($"🎵 Music Volume set to: {musicVolume}");
    }

    // ==============================
    // VOLUMEN SFX
    // ==============================
    public void SetSFXVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);

        if (sfxSource != null)
            sfxSource.volume = sfxVolume;

        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();

        Debug.Log($"🎮 SFX Volume set to: {sfxVolume}");
    }

    // ==============================
    // CARGAR CONFIGURACIÓN
    // ==============================
    private void LoadAudioSettings()
    {
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
    }

    // ==============================
    // REPRODUCCIÓN DE MÚSICA
    // ==============================
    public void PlayMenuMusic()     => PlayMusic(menuMusic, "Menú");
    public void PlayGameplayMusic() => PlayMusic(gameplayMusic, "Gameplay");
    public void PlayBossMusic()     => PlayMusic(bossMusic, "Boss");

    private void PlayMusic(AudioClip clip, string name)
    {
        if (clip == null || musicSource == null)
        {
            Debug.LogError($"❌ No se puede reproducir música de {name}");
            return;
        }

        if (musicSource.clip != clip)
        {
            musicSource.clip = clip;
            musicSource.volume = musicVolume;
            musicSource.Play();
            Debug.Log($"🎵 Reproduciendo música de {name}");
        }
    }

    // ==============================
    // CONTROL DE MÚSICA
    // ==============================
    public void StopMusic()  => musicSource?.Stop();
    public void PauseMusic() => musicSource?.Pause();
    public void ResumeMusic()
    {
        if (musicSource != null && musicSource.clip != null)
            musicSource.Play();
    }

    // ==============================
    // SFX
    // ==============================
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
            sfxSource.PlayOneShot(clip);
    }

    // ==============================
    // GETTERS
    // ==============================
    public float GetMusicVolume() => musicVolume;
    public float GetSFXVolume()   => sfxVolume;
    public AudioSource GetMusicSource() => musicSource;

    [ContextMenu("Debug Audio State")]
    public void DebugAudioState()
    {
        Debug.Log($"🎵 Música: {(musicSource.isPlaying ? "PLAYING" : "STOPPED")} - Clip: {musicSource.clip?.name}");
        Debug.Log($"🔊 Volúmenes -> Música: {musicVolume}, SFX: {sfxVolume}");
    }
}
