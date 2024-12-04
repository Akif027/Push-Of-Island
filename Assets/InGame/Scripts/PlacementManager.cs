using UnityEngine;

public class PlacementManager : MonoBehaviour
{
    public CharacterType characterType; // Enum defining character types (e.g., Mermaid, Knight, etc.)

    private bool isValidPlacement;
    private Renderer tokenRenderer;
    public Vector3 InitialPosition;
    private Vector3 offset;
    private Camera mainCamera;
    private bool isBeingDragged = false;

    [Header("Raycast Settings")]
    public float rayLength = 10f; // Length of the ray
    public Color rayColor = Color.red; // Color of the ray for visualization
    public LayerMask raycastMask; // LayerMask to exclude the object's own collider

    void Start()
    {
        tokenRenderer = GetComponent<Renderer>();
        InitialPosition = transform.position;
        mainCamera = Camera.main;
    }

    void Update()
    {
        HandleDrag();

        // Continuously check placement validity while dragging
        if (isBeingDragged)
        {
            CheckPlacementRules();
        }

        // Visualize the ray
        DebugDrawRay();
    }

    private void HandleDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                isBeingDragged = true;
                InteractionManager.IsDragging = true; // Notify interaction manager
                Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                offset = transform.position - new Vector3(mousePosition.x, mousePosition.y, transform.position.z);
                Debug.Log("Mouse Down on " + gameObject.name);
            }
        }

        if (Input.GetMouseButton(0) && isBeingDragged)
        {
            Debug.Log("Dragging " + gameObject.name);
            Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = transform.position.z;
            transform.position = mousePosition + offset;
        }

        if (Input.GetMouseButtonUp(0) && isBeingDragged)
        {
            Debug.Log("Mouse Released...");
            isBeingDragged = false;
            InteractionManager.IsDragging = false; // Reset interaction state

            // Validate placement after drag ends
            CheckPlacementRules();

            if (isValidPlacement)
            {
                ConfirmPlacement();
            }
            else
            {
                ResetPosition();
            }
        }
    }

    private void CheckPlacementRules()
    {
        isValidPlacement = false;

        switch (characterType)
        {
            case CharacterType.Mermaid:
                isValidPlacement = CheckPlacementWithRaycast("Water") || CheckPlacementWithRaycast("Base");
                break;
            case CharacterType.Knight:
                isValidPlacement = CheckPlacementWithRaycast("Land") && CheckPlacementWithRaycast("Base");
                break;
            case CharacterType.Golem:
                isValidPlacement = CheckPlacementWithRaycast("Land") && !CheckPlacementWithRaycast("Water");
                break;
            default:
                isValidPlacement = CheckPlacementWithRaycast("Base");
                break;
        }

        // Change token color based on placement validity
        if (tokenRenderer != null)
        {
            tokenRenderer.material.color = isValidPlacement ? Color.green : Color.red;
        }
    }

    private bool CheckPlacementWithRaycast(string tag)
    {
        // Cast a ray in the -Z direction from the object's position
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector3.back, rayLength, raycastMask);
        if (hit.collider != null)
        {
            Debug.Log($"Raycast hit {hit.collider.gameObject.name} with tag {hit.collider.tag}");
            return hit.collider.CompareTag(tag);
        }
        return false;
    }

    private void ConfirmPlacement()
    {
        Debug.Log($"{characterType} placed successfully at {transform.position}");
        enabled = false; // Disable further movement
    }

    private void ResetPosition()
    {
        Debug.Log("Invalid placement. Resetting token position.");
        transform.position = InitialPosition;
    }

    private void DebugDrawRay()
    {
        // Visualize the ray in the Scene view in the -Z direction
        Debug.DrawRay(transform.position, Vector3.back * rayLength, rayColor);
    }
}
public static class InteractionManager
{
    public static bool IsDragging { get; set; } = false;
}