using System.Collections;
using UnityEngine;

public class MouseTrap : MonoBehaviour
{
    [SerializeField] private int damage;
    [SerializeField] private Transform snapPosition;
    [SerializeField] private float cleanUpDuration;

    private Interactable interactable;
    private GameObject caughtRat;
    private Rigidbody trapRigidbody;
    
    // Clean up
    private Coroutine cleanUpRoutine;
    private Coroutine shakeRoutine;
    private Vector3 shakeOrigin;

    private void Start()
    {
        interactable = GetComponent<Interactable>();
        trapRigidbody = GetComponent<Rigidbody>();
        
        if (interactable != null)
            interactable.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (caughtRat)
            return;

        if (other.CompareTag("Rat"))
        {
            caughtRat = other.gameObject;
            StartCoroutine(CatchRat());
        }
    }

    private IEnumerator CatchRat()
    {
        gameObject.layer = 11;
        
        caughtRat.transform.position = snapPosition.position;
        caughtRat.transform.parent = transform;
        RatController rat = caughtRat.GetComponent<RatController>();
        rat.TakeDamage(damage);
        rat.boxCollider.enabled = false;
            
        if (interactable != null)
            interactable.enabled = true;
        
        yield return new WaitForSeconds(2f);
        
        trapRigidbody.isKinematic = true;
    }
    
    public void StartCleaning()
    {
        if (cleanUpRoutine != null)
            return;

        shakeOrigin = transform.localPosition;
        cleanUpRoutine = StartCoroutine(CleanUp());
        shakeRoutine = StartCoroutine(CleanUpShakeAnimation());
    }

    public void StopCleaning()
    {
        if (cleanUpRoutine == null)
            return;
        
        StopCoroutine(cleanUpRoutine);
        cleanUpRoutine = null;
        StopCoroutine(shakeRoutine);
        shakeRoutine = null;
        
        transform.localPosition = shakeOrigin;
    }

    private IEnumerator CleanUp()
    {
        yield return new WaitForSeconds(cleanUpDuration);
        StopCoroutine(shakeRoutine);
        Destroy(gameObject);
    }

    private IEnumerator CleanUpShakeAnimation()
    {
        Vector3 shakeDirection;
        
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            
            shakeDirection = Random.onUnitSphere * 0.05f;
            transform.localPosition = shakeOrigin + shakeDirection;

            yield return new WaitForSeconds(0.1f);
            
            transform.localPosition = shakeOrigin;
        }
    }
}
