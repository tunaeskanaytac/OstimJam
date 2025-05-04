using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    [Header("---------- Audio Source ----------")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("---------- Chapter Clips ----------")]
    public AudioClip gameplayMusic;

    [Header("---------- SFX Clips ----------")]
    public AudioClip dash;
    public AudioClip enemyDeath;
    public AudioClip jump;
    public AudioClip parry;
    public AudioClip regularShow; // GAMEOVER da

    public static AudioManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        PlayBackgroundMusic(gameplayMusic);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Adjust the music which we want to play in chapters.
        switch (scene.name)
        {
            case "MainMenu":
                SetBackgroundMusic(gameplayMusic);
                break;
            case "BeytepeScene":
                SetBackgroundMusic(gameplayMusic);
                break;
            case "SpecialThanks":
                SetBackgroundMusic(gameplayMusic);
                break;
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void SetBackgroundMusic(AudioClip clip)
    {
        if (musicSource.clip != clip)
        {
            musicSource.clip = clip;
            musicSource.Play();
        }
    }

    public void PlayBackgroundMusic(AudioClip clip)
    {
        if (musicSource.clip != clip)
        {
            musicSource.clip = clip;
            musicSource.Play();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }
}
