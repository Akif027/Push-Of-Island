using UnityEngine;

[CreateAssetMenu(fileName = "New Character Ability", menuName = "Abilities/Generic Character Ability")]
public class CharacterAbility : ScriptableObject
{
    [Header("Ability Details")]
    public string abilityName;
    [TextArea] public string description;

    [Header("General Abilities")]
    public bool canMoveOnWater;

    public bool becomesImmobileOnPlacement;

    [Header("Vault Interaction")]
    public bool isVaultInteraction; // Flag for vault/base interaction

    public int coinsPerCaptureBase = 0; // Coins gained for capturing a vault/base

    [Header("Placement Bonus")]
    public bool bonusPointsOnPlaced;
    public int bonusCoins = 30; // Default bonus coins for placement

    [Header("Modifiers")]
    public float speedMultiplier = 1f;
    public float weightMultiplier = 1f;



    /// <summary>
    /// Called when the token is activated.
    /// </summary>
    public virtual void Activate(Token token)
    {
        Debug.Log($"{token.name} activated {abilityName}: {description}");

        // Bonus coins when placed
        if (bonusPointsOnPlaced && bonusCoins > 0)
        {
            EventManager.TriggerCoinAdd(token.owner, bonusCoins); SoundManager.Instance.PlayCoinCollect();
            Debug.LogError($"{token.name} awarded {bonusCoins} bonus coins on placement.");
        }

        // Handle immobility on placement for Golem
        if (becomesImmobileOnPlacement && token.characterData.characterType == CharacterType.Golem)
        {
            token.SetImmobile(); // Call the Token's method to make it immobile
            Debug.Log($"{token.characterData.characterName} becomes immobile on placement.");
        }

    }

    /// <summary>
    /// Called when the token interacts with a vault or base.
    /// </summary>
    public virtual void OnVaultInteraction(Token token)
    {
        if (GameManager.Instance.currentPhase != GamePhase.GamePlay) return;
        if (isVaultInteraction)
        {

            float radius = token.GetComponent<CircleCollider2D>().radius; // Adjust this value as needed
            LayerMask layerMask = LayerMask.GetMask("Vault", "VaultMid");

            // Use OverlapCircle to detect colliders within the specified radius
            Collider2D hitCollider = Physics2D.OverlapCircle(token.transform.position, radius, layerMask);

            if (hitCollider != null)
            {
                if (hitCollider.CompareTag("Vault"))
                {
                    EventManager.TriggerCoinAdd(token.owner, 20);
                    SoundManager.Instance.PlayCoinCollect();
                }
                else if (hitCollider.CompareTag("VaultMid"))
                {
                    EventManager.TriggerCoinAdd(token.owner, 10);
                    SoundManager.Instance.PlayCoinCollect();
                }
            }


        }
    }
    bool Basealreadycaptured = false;
    /// <summary>
    /// Called when the token interacts with a vault or base.
    /// </summary>
    public virtual void OnBaseCapture(Token token)
    {
        if (GameManager.Instance.currentPhase != GamePhase.GamePlay && token.characterData.characterType == CharacterType.Thief) return;
        if (coinsPerCaptureBase > 0)
        {

            float radius = token.GetComponent<CircleCollider2D>().radius; // Adjust based on your token's size
            LayerMask layerMask = LayerMask.GetMask("BaseIcon", "Base");

            // Use OverlapCircle to detect colliders within the specified radius
            Collider2D hitCollider = Physics2D.OverlapCircle(token.transform.position, radius, layerMask);

            if (hitCollider != null && hitCollider.CompareTag("BaseIcon"))
            {
                Base baseS = hitCollider.GetComponentInParent<Base>();

                if (baseS == null) return;

                if (baseS.ownerID != token.owner && !Basealreadycaptured) // Only capture opponent bases
                {
                    EventManager.TriggerGloryPointAdd(token.owner, coinsPerCaptureBase);
                    Basealreadycaptured = true;
                    Debug.LogError($"Player {token.owner} has captured the base.");
                    SoundManager.Instance?.PlayScore();
                }
            }
            // else if (hitCollider != null && hitCollider.CompareTag("Base") && token.characterData.characterType == CharacterType.Mermaid)
            // {

            //     token.EliminateToken();
            //     Basealreadycaptured = false;
            // }
            else
            {
                Basealreadycaptured = false;

            }
        }

    }


    public void OutOfBound(Token token, Vector2 boundarySize)
    {
        Rigidbody2D rb = token.GetRigidbody2D();
        if (rb == null) return;

        Vector2 position = rb.position;

        // Check if the token is out of the horizontal bounds
        if (Mathf.Abs(position.x) > boundarySize.x / 2f || Mathf.Abs(position.y) > boundarySize.y / 2f)
        {
            Debug.Log($"{token.name} is out of bounds and will be eliminated.");
            token.EliminateToken();
            token.ChangeTurn();
        }
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
            Debug.Log($"{token.name} (Satyr) reflected with velocity: {velocity} ");
            SoundManager.Instance?.PlayFlickTheChip();
        }
    }
    /// <summary>
    /// Validates the token's final position based on its ability.
    /// </summary>
    public virtual bool ValidateFinalPosition(Token token)
    {
        // Define the circle's radius
        float radius = token.GetComponent<CircleCollider2D>().radius; // Adjust this value as needed
        LayerMask layerMask = LayerMask.GetMask("Water", "Land", "Base", "Vault", "VaultMid");

        // Use OverlapCircle to detect colliders within the specified radius
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(token.transform.position, radius, layerMask);

        if (hitColliders.Length > 0)
        {
            foreach (Collider2D collider in hitColliders)
            {

                // Mermaid-specific logic
                if (token.characterData.characterType == CharacterType.Mermaid)
                {
                    if (collider.CompareTag("Water") || collider.CompareTag("BaseIcon") || collider.CompareTag("Vault") || collider.CompareTag("VaultMid"))
                    {
                        Debug.Log($"{token.characterData.characterName} is safely on water  . ");
                        return true;
                    }
                    if (collider.CompareTag("Land") && collider.CompareTag("Water"))
                    {
                        Debug.LogError($"{token.characterData.characterName} is on invalid terrain: {collider.gameObject.name}");
                        return true;
                    }
                    if (collider.CompareTag("Land"))
                    {
                        Debug.LogError($"{token.characterData.characterName} is on invalid terrain: {collider.gameObject.name}");
                        return false;
                    }

                }


                // General logic for other characters
                else
                {
                    if (collider.CompareTag("Land") || collider.CompareTag("Base") || collider.CompareTag("Base") && collider.CompareTag("Water") || collider.CompareTag("Land") && collider.CompareTag("Water"))
                    {
                        Debug.Log($"{token.characterData.characterName} is safely on land or base.");
                        return true; // Valid for land-based characters
                    }
                    if (collider.CompareTag("Water"))
                    {
                        Debug.LogError($"{token.characterData.characterName} cannot be on water.");
                        return false; // Invalid for non-water characters
                    }
                }
            }
        }

        Debug.LogError($"{token.characterData.characterName} is not on any valid terrain.");
        return false; // Default invalid
    }


}
