using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlacementManager : MonoBehaviour
{
    private CharacterType characterType;

    [Header("References")]
    public GameObject MainSpriteObj;
    public GameObject CannotPlaceInvalidObj;
    public GameObject BlueBorder;
    public GameObject YellowBorder;

    [Header("Settings")]
    [SerializeField] private bool isValidPlacement = false;
    public float rayLength = 10f; // Length of the ray
    public Color rayColor = Color.red; // Visualization color for ray
    public LayerMask raycastMask; // Mask for raycast

    private SpriteRenderer spriteRenderer;
    private Camera mainCamera;
    private Vector3 offset;
    private bool isBeingDragged = false;
    public bool isTokenPlaced = false;
    private bool dragEnabled = false;


    private Token token;

    private Dictionary<PolygonCollider2D, Vector2[]> polygonVerticesCache = new Dictionary<PolygonCollider2D, Vector2[]>();

    void Start()
    {
        token = GetComponent<Token>();
        spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;

        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer not found on the token's first child.");
        }

        EnableDrag(true);

        characterType = token.characterData.characterType;
        HandleBorder();

        // Delay placement rule check for initialization
        StartCoroutine(DelayedCheckPlacementRules());
    }

    private IEnumerator DelayedCheckPlacementRules()
    {
        yield return new WaitForEndOfFrame();
        CheckPlacementRules();
    }

    private void EnableDrag(bool isEnabled)
    {
        dragEnabled = isEnabled;
    }

    private void Update()
    {
        HandleDrag();

        GamePhase currentPhase = GameManager.Instance.getCurrentPhase();

        if (currentPhase == GamePhase.GamePlay && token.characterData.characterType != CharacterType.Golem && GetComponent<Rigidbody2D>().bodyType != RigidbodyType2D.Dynamic)
        {
            SetRigidbodyType(RigidbodyType2D.Dynamic);
        }
        else if (currentPhase == GamePhase.Placement)
        {
            SetRigidbodyType(RigidbodyType2D.Kinematic);
        }

        if (token.characterData.characterType == CharacterType.Golem && !token.IsImmobile() && GetComponent<Rigidbody2D>().bodyType != RigidbodyType2D.Dynamic)
        {
            SetRigidbodyType(RigidbodyType2D.Dynamic);
            Debug.LogError("Golem is now dynamic.");
        }

        if (isBeingDragged)
        {
            CheckPlacementRules();
        }

        DebugDrawRay();
    }

    private void HandleDrag()
    {
        if (!dragEnabled || isTokenPlaced) return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

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
            MapScroll.Instance.EnableScroll();
            CheckPlacementRules();

            if (isValidPlacement)
            {
                EventManager.TriggerEvent<PlacementManager>("TokenPlaced", this);
            }
            else
            {
                ResetPosition();
            }
        }
    }
    // void OnDrawGizmos()
    // {
    //     float radius = GetComponent<CircleCollider2D>().radius;
    //     Vector3 position = transform.position;

    //     // Create an array of points around the perimeter of the circle
    //     int numRays = 360; // Number of rays to cast
    //     for (int i = 0; i < numRays; i++)
    //     {
    //         float angle = i * Mathf.Deg2Rad * (360f / numRays);
    //         Vector3 rayOrigin = position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

    //         // Draw the ray in the Scene view with Z = -1 and direction along the Z axis
    //         Gizmos.color = Color.green; // Set the ray color to green
    //         Gizmos.DrawLine(new Vector3(rayOrigin.x, rayOrigin.y, rayOrigin.z), new Vector3(rayOrigin.x, rayOrigin.y, -1) + Vector3.forward * 3);
    //     }
    // }
    private void SetRigidbodyType(RigidbodyType2D type)
    {
        GetComponent<Rigidbody2D>().bodyType = type;
    }

    public void CheckPlacementRules()
    {
        isValidPlacement = false;

        switch (characterType)
        {
            case CharacterType.Mermaid:

                isValidPlacement = token.characterData.ability.CheckPlacementForMermaid(this);

                break;

            default:
                isValidPlacement = CheckPlacement("Base");
                break;
        }

        MainSpriteObj.SetActive(isValidPlacement);
        CannotPlaceInvalidObj.SetActive(!isValidPlacement);
    }

    private void DebugDrawRay()
    {
        Debug.DrawRay(transform.position, Vector3.back * rayLength, rayColor);
    }

    private bool CheckPlacement(string tag)
    {
        float radius = GetComponent<CircleCollider2D>().radius;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius, raycastMask);

        foreach (Collider2D collider in colliders)
        {
            Base playerBase = collider.GetComponent<Base>();
            if (playerBase != null && (playerBase.ownerID != token.owner && playerBase.ownerID != 0))
            {
                return false;
            }

            if (collider.CompareTag(tag))
            {
                return true;
            }
        }
        return false;
    }

    public void ConfirmPlacement()
    {
        Debug.Log($"{characterType} placed successfully at  {transform.position}");
        EnableDrag(false);
        isTokenPlaced = true;
        SetRigidbodyType(RigidbodyType2D.Kinematic);
        token.OnTokenPlaced();
    }
    private void DebugDrawEdge(Vector2 closestPoint)
    {
        Debug.DrawLine(transform.position, closestPoint, Color.green, 0.1f);
    }
    private void ResetPosition()
    {
        Debug.Log("Invalid placement. Resetting token position.");
        Handheld.Vibrate();
        SoundManager.Instance?.PlayNotPossiblePlacement();
        EventManager.TriggerEvent<bool>("TokenPlaced", false);

    }

    private void HandleBorder()
    {
        Color playerColor = GameManager.Instance.GetPlayerColor(token.owner);
        YellowBorder.SetActive(playerColor == Color.yellow);
        BlueBorder.SetActive(playerColor != Color.yellow);
    }



}
