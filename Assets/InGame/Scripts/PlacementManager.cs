using UnityEngine;

public class PlacementManager : MonoBehaviour
{
    public CharacterType characterType; // Enum defining character types (e.g., Mermaid, Knight, etc.)
    public Sprite ValidToken; // Sprite for valid placement
    public Sprite InvalidValidToken; // Sprite for invalid placement
    public int owner; // Player number who owns this token (e.g., 1 or 2)

    private bool isValidPlacement;
    private SpriteRenderer spriteRenderer; // Renderer for the token sprite
    public Vector3 InitialPosition;
    private Vector3 offset;
    private Camera mainCamera;
    private bool isBeingDragged = false;

    [Header("Raycast Settings")]
    public float rayLength = 10f; // Length of the ray
    public Color rayColor = Color.red; // Color of the ray for visualization
    public LayerMask raycastMask; // LayerMask to exclude the object's own collider
    public bool isTokenPlaced = false;
    bool Drag = false;
    void Start()
    {
        spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("No SpriteRenderer found on the first child of the token!");
        }
        CheckPlacementRules();
        DragEnableOrDisable(true);
        InitialPosition = transform.position;
        mainCamera = Camera.main;
    }
    private void DragEnableOrDisable(bool isTrue)
    {

        Drag = isTrue;

    }

    public bool ReturnDragStatus()
    {
        return Drag;

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
        if (!Drag) return;
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            // Allow drag only if the token belongs to the current player
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                isBeingDragged = true;
                InteractionManager.IsDragging = true; // Notify interaction manager
                Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                offset = transform.position - new Vector3(mousePosition.x, mousePosition.y, transform.position.z);
                Debug.Log($"Player {owner} is dragging token: " + gameObject.name);
            }
        }

        if (Input.GetMouseButton(0) && isBeingDragged)
        {
            Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = transform.position.z;
            transform.position = mousePosition + offset;
        }

        if (Input.GetMouseButtonUp(0) && isBeingDragged)
        {
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
    public void SetToDynamic()
    {
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;


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
                isValidPlacement = CheckPlacementWithRaycast("Land") || CheckPlacementWithRaycast("Base");
                break;
            case CharacterType.Golem:
                isValidPlacement = CheckPlacementWithRaycast("Land") || CheckPlacementWithRaycast("Base");
                break;
            case CharacterType.Dwarf:
                isValidPlacement = CheckPlacementWithRaycast("Water") || CheckPlacementWithRaycast("Base");
                break;
            case CharacterType.King:
                isValidPlacement = CheckPlacementWithRaycast("Land") || CheckPlacementWithRaycast("Base");
                break;
            case CharacterType.Gryphon:
                isValidPlacement = CheckPlacementWithRaycast("Land") || CheckPlacementWithRaycast("Base");
                break;
            case CharacterType.Thief:
                isValidPlacement = CheckPlacementWithRaycast("Water") || CheckPlacementWithRaycast("Base");
                break;
            case CharacterType.Rogue:
                isValidPlacement = CheckPlacementWithRaycast("Land") || CheckPlacementWithRaycast("Base");
                break;
            case CharacterType.Enchantress:
                isValidPlacement = CheckPlacementWithRaycast("Land") || CheckPlacementWithRaycast("Base");
                break;
            case CharacterType.Satyr:
                isValidPlacement = CheckPlacementWithRaycast("Land") || CheckPlacementWithRaycast("Base");
                break;
            default:
                isValidPlacement = CheckPlacementWithRaycast("Base");
                break;
        }

        // Update sprite based on placement validity
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = isValidPlacement ? ValidToken : InvalidValidToken;
        }
    }

    private bool CheckPlacementWithRaycast(string tag)
    {
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
        DragEnableOrDisable(false);
        isTokenPlaced = true;
        EventManager.TriggerEvent<bool>("TokenPlaced", true);
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
    }

    private void ResetPosition()
    {
        Debug.Log("Invalid placement. Resetting token position.");
        transform.position = InitialPosition;
        EventManager.TriggerEvent<bool>("TokenPlaced", false);
    }

    private void DebugDrawRay()
    {
        Debug.DrawRay(transform.position, Vector3.back * rayLength, rayColor);
    }
}

public static class InteractionManager
{
    public static bool IsDragging { get; set; } = false;
}
