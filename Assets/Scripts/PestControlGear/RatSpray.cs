using System.Collections;
using UnityEngine;

public class RatSpray : MonoBehaviour, IHoldInteraction
{
    [SerializeField] private int damage;
    [SerializeField] private float radius;
    [SerializeField] private float range;
    [SerializeField] private float tickRate = 0.1f; // seconds between damage ticks while held
    [SerializeField] private ParticleSystem sprayParticle;
    [SerializeField] private LayerMask layerMask;

    private Coroutine sprayRoutine;

    public bool IsSpraying => sprayRoutine != null;

    public void OnHoldStart() => StartSpray();
    public void OnHoldEnd() => StopSpray();

    public void StartSpray()
    {
        if (sprayRoutine != null)
            return; // already spraying, don't stack coroutines

        sprayRoutine = StartCoroutine(Spray());
    }

    public void StopSpray()
    {
        if (sprayRoutine == null)
            return;

        StopCoroutine(sprayRoutine);
        sprayRoutine = null;

        if (sprayParticle.isPlaying)
            sprayParticle.Stop();
    }

    private IEnumerator Spray()
    {
        sprayParticle.Play();

        WaitForSeconds wait = new WaitForSeconds(tickRate);

        while (true)
        {
            DoSprayTick();
            yield return wait;
        }
    }

    private void DoSprayTick()
    {
        Vector3 point1 = transform.position;
        Vector3 point2 = transform.position + Vector3.forward;

        RaycastHit[] hits = Physics.CapsuleCastAll(point1, point2, radius, Vector3.down, range, layerMask);

        foreach (RaycastHit hit in hits)
        {
            // Colliders can be on child objects, so check the parent hierarchy too
            ITargetable target = hit.collider.GetComponentInParent<ITargetable>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }
        }
    }

    private void OnDisable()
    {
        // Safety net: don't leave a dangling coroutine/particle if the object is disabled mid-spray
        StopSpray();
    }
}