using System;
using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Switching Positions")]
    [SerializeField] private float panDuration = 0.75f;

    // The camera's normal pose relative to its parent (the head), captured once at startup,
    // before any sprint/bob effects can change it.
    private Vector3 restLocalPosition;
    private Quaternion restLocalRotation;

    // World rotation saved when leaving the player view, so you look the same way on return.
    private Quaternion lastRotation;

    // While true, the camera is pinned to heldPosition/heldRotation at the end of every frame.
    // PlayerMovement writes the camera's rotation each frame, which would otherwise snap it
    // back to the player's look angle as soon as the pan finishes.
    private bool holdingPose;
    private Vector3 heldPosition;
    private Quaternion heldRotation;
    private Coroutine panRoutine;

    public static CameraController Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        restLocalPosition = transform.localPosition;
        restLocalRotation = transform.localRotation;
    }

    // Runs after every Update, so it wins over PlayerMovement's camera rotation
    private void LateUpdate()
    {
        if (!holdingPose)
            return;

        transform.SetPositionAndRotation(heldPosition, heldRotation);
    }

    public void SwitchPosition(Transform endPoint, Action onComplete)
    {
        // Only save the rotation if we're in the normal player view (not mid-pan or already at a computer)
        if (!holdingPose && panRoutine == null)
        {
            lastRotation = transform.rotation;
        }

        StartPan(() => endPoint.position, endPoint.rotation, true, onComplete);
    }

    public void ReturnToLastPosition(Action onComplete)
    {
        // The end position is recomputed from the parent every frame of the pan,
        // so it is always the true rest height, no matter what the player was doing
        // (sprinting, bobbing, drifting) when the computer was opened.
        StartPan(GetRestWorldPosition, lastRotation, false, onComplete);
    }

    private Vector3 GetRestWorldPosition()
    {
        return transform.parent != null
            ? transform.parent.TransformPoint(restLocalPosition)
            : restLocalPosition;
    }

    private void StartPan(Func<Vector3> endPosGetter, Quaternion endRot, bool holdAtEnd, Action onComplete)
    {
        // Never run two pans at once, they would fight over the camera
        if (panRoutine != null)
        {
            StopCoroutine(panRoutine);
        }

        // Release any hold so it doesn't fight the pan
        holdingPose = false;

        panRoutine = StartCoroutine(PanCamera(transform.position, transform.rotation, endPosGetter, endRot, holdAtEnd, onComplete));
    }

    private IEnumerator PanCamera(Vector3 startPos, Quaternion startRot, Func<Vector3> endPosGetter, Quaternion endRot, bool holdAtEnd, Action onComplete)
    {
        float elapsed = 0f;

        while (elapsed < panDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / panDuration);

            transform.position = Vector3.Lerp(startPos, endPosGetter(), t);
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);

            yield return null;
        }

        Vector3 finalPos = endPosGetter();
        transform.position = finalPos;
        transform.rotation = endRot;

        if (holdAtEnd)
        {
            heldPosition = finalPos;
            heldRotation = endRot;
            holdingPose = true;
        }
        else
        {
            // Snap exactly to the rest height in case anything nudged it
            transform.localPosition = restLocalPosition;
        }

        panRoutine = null;

        onComplete?.Invoke();
    }
}

// using System;
// using System.Collections;
// using UnityEngine;
//
// public class CameraController : MonoBehaviour
// {
//     // [Header("Camera Settings")]
//     // [SerializeField] private float mouseSensitivity = 2f;
//     // [SerializeField] private float cameraClampY = 90f;
//     // [SerializeField] public bool isActive = true;
//
//     [Header("Switching Positions")] 
//     [SerializeField] private float panDuration;
//     
//     // [Header("References")]
//     // [SerializeField] private Transform playerBody;
//     //
//     // private float xRotation = 0f;
//     //private float yRotation = 0f;
//
//     // private float smoothX;
//     // private float smoothY;
//     private Vector3 lastPosition;
//     private Quaternion lastRotation;
//
//     // While true, the camera is pinned to heldPosition/heldRotation at the end of every frame.
//     // PlayerMovement writes the camera's rotation each frame, which would otherwise snap it
//     // back to the player's look angle as soon as the pan finishes.
//     private bool holdingPose;
//     private Vector3 heldPosition;
//     private Quaternion heldRotation;
//     private Coroutine panRoutine;
//     
//     public static CameraController Instance;
//
//     private void Awake()
//     {
//         if (Instance != null)
//         {
//             Destroy(gameObject);
//             return;
//         }
//
//         Instance = this;
//     }
//
//     private void Start()
//     {
//         Cursor.lockState = CursorLockMode.Locked;
//         Cursor.visible = false;
//     }
//
//     private void Update()
//     {
//         // if (!isActive)
//         //     return;
//         
//         // Camera movement handled by player movement script
//         
//         // float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
//         // float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
//         
//         // Apply rotation deltas
//         //yRotation += mouseX;
//         // xRotation -= mouseY;
//         // xRotation = Mathf.Clamp(xRotation, -cameraClampY, cameraClampY);
//         //
//         // transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
//         //
//         // playerBody.Rotate(Vector3.up * mouseX);
//     }
//
//     // Runs after every Update, so it wins over PlayerMovement's camera rotation
//     private void LateUpdate()
//     {
//         if (!holdingPose)
//             return;
//
//         transform.SetPositionAndRotation(heldPosition, heldRotation);
//     }
//     
//     public void SwitchPosition(Transform endPoint, Action onComplete)
//     {
//         // Save actual position/rotation values, not a Transform reference
//         lastPosition = transform.position;
//         lastRotation = transform.rotation;
//     
//         StartPan(endPoint.position, endPoint.rotation, true, onComplete);
//     }
//
//     public void ReturnToLastPosition(Action onComplete)
//     {
//         // Once back, PlayerMovement takes over the camera again
//         StartPan(lastPosition, lastRotation, false, onComplete);
//     }
//
//     private void StartPan(Vector3 endPos, Quaternion endRot, bool holdAtEnd, Action onComplete)
//     {
//         // Never run two pans at once, they would fight over the camera
//         if (panRoutine != null)
//         {
//             StopCoroutine(panRoutine);
//         }
//
//         // Release any hold so it doesn't fight the pan
//         holdingPose = false;
//
//         panRoutine = StartCoroutine(PanCamera(transform.position, transform.rotation, endPos, endRot, holdAtEnd, onComplete));
//     }
//
//     private IEnumerator PanCamera(Vector3 startPos, Quaternion startRot, Vector3 endPos, Quaternion endRot, bool holdAtEnd, Action onComplete)
//     {
//         float elapsed = 0f;
//
//         while (elapsed < panDuration)
//         {
//             elapsed += Time.deltaTime;
//             float t = Mathf.SmoothStep(0f, 1f, elapsed / panDuration);
//
//             transform.position = Vector3.Lerp(startPos, endPos, t);
//             transform.rotation = Quaternion.Slerp(startRot, endRot, t);
//
//             yield return null;
//         }
//
//         transform.position = endPos;
//         transform.rotation = endRot;
//
//         if (holdAtEnd)
//         {
//             heldPosition = endPos;
//             heldRotation = endRot;
//             holdingPose = true;
//         }
//
//         panRoutine = null;
//
//         onComplete?.Invoke();
//     }
// }