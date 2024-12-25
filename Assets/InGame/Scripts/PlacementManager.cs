using UnityEngine;
using System.Collections;

public class PlacementManager : MonoBehaviour
{
    CharacterType characterType;

    public GameObject MainSpriteObj;
    public GameObject CannotPlaceInvalidObj;
    public GameObject BlueBorder;
    public GameObject YellowBorder;
    [SerializeField] private bool isValidPlacement;
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
    Token token;
    void Start()
    {
        token = GetComponent<Token>();
        spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("No SpriteRenderer found on the first child of the token! ");
        }

        DragEnableOrDisable(true);
        InitialPosition = transform.position;
        mainCamera = Camera.main;

        characterType = token.characterData.characterType;
        HandleBorder();

        // Log initial position
        Debug.Log($"Initial Token Position: {transform.position}");

        // Delay the placement check
        StartCoroutine(DelayedCheckPlacementRules());
    }

    private IEnumerator DelayedCheckPlacementRules()
    {
        yield return new WaitForEndOfFrame(); // Wait for one frame to ensure initialization
        CheckPlacementRules();
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
        if (GameManager.Instance.getCurrentPhase() == GamePhase.GamePlay && GetComponent<Rigidbody2D>().bodyType != RigidbodyType2D.Dynamic)
        {
            SetToDynamic();

        }
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
        // Do not allow dragging if Drag is disabled or the token does not belong to the current player
        if (!Drag || isTokenPlaced) return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            // Allow drag only if the token itself is clicked
            if (hit.collider != null && hit.collider.gameObject == gameObject && token.IsCurrentPlayerOwner())
            {
                isBeingDragged = true;

                MapScroll.Instance.DisableScroll();
                Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                offset = transform.position - new Vector3(mousePosition.x, mousePosition.y, transform.position.z);

            }
        }

        if (Input.GetMouseButton(0) && isBeingDragged)
        {
            UIManager.Instance.OkButton.gameObject.SetActive(false);
            Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = transform.position.z;
            transform.position = mousePosition + offset;
        }

        if (Input.GetMouseButtonUp(0) && isBeingDragged)
        {
            isBeingDragged = false;
            MapScroll.Instance.EnableScroll(); // Reset interaction state

            // Validate placement after drag ends
            CheckPlacementRules();

            if (isValidPlacement)
            {
                EventManager.TriggerEvent<PlacementManager>("TokenPlaced", GetComponent<PlacementManager>());
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
    public void CheckPlacementRules()
    {
        isValidPlacement = false;

        // Determine placement validity based on character type and raycast results
        switch (characterType)
        {
            case CharacterType.Mermaid:
                // Valid only on Water, invalid on Land or Base
                isValidPlacement = CheckPlacementWithRaycast("Water") &&
                                   !CheckPlacementWithRaycast("Land") &&
                                   !CheckPlacementWithRaycast("Base");
                break;
            case CharacterType.Knight:
                isValidPlacement = CheckPlacementWithRaycast("Land") || CheckPlacementWithRaycast("Base");
                break;
            case CharacterType.Golem:
                isValidPlacement = CheckPlacementWithRaycast("Land") || CheckPlacementWithRaycast("Base");
                break;
            case CharacterType.Dwarf:
                isValidPlacement = CheckPlacementWithRaycast("Land") || CheckPlacementWithRaycast("Base");
                break;
            case CharacterType.King:
                isValidPlacement = CheckPlacementWithRaycast("Land") || CheckPlacementWithRaycast("Base");
                break;
            case CharacterType.Gryphon:
                isValidPlacement = CheckPlacementWithRaycast("Land") || CheckPlacementWithRaycast("Base");
                break;
            case CharacterType.Thief:
                isValidPlacement = CheckPlacementWithRaycast("Land") || CheckPlacementWithRaycast("Base");
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

        // Handle visuals based on placement validity
        if (isValidPlacement)
        {
            MainSpriteObj.SetActive(true);
            CannotPlaceInvalidObj.SetActive(false);
            //            HandleBorder(); // Show appropriate border
        }
        else
        {
            SoundManager.Instance?.PlayNotPossiblePlacement();
            MainSpriteObj.SetActive(false);
            CannotPlaceInvalidObj.SetActive(true);

        }
    }

    private void HandleBorder()
    {

        if (token.owner == 1)
        {
            BlueBorder.SetActive(true);
            YellowBorder.SetActive(false);
        }
        else
        {
            YellowBorder.SetActive(true);
            BlueBorder.SetActive(false);
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

    public void ConfirmPlacement()
    {
        Debug.Log($"{characterType} placed successfully at {transform.position}");

        // Disable dragging and mark the token as placed
        DragEnableOrDisable(false);
        isTokenPlaced = true;

        // Set Rigidbody to Kinematic to prevent further movement
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        token.OnTokenPlaced();
        // Trigger an event for other systems (like turn progression)
        EventManager.TriggerEvent("OnTurnEnd");
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

