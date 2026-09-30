using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(SphereCollider))]
public class Poison : MonoBehaviour, IThrowable
{
    [Header("Poison Settings")]
    [SerializeField] private int damage;
    [SerializeField] private float tickRate = 0.1f;
    [SerializeField] private float duration;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private float throwForce = 15f;
    [SerializeField] private LayerMask groundMask;

    [Header("References")] 
    [SerializeField] private DecalProjector[] poisonSplatters;
    [SerializeField] private GameObject mesh;
    [SerializeField] private BoxCollider boxCollider;
    [SerializeField] private SphereCollider sphereCollider;
    [SerializeField] ParticleSystem particles;
    private Rigidbody rb;

    private bool isThrown;
    private bool isActive; // true for `duration` after landing - separate from isThrown so damage stops even if still isThrown

    private readonly Dictionary<ITargetable, Coroutine> activeDamageRoutines = new Dictionary<ITargetable, Coroutine>();

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Throw()
    {
        Vector3 direction = Camera.main.transform.forward;
            
        rb.useGravity = true;
        rb.linearVelocity = direction.normalized * throwForce;
        isThrown = true;

        if (TryGetComponent(out Interactable interactable))
        {
            interactable.enabled = false;
        }
    }

    public void Throw(Vector3 direction)
    {
        rb.useGravity = true;
        rb.linearVelocity = direction.normalized * throwForce;
        isThrown = true;

        if (TryGetComponent(out Interactable interactable))
        {
            interactable.enabled = false;
        }
    }

    private void EnableDecals()
    {
        foreach (var decal in poisonSplatters)
        {
            decal.gameObject.SetActive(true);
            
            float x = Random.Range(-3f, 3f);
            float z = Random.Range(-3f, 3f);
        
            Vector3 position = transform.position + new Vector3(x, 0.1f, z);
            
            decal.gameObject.transform.position = position;
        }
    }

    private IEnumerator FadeDecals()
    {
        float elapsedTime = 0;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            
            foreach (var decal in poisonSplatters)
            {
                decal.fadeFactor = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            }
            
            yield return null;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isThrown || isActive) return;
        if ((groundMask.value & (1 << collision.gameObject.layer)) == 0) return;
        
        SoundPlayer.Instance.PlaySound(SoundID.PoisonBottleShatter, transform.position);
            
        rb.isKinematic = true;
        mesh.SetActive(false);
        boxCollider.enabled = false;
        sphereCollider.enabled = true;

        isActive = true;
        transform.rotation = Quaternion.Euler(0,0,0);
        transform.position = collision.contacts[0].point;
        StartCoroutine(EndAfterDuration());
        particles.Play();
            
        EnableDecals();
        StartCoroutine(FadeDecals());
    }

    private IEnumerator EndAfterDuration()
    {
        yield return new WaitForSeconds(duration);

        isActive = false;

        // Stop any targets still standing in the cloud when it expires
        foreach (Coroutine routine in activeDamageRoutines.Values)
        {
            StopCoroutine(routine);
        }
        activeDamageRoutines.Clear();
        
        particles.Stop();
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;
        if ((layerMask.value & (1 << other.gameObject.layer)) == 0) return;

        ITargetable target = other.GetComponentInParent<ITargetable>();
        if (target == null) return;
        if (activeDamageRoutines.ContainsKey(target)) return; // already ticking (e.g. multiple colliders on the same rat)

        activeDamageRoutines[target] = StartCoroutine(DamageOverTime(target));
    }

    private void OnTriggerExit(Collider other)
    {
        ITargetable target = other.GetComponentInParent<ITargetable>();
        if (target == null) return;

        if (activeDamageRoutines.TryGetValue(target, out Coroutine routine))
        {
            StopCoroutine(routine);
            activeDamageRoutines.Remove(target);
        }
    }

    private IEnumerator DamageOverTime(ITargetable target)
    {
        WaitForSeconds wait = new WaitForSeconds(tickRate);

        while (true)
        {
            target.TakeDamage(damage);
            yield return wait;
        }
    }
}

public interface IThrowable
{
    void Throw(Vector3 direction);
}