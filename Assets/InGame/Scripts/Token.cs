using UnityEngine;

public class Token : MonoBehaviour
{
    [SerializeField]
    private bool isUnlocked;

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

    [SerializeField]
    private Transform arrow; // Arrow GameObject for direction

    private float throwForce = 10f; // Base force for movement
    public Rigidbody2D tokenRigidbody;

    [SerializeField]
    private float linearDrag = 2f;

    private bool isDragging = false; // Flag to check if the token is being dragged
    private Vector3 previousMousePosition; // To track the last mouse position during drag
    private bool isThrown = false;

    private void Start()
    {
        arrow.gameObject.SetActive(false);
        owner = gameObject.GetComponent<PlacementManager>().owner;
        // Validate required components
        if (tokenRigidbody == null)
        {
            Debug.LogError("No Rigidbody2D component found on the Token.");
        }

        if (arrow == null)
        {
            Debug.LogError("Arrow Transform is not assigned.");
        }

        if (characterData == null)
        {
            Debug.LogError("CharacterData is not assigned.");
        }

        tokenRigidbody.gravityScale = 0; // Disable gravity
        tokenRigidbody.linearDamping = linearDrag; // Set linear drag

        // Initialize properties based on character type
        InitializeProperties();
    }

    public void InitializeProperties()
    {
        if (characterData == null) return;

        // Set weight and force modifiers from CharacterData
        tokenRigidbody.mass = characterData.Weight;
        Debug.Log($"Initialized {characterData.characterName} with Weight: {characterData.Weight}, Speed: {characterData.Speed}");
    }

    public void SetForce(float sliderForce)
    {
        if (!IsCurrentPlayerOwner()) return;

        // Calculate the adjusted force based on the player's slider input
        throwForce = sliderForce * characterData.Speed / Mathf.Max(tokenRigidbody.mass, 0.1f); // Scale with speed and weight
        Debug.Log($"{name} set throw force to: {throwForce} (Slider Force: {sliderForce}, Speed: {characterData.Speed}, Weight: {characterData.Weight})");
    }

    public void OnTokenSelected()
    {
        if (!IsCurrentPlayerOwner()) return;
        UIManager.Instance.OpenPlayAttackLowerPanel();
        arrow.gameObject.SetActive(true);
        tokenRigidbody.linearVelocity = Vector2.zero; // Stop any residual movement
        tokenRigidbody.angularVelocity = 0; // Stop any rotation
        transform.rotation = Quaternion.identity; // Reset rotation to default
        Debug.Log($"{name} is selected.");
    }

    public void OnTokenDeselected()
    {

        isDragging = false;
        UIManager.Instance.ClosePlayAttackLowerPanel();
        arrow.gameObject.SetActive(false);
        Debug.Log($"{name} is deselected.");
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
        MapScroll.Instance.EnableScroll();
        isDragging = false;
    }

    private void Update()
    {
        // Check if the token is thrown and moving
        if (isThrown)
        {
            // Check if the token's velocity is near zero
            if (tokenRigidbody.linearVelocity.magnitude < 0.1f)
            {
                ResetToken();
            }
        }
    }

    public void ResetToken()
    {
        isThrown = false; // Allow the token to be launched again
        tokenRigidbody.linearVelocity = Vector2.zero; // Stop any residual movement
        tokenRigidbody.angularVelocity = 0; // Stop any rotation
        transform.rotation = Quaternion.identity; // Reset rotation to default
        arrow.gameObject.SetActive(false); // Hide the arrow
        Debug.Log($"{name} has been reset and is ready for another throw.");
    }

    public void DragToRotate(Vector3 mouseWorldPosition)
    {
        if (!isDragging || isThrown || !IsCurrentPlayerOwner()) return;

        // Calculate the direction vectors
        Vector3 directionToPrevious = previousMousePosition - transform.position;
        Vector3 directionToCurrent = mouseWorldPosition - transform.position;

        // Calculate the angle between the two directions
        float angle = Vector3.SignedAngle(directionToPrevious, directionToCurrent, Vector3.forward);

        // Apply the rotation
        transform.Rotate(0, 0, angle);

        // Update the previous mouse position
        previousMousePosition = mouseWorldPosition;
    }

    public void Launch()
    {
        if (isThrown || !IsCurrentPlayerOwner()) return;

        Vector2 movementDirection = (transform.position - arrow.position).normalized;

        isThrown = true;

        // Apply force in the direction of the arrow
        tokenRigidbody.linearVelocity = Vector2.zero; // Reset velocity
        tokenRigidbody.AddForce(movementDirection * throwForce, ForceMode2D.Impulse);

        Debug.Log($"{name} launched with force: {throwForce} in direction {movementDirection}");
    }

    private bool IsCurrentPlayerOwner()
    {
        if (GameManager.Instance.GetCurrentPlayer() != owner)
        {
            Debug.LogWarning($"Player {GameManager.Instance.GetCurrentPlayer()} is not the owner of {name} (Owner: {owner}). Action denied.");
            return false;
        }
        return true;
    }
}
