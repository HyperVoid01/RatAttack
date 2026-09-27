using System.Collections;
using UnityEngine;

public class Sink : MonoBehaviour
{
    [SerializeField] private float cleanDuration;
    [SerializeField] private ParticleSystem soapParticles;
    
    private Coroutine cleanRoutine;

    public void StartCleaningHands()
    {
        if (cleanRoutine != null)
            return;
        
        cleanRoutine = StartCoroutine(CleanHands());
    }

    public void StopCleaningHands()
    {
        if (cleanRoutine == null)
            return;
        
        StopCoroutine(cleanRoutine);
        cleanRoutine = null;
        soapParticles.Stop();
    }

    private IEnumerator CleanHands()
    {
        soapParticles.Play();
        yield return new WaitForSeconds(cleanDuration);
        GameManager.Instance.CleanHands();
        soapParticles.Stop();
    }
}
