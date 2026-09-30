using UnityEngine;

// Plays voice over clips in order, one per dialogue sentence.
// Lives on the NPC so the audio follows the NPC's position.
[RequireComponent(typeof(AudioSource))]
public class DialogueVoiceOver : MonoBehaviour
{
    [Header("Voice Overs (in dialogue order)")]
    [SerializeField] private AudioClip[] voiceClips;

    [Header("Sound Settings")]
    [SerializeField, Range(0f, 1f)] private float volume = 1f;
    [SerializeField, Range(0f, 1f)] private float spatialBlend = 1f;
    [SerializeField] private float minDistance = 5f;
    [SerializeField] private float maxDistance = 60f;

    [Header("Debug")]
    [SerializeField] private bool logDebug = true;

    private AudioSource source;
    private int currentIndex = -1;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.volume = volume;
        source.spatialBlend = spatialBlend;
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        source.minDistance = minDistance;
        source.maxDistance = maxDistance;
    }

    // Stops the current voice over (if any) and plays the next one in the list
    public void PlayNext()
    {
        Stop();

        currentIndex++;

        if (currentIndex >= voiceClips.Length)
        {
            Debug.LogWarning($"{name}: No voice over clip for sentence {currentIndex}. Array only has {voiceClips.Length} clips.");
            return;
        }

        AudioClip clip = voiceClips[currentIndex];

        if (clip == null)
        {
            Debug.LogWarning($"{name}: Voice clip slot {currentIndex} is empty.");
            return;
        }

        source.clip = clip;
        source.Play();

        if (logDebug)
        {
            Debug.Log($"{name}: Playing voice clip {currentIndex} ({clip.name}), isPlaying = {source.isPlaying}");
        }
    }

    // Stops the current voice over without changing the position in the list
    public void Stop()
    {
        if (source.isPlaying)
            source.Stop();
    }

    // Call this if the dialogue restarts from the beginning
    public void ResetSequence()
    {
        Stop();
        currentIndex = -1;
    }
}