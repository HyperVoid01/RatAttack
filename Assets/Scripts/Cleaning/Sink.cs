using System.Collections;
using UnityEngine;

public class Sink : MonoBehaviour
{
    [SerializeField] private float cleanDuration;
    [SerializeField] private ParticleSystem soapParticles;
    
    private Coroutine cleanRoutine;
    
    private AudioSource audioSource;

    public void StartCleaningHands()
    {
        if (cleanRoutine != null)
            return;
        
        cleanRoutine = StartCoroutine(CleanHands());
        audioSource = SoundPlayer.Instance.PlayLoop(SoundID.WashingHands, transform.position);
    }

    public void StopCleaningHands()
    {
        if (cleanRoutine == null)
            return;
        
        StopCoroutine(cleanRoutine);
        cleanRoutine = null;
        soapParticles.Stop();
        SoundPlayer.Instance.StopLoop(audioSource);
    }

    private IEnumerator CleanHands()
    {
        soapParticles.Play();
        yield return new WaitForSeconds(cleanDuration);
        GameManager.Instance.CleanHands();
        soapParticles.Stop();
        SoundPlayer.Instance.StopLoop(audioSource);
    }
}
