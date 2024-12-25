using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(PlacementManager))]
public class Token : MonoBehaviour
{
    [SerializeField] private bool isUnlocked;
    private Vector3 lastPosition;
    public bool IsUnlocked
    {
        get => isUnlocked;
        set
        {
            isUnlocked = value;
            Debug.Log($"IsUnlocked updated to: {isUnlocked}");
        }
    }

    public int owner; // Player who owns this token
    public CharacterData characterData;

    [SerializeField] private Transform arrow; // Arrow GameObject for direction
    [SerializeField] private float linearDrag = 2f;

    private Rigidbody2D tokenRigidbody;
    private bool isDragging = false;
    private bool isImmobile = false; // Flag to track immobility status
    private Vector3 previousMousePosition;
    private bool isThrown = false;
    private float throwForce = 10f; // Base force for movement

    private void Awake()
    {
        tokenRigidbody = GetComponent<Rigidbody2D>();

        ValidateComponents();
    }

    private void Start()
    {
        InitializeProperties();
    }

    private void ValidateComponents()
    {
        if (!arrow) Debug.LogError("Arrow Transform is not assigned.");
        if (characterData == null) Debug.LogError("CharacterData is not assigned.");
    }

    private void InitializeProperties()
    {
        if (characterData == null) return;

        tokenRigidbody.mass = characterData.ability.weightMultiplier;
        tokenRigidbody.gravityScale = 0;
        tokenRigidbody.linearDamping = linearDrag;


    }

    private void Update()
    {


        CheckTokenPosition(); // Call the method if movement is detected

        if (isThrown)
            HandleTokenMovement();
        // Check if the character ability is active and if reflection is needed
        if (characterData.characterType == CharacterType.Satyr && characterData.ability != null)
        {
            // Use the common HandleReflection method from CharacterAbility
            characterData.ability.HandleReflection(this, new Vector2(10f, 10f));
        }
    }
    public bool IsImmobile()
    {
        return isImmobile;
    }

    public Rigidbody2D GetRigidbody2D()
    {
        return tokenRigidbody;
    }
    public void OnTokenPlaced()
    {
        if (characterData?.ability == null) return;

        characterData.ability.Activate(this); // Trigger ability-specific logic

        if (characterData.ability.becomesImmobileOnPlacement)
        {
            SetImmobile();
        }

        Debug.Log($"{characterData.characterName} placed successfully.");
    }

    public void SetImmobile()
    {
        if (characterData.characterType == CharacterType.Golem && characterData.ability.becomesImmobileOnPlacement)
        {
            isImmobile = true;
            tokenRigidbody.bodyType = RigidbodyType2D.Kinematic; // Set Rigidbody to kinematic
            Debug.Log($"{characterData.characterName} is now immobile on placement.");
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (characterData.characterType == CharacterType.Golem && isImmobile && collision.gameObject.CompareTag("Token"))
        {
            Reactivate();
        }
        SoundManager.Instance?.PlayCollision();
    }

    private void Reactivate()
    {
        isImmobile = false;
        tokenRigidbody.bodyType = RigidbodyType2D.Dynamic; // Enable movement
        Debug.Log($"{characterData.characterName} has been reactivated and can now move.");
    }
    private void HandleTokenMovement()
    {
        if (tokenRigidbody.linearVelocity.magnitude < 0.01f) // Token has stopped moving
        {

            EventManager.TriggerEvent("OnTurnEnd");
            if (characterData?.ability != null)
            {
                // Validate the final position based on the token's ability
                bool isValid = characterData.ability.ValidateFinalPosition(this);
                if (!isValid)
                {
                    EliminateToken(); // Eliminate if conditions are not met
                    return; // Exit further processing
                }

                characterData.ability.OnBaseCapture(this);
            }

            ResetToken(); // Reset token for the next turn
        }
    }

    private void CheckTokenPosition()
    {
        if (tokenRigidbody.linearVelocity.magnitude < 0.01f && GameManager.Instance.currentPhase == GamePhase.GamePlay)
        {
            if (characterData?.ability != null && characterData.characterType != CharacterType.Mermaid)
            {
                bool isValid = characterData.ability.ValidateFinalPosition(this);
                if (!isValid)
                {
                    EliminateToken(); // Eliminate if conditions are not met
                    return;
                }
                lastPosition = transform.position; // Update last position
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Vault")) // Only trigger if the token has stopped
        {
            Debug.Log($"{characterData.characterName} has triggered the vault");
            characterData.ability?.OnVaultInteraction(this);
        }
    }



    private void EliminateToken()
    {
        SoundManager.Instance?.PlayTokenLeaving();
        GameManager.Instance.RemoveTokenFromPlayer(owner, characterData.characterType);
        isUnlocked = false;
        Destroy(gameObject);
    }

    public void SetForce(float sliderForce)
    {
        if (!IsCurrentPlayerOwner()) return;

        float safeMass = Mathf.Max(tokenRigidbody.mass, 0.1f); // Avoid division by zero
        throwForce = sliderForce * characterData.ability.speedMultiplier / safeMass;

        //  Debug.Log($"{name} set throw force to: {throwForce} (Slider: {sliderForce}, Speed: {characterData.Speed}, Weight: {safeMass})");
    }

    public void OnTokenSelected()
    {
        if (!IsCurrentPlayerOwner())
        {
            SoundManager.Instance?.PlayWhenTapOnOpponentChip();
            return;
        }
        UIManager.Instance.ClosePlayLowerPanel();
        UIManager.Instance.OpenPlayAttackLowerPanel();

        arrow.gameObject.SetActive(true);
        StopMovement();
        Debug.Log($"{name} is selected.");
    }
    void OnDrawGizmos()
    {
        // Use the same radius as in OverlapCircle
        float radius = GetComponent<CircleCollider2D>().radius;
        int segments = 30; // Number of segments to approximate the circle

        // Set the Gizmo color
        Gizmos.color = Color.yellow;

        // Draw a 2D circle
        Vector3 position = transform.position; // Center of the circle
        float angle = 0f;

        for (int i = 0; i <= segments; i++)
        {
            // Calculate points on the circle
            float x = position.x + Mathf.Cos(angle) * radius;
            float y = position.y + Mathf.Sin(angle) * radius;

            // Calculate the next point
            float nextX = position.x + Mathf.Cos(angle + (2 * Mathf.PI / segments)) * radius;
            float nextY = position.y + Mathf.Sin(angle + (2 * Mathf.PI / segments)) * radius;

            // Draw a line between the points
            Gizmos.DrawLine(new Vector3(x, y, 0), new Vector3(nextX, nextY, 0));

            angle += (2 * Mathf.PI / segments);
        }
    }

    public void OnTokenDeselected()
    {
        Debug.LogError("OnDeselected");
        isDragging = false;

        UIManager.Instance.OpenPlayLowerPanel();
        UIManager.Instance.ClosePlayAttackLowerPanel();
        arrow.gameObject.SetActive(false);
    }

    public void StartDragging(Vector3 mouseWorldPosition)
    {
        if (isThrown || !IsCurrentPlayerOwner()) return;

        MapScroll.Instance.DisableScroll();
        isDragging = true;
        previousMousePosition = mouseWorldPosition;
    }

    public void StopDragging()
    {
        // MapScroll.Instance.EnableScroll();
        isDragging = false;
    }

    public void DragToRotate(Vector3 mouseWorldPosition)
    {
        if (!isDragging || isThrown || !IsCurrentPlayerOwner()) return;

        Vector3 directionToPrevious = previousMousePosition - transform.position;
        Vector3 directionToCurrent = mouseWorldPosition - transform.position;

        float angle = Vector3.SignedAngle(directionToPrevious, directionToCurrent, Vector3.forward);
        transform.Rotate(0, 0, angle);

        previousMousePosition = mouseWorldPosition;
    }

    public void Launch()
    {
        if (isThrown || !IsCurrentPlayerOwner()) return;
        MapScroll.Instance.StartFollowing(gameObject);
        Vector2 movementDirection = (transform.position - arrow.position).normalized;

        isThrown = true;
        tokenRigidbody.linearVelocity = Vector2.zero; // Reset velocity
        tokenRigidbody.AddForce(movementDirection * throwForce, ForceMode2D.Impulse);

        Debug.Log($"{name} launched with force: {throwForce} in direction {movementDirection}");
    }

    private void ResetToken()
    {

        isThrown = false;
        StopMovement();
        arrow.gameObject.SetActive(false);
        UIManager.Instance.OpenPlayLowerPanel();
        UIManager.Instance.ClosePlayAttackLowerPanel();
    }


    private void StopMovement()
    {
        MapScroll.Instance.StopFollowing();

        tokenRigidbody.linearVelocity = Vector2.zero;
        tokenRigidbody.angularVelocity = 0f;
        transform.rotation = Quaternion.identity;
    }

    public bool IsCurrentPlayerOwner()
    {
        if (GameManager.Instance.GetCurrentPlayer() != owner)
        {
            Debug.LogWarning($"Action denied: Player {GameManager.Instance.GetCurrentPlayer()} does not own {name}.");
            return false;
        }
        return true;
    }
}
