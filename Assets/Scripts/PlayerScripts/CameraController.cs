using System;
using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // [Header("Camera Settings")]
    // [SerializeField] private float mouseSensitivity = 2f;
    // [SerializeField] private float cameraClampY = 90f;
    // [SerializeField] public bool isActive = true;

    [Header("Switching Positions")] 
    [SerializeField] private float panDuration;
    
    // [Header("References")]
    // [SerializeField] private Transform playerBody;
    //
    // private float xRotation = 0f;
    //private float yRotation = 0f;

    // private float smoothX;
    // private float smoothY;
    private Vector3 lastPosition;
    private Quaternion lastRotation;
    
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
    }

    private void Update()
    {
        // if (!isActive)
        //     return;
        
        // Camera movement handled by player movement script
        
        // float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        // float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        
        // Apply rotation deltas
        //yRotation += mouseX;
        // xRotation -= mouseY;
        // xRotation = Mathf.Clamp(xRotation, -cameraClampY, cameraClampY);
        //
        // transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        //
        // playerBody.Rotate(Vector3.up * mouseX);
    }
    
    public void SwitchPosition(Transform endPoint, Action onComplete)
    {
        // Save actual position/rotation values, not a Transform reference
        lastPosition = transform.position;
        lastRotation = transform.rotation;
    
        StartCoroutine(PanCamera(transform.position, transform.rotation, endPoint.position, endPoint.rotation, onComplete));
    }

    public void ReturnToLastPosition(Action onComplete)
    {
        StartCoroutine(PanCamera(transform.position, transform.rotation, lastPosition, lastRotation, onComplete));
    }

    private IEnumerator PanCamera(Vector3 startPos, Quaternion startRot, Vector3 endPos, Quaternion endRot, Action onComplete)
    {
        float elapsed = 0f;

        while (elapsed < panDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / panDuration);

            transform.position = Vector3.Lerp(startPos, endPos, t);
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);

            yield return null;
        }

        transform.position = endPos;
        transform.rotation = endRot;

        onComplete?.Invoke();
    }
}