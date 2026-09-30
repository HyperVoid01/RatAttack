using System.Collections;
using UnityEngine;

public class RatSpray : MonoBehaviour, IHoldInteraction
{
    [Header("Spray Settings")]
    [SerializeField] private int damage;
    [SerializeField] private float radius;   // spread
    [SerializeField] private float range;    // how far the sweep travels
    [SerializeField] private float capsuleLength = 1f;
    [SerializeField] private float nozzleOffset = 0.3f; // pushes the cast start ahead of the can's own collider
    [SerializeField] private float tickRate = 0.1f; // seconds between damage ticks while held
    [SerializeField] private LayerMask layerMask;

    [Header("References")]
    [SerializeField] private ParticleSystem sprayParticle;

    [Header("Gizmo")]
    [SerializeField] private bool drawGizmoAlways = false;

    private Coroutine sprayRoutine;
    private AudioSource audioSource;

    public bool IsSpraying => sprayRoutine != null;

    // Shared by the cast and the gizmo so they can never disagree
    private Vector3 SweepDirection => transform.forward;
    private Vector3 CapsuleStart => transform.position + SweepDirection * nozzleOffset;
    private Vector3 CapsuleEnd => CapsuleStart + SweepDirection * capsuleLength;

    public void OnHoldStart() => StartSpray();
    public void OnHoldEnd() => StopSpray();

    public void StartSpray()
    {
        if (sprayRoutine != null)
            return; // already spraying, don't stack coroutines

        sprayRoutine = StartCoroutine(Spray());
        audioSource = SoundPlayer.Instance.PlayLoop(SoundID.RatSpray, transform);
    }

    public void StopSpray()
    {
        if (sprayRoutine == null)
            return;

        SoundPlayer.Instance.StopLoop(audioSource);
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
        RaycastHit[] hits = Physics.CapsuleCastAll(
            CapsuleStart, CapsuleEnd, radius, SweepDirection, range, layerMask);

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

    // ---------------- Gizmos ----------------

    private void OnDrawGizmosSelected()
    {
        DrawSprayGizmo();
    }

    private void OnDrawGizmos()
    {
        if (drawGizmoAlways)
            DrawSprayGizmo();
    }

    private void DrawSprayGizmo()
    {
        Vector3 dir = SweepDirection.normalized;
        Vector3 offset = dir * range;

        Vector3 p1 = CapsuleStart;
        Vector3 p2 = CapsuleEnd;
        Vector3 p1End = p1 + offset;
        Vector3 p2End = p2 + offset;

        // Start capsule (spread at the origin)
        Gizmos.color = Color.green;
        DrawWireCapsule(p1, p2, radius);

        // End capsule (spread at max range)
        Gizmos.color = Color.red;
        DrawWireCapsule(p1End, p2End, radius);

        // Sweep path: centre lines plus the outer edges of the swept volume
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(p1, p1End);
        Gizmos.DrawLine(p2, p2End);

        GetPerpendiculars(dir, out Vector3 a, out Vector3 b);
        foreach (Vector3 side in new[] { a, -a, b, -b })
        {
            Vector3 s = side * radius;
            Gizmos.DrawLine(p1 + s, p1End + s);
            Gizmos.DrawLine(p2 + s, p2End + s);
        }
    }

    private static void DrawWireCapsule(Vector3 p1, Vector3 p2, float r)
    {
        Gizmos.DrawWireSphere(p1, r);
        Gizmos.DrawWireSphere(p2, r);

        Vector3 axis = p2 - p1;
        if (axis.sqrMagnitude < 0.0001f) return;

        GetPerpendiculars(axis.normalized, out Vector3 a, out Vector3 b);
        foreach (Vector3 side in new[] { a, -a, b, -b })
        {
            Gizmos.DrawLine(p1 + side * r, p2 + side * r);
        }
    }

    private static void GetPerpendiculars(Vector3 dir, out Vector3 a, out Vector3 b)
    {
        Vector3 helper = Mathf.Abs(Vector3.Dot(dir, Vector3.up)) > 0.99f ? Vector3.right : Vector3.up;
        a = Vector3.Cross(dir, helper).normalized;
        b = Vector3.Cross(dir, a).normalized;
    }
}