using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;

public partial class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [SerializeField] private AudioMixer myMixer;
    [Header("Audio Source")]
    [SerializeField] AudioSource musicSource;

    [Header("Music Clips")]
    public AudioClip menuMusic;
    public AudioClip levelMusic;

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
    }

    private void Start()
    {
        float savedVol = PlayerPrefs.GetFloat("SavedMusicVolume", 0.75f);
        SetVolume(savedVol);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            PlayMusic(menuMusic);
        }
        else if (scene.name == "SampleScene")
        {
            PlayMusic(levelMusic);
        }
    }

    void PlayMusic(AudioClip clip)
    {
        // Zmieñ tylko, jeœli gra coœ innego (¿eby nie puszczaæ od nowa tej samej piosenki)
        if (musicSource.clip == clip) return;

        musicSource.clip = clip;
        musicSource.Play();
    }

    public void SetVolume(float sliderValue)
    {
        // Steruje g³oœnoœci¹ miksera (wp³ywa na wszystko, co do niego podpiête)
        float volume = Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20;
        myMixer.SetFloat("MusicVol", volume);

        // Opcjonalnie: Zapisz ustawienie, by oba menu wiedzia³y o zmianie
        PlayerPrefs.SetFloat("SavedMusicVolume", sliderValue);
    }
}
