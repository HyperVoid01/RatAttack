using System.Collections.Generic;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    public static SoundPlayer Instance { get; private set; }

    [Header("Sound Library")]
    [SerializeField] private SoundLibrary soundLibrary;

    [Header("Sound Settings")]
    [SerializeField, Range(0f, 1f)] private float volume = 1f;
    [SerializeField, Range(0f, 1f)] private float spatialBlend = 1f;

    private readonly List<AudioSource> activeLoops = new List<AudioSource>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // ---------- One-shot sounds ----------

    public void PlaySound(SoundID soundID, Vector3 location)
    {
        AudioClip clip = GetClip(soundID);

        if (clip == null)
            return;

        GameObject soundObject = new GameObject("Temporary Sound");
        soundObject.transform.position = location;

        AudioSource source = ConfigureSource(soundObject, clip, false);
        source.Play();

        Destroy(soundObject, clip.length);
    }

    // ---------- Looped sounds ----------

    /// <summary>
    /// Starts a looping sound at a fixed position.
    /// Keep the returned AudioSource to pause, resume or stop it later.
    /// </summary>
    public AudioSource PlayLoop(SoundID soundID, Vector3 location)
    {
        return CreateLoop(soundID, location, null);
    }

    /// <summary>
    /// Starts a looping sound that follows the given transform
    /// (e.g. an engine hum on a car).
    /// </summary>
    public AudioSource PlayLoop(SoundID soundID, Transform followTarget)
    {
        if (followTarget == null)
        {
            Debug.LogWarning("PlayLoop called with a null follow target.");
            return null;
        }

        return CreateLoop(soundID, followTarget.position, followTarget);
    }

    /// <summary>Pauses a loop. Resume it with ResumeLoop.</summary>
    public void PauseLoop(AudioSource loop)
    {
        if (loop == null)
            return;

        loop.Pause();
    }

    /// <summary>Resumes a loop that was paused with PauseLoop.</summary>
    public void ResumeLoop(AudioSource loop)
    {
        if (loop == null)
            return;

        loop.UnPause();
    }

    /// <summary>Stops a loop and destroys it. The handle is invalid afterwards.</summary>
    public void StopLoop(AudioSource loop)
    {
        if (loop == null)
            return;

        activeLoops.Remove(loop);
        loop.Stop();
        Destroy(loop.gameObject);
    }

    public void PauseAllLoops()
    {
        CleanupLoops();

        foreach (AudioSource loop in activeLoops)
            loop.Pause();
    }

    public void ResumeAllLoops()
    {
        CleanupLoops();

        foreach (AudioSource loop in activeLoops)
            loop.UnPause();
    }

    public void StopAllLoops()
    {
        foreach (AudioSource loop in activeLoops)
        {
            if (loop == null)
                continue;

            loop.Stop();
            Destroy(loop.gameObject);
        }

        activeLoops.Clear();
    }

    // ---------- Helpers ----------

    private AudioSource CreateLoop(SoundID soundID, Vector3 location, Transform parent)
    {
        AudioClip clip = GetClip(soundID);

        if (clip == null)
            return null;

        GameObject soundObject = new GameObject($"Looping Sound ({soundID})");
        soundObject.transform.position = location;

        if (parent != null)
            soundObject.transform.SetParent(parent, true);

        AudioSource source = ConfigureSource(soundObject, clip, true);
        source.Play();

        activeLoops.Add(source);
        return source;
    }

    private AudioSource ConfigureSource(GameObject target, AudioClip clip, bool loop)
    {
        AudioSource source = target.AddComponent<AudioSource>();

        source.clip = clip;
        source.volume = volume;
        source.spatialBlend = spatialBlend;
        source.loop = loop;

        return source;
    }

    private AudioClip GetClip(SoundID soundID)
    {
        if (soundLibrary == null)
        {
            Debug.LogWarning("SoundPlayer has no SoundLibrary assigned.");
            return null;
        }

        return soundLibrary.GetSound(soundID);
    }

    // Removes loops whose GameObjects were destroyed elsewhere
    // (e.g. a followed object was destroyed).
    private void CleanupLoops()
    {
        activeLoops.RemoveAll(loop => loop == null);
    }
}