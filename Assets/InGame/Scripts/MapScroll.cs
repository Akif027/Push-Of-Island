using UnityEngine;

using Unity.Cinemachine;

public class MapScroll : MonoBehaviour
{
    [Header("Cinemachine References")]
    public CinemachineCamera cinemachineCamera; // Reference to the Cinemachine camera
    public CinemachineConfiner2D confiner2D;    // Reference to the Cinemachine Confiner 2D component
    public Collider2D boundingShape;           // Collider used as the bounding shape for the confiner

    [Header("Scroll Settings")]
    public float scrollSpeed = 0.5f; // Speed of the scroll

    [Header("Zoom Settings")]
    public float minZoom = 5f; // Minimum orthographic size for the camera
    public float maxZoom = 15f; // Maximum orthographic size for the camera
    public float zoomSpeed = 0.1f; // Speed of zooming

    private Camera mainCamera; // Main Unity Camera
    private Vector3 dragOrigin; // Stores the position where dragging started

    void Start()
    {
        if (cinemachineCamera == null)
        {
            Debug.LogError("Cinemachine camera is not assigned!");
            return;
        }

        if (confiner2D == null)
        {
            Debug.LogError("Cinemachine Confiner 2D is not assigned!");
            return;
        }

        if (boundingShape != null)
        {
            // Assign the bounding shape to the confiner
            confiner2D.BoundingShape2D = boundingShape;
        }
        else
        {
            Debug.LogWarning("Bounding shape not assigned! Camera may go out of bounds.");
        }

        mainCamera = Camera.main;
    }

    void Update()
    {
        HandleDrag();
        HandleZoom();
    }

    private void HandleDrag()
    {
        if (Input.GetMouseButtonDown(0)) // On mouse (or finger) press
        {
            dragOrigin = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(0)) // While holding mouse (or finger)
        {
            Vector3 currentPoint = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector3 difference = dragOrigin - currentPoint;

            // Move the camera
            MoveCamera(difference);
        }
    }

    private void MoveCamera(Vector3 offset)
    {
        // Get the current camera position
        Transform camTransform = cinemachineCamera.transform;
        Vector3 newPosition = camTransform.position + offset * scrollSpeed;

        // Clamp the position within the bounds defined by the confiner
        if (confiner2D != null && boundingShape != null)
        {
            Bounds bounds = boundingShape.bounds;
            newPosition.x = Mathf.Clamp(newPosition.x, bounds.min.x, bounds.max.x);
            newPosition.y = Mathf.Clamp(newPosition.y, bounds.min.y, bounds.max.y);
        }

        // Apply the new position
        camTransform.position = new Vector3(newPosition.x, newPosition.y, camTransform.position.z);
    }

    private void HandleZoom()
    {
        if (Input.touchCount == 2) // Pinch zoom
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            // Find the position in the previous frame of each touch
            Vector2 touch0PrevPos = touch0.position - touch0.deltaPosition;
            Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;

            // Find the magnitude of the vector (distance) between the touches in each frame
            float prevMagnitude = (touch0PrevPos - touch1PrevPos).magnitude;
            float currentMagnitude = (touch0.position - touch1.position).magnitude;

            // Find the difference in the distances between each frame
            float difference = prevMagnitude - currentMagnitude;

            // Adjust the orthographic size based on the zoomSpeed and the difference in magnitude
            mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize + difference * zoomSpeed, minZoom, maxZoom);
        }
    }
}
