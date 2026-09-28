using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance { get; private set; }

    [Header("Sound Library")]
    [SerializeField] private SoundLibrary soundLibrary;

    [Header("Music Settings")]
    [SerializeField, Range(0f, 1f)] private float volume = 1f;
    [SerializeField] private bool playOnStart = false;
    [SerializeField] private MusicID startingSong = MusicID.MainMenu;

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = volume;
    }

    private void Start()
    {
        if (playOnStart)
            PlayMusic(startingSong);
    }

    public void PlayMusic(MusicID musicID)
    {
        if (soundLibrary == null)
        {
            Debug.LogWarning("MusicPlayer has no SoundLibrary assigned.");
            return;
        }

        AudioClip clip = soundLibrary.GetMusic(musicID);

        if (clip == null)
            return;

        // Don't restart the same song unnecessarily.
        if (audioSource.clip == clip && audioSource.isPlaying)
            return;

        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.loop = true;
        audioSource.Play();
    }

    public void StopMusic()
    {
        audioSource.Stop();
    }

    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        audioSource.volume = volume;
    }

    public void PauseMusic()
    {
        audioSource.Pause();
    }

    public void ResumeMusic()
    {
        audioSource.UnPause();
    }
}