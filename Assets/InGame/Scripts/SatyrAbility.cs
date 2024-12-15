using UnityEngine;

[CreateAssetMenu(fileName = "Satyr Ability", menuName = "Abilities/Satyr Ability")]
public class SatyrAbility : CharacterAbility
{
    public override void Activate(Token token)
    {
        Debug.Log($"{token.name} (Satyr) activated: Cannot be knocked off the edge and reflects off edges.");
    }

    public void HandleReflection(Token token, Vector2 boundarySize)
    {
        Rigidbody2D rb = token.GetRigidbody2D();
        if (rb == null) return;

        Vector2 position = rb.position;
        Vector2 velocity = rb.linearVelocity;

        bool reflected = false;

        // Horizontal boundary check
        if (Mathf.Abs(position.x) >= boundarySize.x / 2f)
        {
            velocity.x *= -1; // Reverse X velocity
            position.x = Mathf.Clamp(position.x, -boundarySize.x / 2f, boundarySize.x / 2f); // Clamp position
            reflected = true;
        }

        // Vertical boundary check
        if (Mathf.Abs(position.y) >= boundarySize.y / 2f)
        {
            velocity.y *= -1; // Reverse Y velocity
            position.y = Mathf.Clamp(position.y, -boundarySize.y / 2f, boundarySize.y / 2f); // Clamp position
            reflected = true;
        }

        if (reflected)
        {
            rb.linearVelocity = velocity; // Apply reflected velocity
            rb.position = position; // Update position
            Debug.Log($"{token.name} (Satyr) reflected with velocity: {velocity}");
        }
    }
}
