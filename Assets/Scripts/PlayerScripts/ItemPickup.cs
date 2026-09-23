using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [SerializeField] private float range; // Also max Hold distance
    [SerializeField] private float radius;
    [SerializeField] private float currentHoldDistance;
    [SerializeField] private float minHoldDistance;
    [SerializeField] private float maxHoldDistance; // How far until drop item
    [SerializeField] private float holdDistanceDelta; // How much hold distance changes
    [SerializeField] private LayerMask layer;
    [SerializeField] private Camera playerCamera;

    [Header("Hold Feel")]
    [SerializeField] private float followStrength = 12f;   // higher = snappier/stiffer, lower = floatier/laggier
    [SerializeField] private float rotationStrength = 12f;
    [SerializeField] private float maxHoldSpeed = 20f;      // clamp so a big correction doesn't fling the item

    public GameObject currentPickup;
    private Rigidbody currentPickupRb;
    private bool isHolding;
    
    private CollisionDetectionMode originalCollisionMode;

    private void Update()
    {
        AdjustHoldDistance();
    }

    private void LateUpdate()
    {
        if (Input.GetMouseButton(0) && Physics.SphereCast(playerCamera.transform.position, radius, playerCamera.transform.forward, out RaycastHit hit, range, layer) && !currentPickup)
        {
            currentPickup = hit.collider.gameObject;
            currentPickupRb = currentPickup.GetComponent<Rigidbody>();
            currentHoldDistance = Vector3.Distance(playerCamera.transform.position, currentPickup.transform.position);
            currentHoldDistance = Mathf.Clamp(currentHoldDistance, minHoldDistance, range);

            originalCollisionMode = currentPickupRb.collisionDetectionMode;
            currentPickupRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            currentPickupRb.useGravity = false;
            currentPickupRb.linearDamping = 0f;
            currentPickupRb.angularDamping = 0f;

            isHolding = true;
        }
        else if (currentPickup && Input.GetMouseButtonUp(0))
        {
            DropItem();
        }
    }

    private void FixedUpdate()
    {
        if (isHolding && currentPickup)
        {
            MovePickupItem();
        }
    }

    private void DropItem()
    {
        currentPickupRb.useGravity = true;
        currentPickupRb.collisionDetectionMode = originalCollisionMode;
        // no velocity reset, whatever velocity it has right now is the throw

        isHolding = false;
        currentPickup = null;
        currentPickupRb = null;
    }

    private void MovePickupItem()
    {
        HUDManager.Instance.DisableInteractionText();

        if (Vector3.Distance(playerCamera.transform.position, currentPickup.transform.position) > maxHoldDistance)
        {
            DropItem();
            return;
        }
        
        Transform camTransform = playerCamera.transform;
        Vector3 targetPosition = camTransform.position + camTransform.forward * currentHoldDistance;
        
        // Position: steer velocity toward the target point (spring-like)
        Vector3 toTarget = targetPosition - currentPickupRb.position;
        Vector3 desiredVelocity = toTarget * followStrength;
        currentPickupRb.linearVelocity = Vector3.ClampMagnitude(desiredVelocity, maxHoldSpeed);

        // Rotation: steer angular velocity toward matching the player's rotation
        Quaternion rotDelta = camTransform.rotation * Quaternion.Inverse(currentPickupRb.rotation);
        rotDelta.ToAngleAxis(out float angleDeg, out Vector3 axis);
        if (angleDeg > 180f) angleDeg -= 360f;

        if (Mathf.Abs(angleDeg) > Mathf.Epsilon)
        {
            currentPickupRb.angularVelocity = axis * (angleDeg * Mathf.Deg2Rad * rotationStrength);
        }
    }

    private void AdjustHoldDistance()
    {
        float scrollDelta = Input.GetAxis("Mouse ScrollWheel");

        if (scrollDelta != 0)
        {
            currentHoldDistance += scrollDelta * holdDistanceDelta;
            currentHoldDistance = Mathf.Clamp(currentHoldDistance, minHoldDistance, range);
        }
    }

    private void OnDrawGizmos()
    {
        if (playerCamera == null) return;

        Transform camTransform = playerCamera.transform;
        Vector3 start = camTransform.position;
        Vector3 direction = camTransform.forward;

        bool didHit = Physics.SphereCast(start, radius, direction, out RaycastHit hit, range, layer);
        float distance = didHit ? hit.distance : range;
        Vector3 end = start + direction * distance;

        Gizmos.color = didHit ? Color.green : Color.red;

        // Start and end spheres of the sweep
        Gizmos.DrawWireSphere(start, radius);
        Gizmos.DrawWireSphere(end, radius);

        // Side lines to suggest the swept "capsule" silhouette
        Vector3 up = camTransform.up * radius;
        Vector3 right = camTransform.right * radius;
        Gizmos.DrawLine(start + up, end + up);
        Gizmos.DrawLine(start - up, end - up);
        Gizmos.DrawLine(start + right, end + right);
        Gizmos.DrawLine(start - right, end - right);

        if (didHit)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(hit.point, 0.1f);
        }
    }
}