using System;
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

    public CharacterData characterData;

    [SerializeField]
    private Transform arrow; // Arrow GameObject for direction

    [SerializeField]
    private float throwForce = 10f; // Force for movement

    public Rigidbody2D tokenRigidbody;

    [SerializeField]
    private float linearDrag = 2f;

    private bool isDragging = false; // Flag to check if the token is being dragged
    private Vector3 previousMousePosition; // To track the last mouse position during drag
    private bool isThrown = false;

    private void Start()
    {
        arrow.gameObject.SetActive(false);

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

        // Set Rigidbody to Dynamic and disable gravity
        tokenRigidbody.bodyType = RigidbodyType2D.Dynamic;
        tokenRigidbody.gravityScale = 0; // Disable gravity
        tokenRigidbody.linearDamping = linearDrag; // Set linear drag
    }

    public void SetForce(float force)
    {
        throwForce = force;
    }

    public void OnTokenSelected()
    {
        arrow.gameObject.SetActive(true);
        tokenRigidbody.linearVelocity = Vector2.zero; // Stop any residual movement
        tokenRigidbody.angularVelocity = 0; // Stop any rotation
        transform.rotation = Quaternion.identity; // Reset rotation to default
        Debug.Log($"{name} is selected.");
    }

    public void OnTokenDeselected()
    {
        isDragging = false;
        arrow.gameObject.SetActive(false);
        Debug.Log($"{name} is deselected.");
    }

    public void StartDragging(Vector3 mouseWorldPosition)
    {
        if (isThrown) return;

        isDragging = true;
        previousMousePosition = mouseWorldPosition;
    }

    public void StopDragging()
    {
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
        if (!isDragging || isThrown) return;

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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the collision is with another token
        Token otherToken = collision.collider.GetComponent<Token>();
        if (otherToken != null)
        {
            Debug.Log($"{name} collided with {otherToken.name}.");
            // Physics is handled automatically since both tokens are Dynamic
        }
    }

    public void Launch()
    {
        if (isThrown) return;

        Vector2 movementDirection = (transform.position - arrow.position).normalized;

        isThrown = true;

        // Apply force in the direction of the arrow
        tokenRigidbody.linearVelocity = Vector2.zero; // Reset velocity
        tokenRigidbody.AddForce(movementDirection * throwForce, ForceMode2D.Impulse);

        Debug.Log($"{name} launched in direction {movementDirection}.");
    }
}
