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
    public int coinsPerCaptureVault = 0; // Coins gained for capturing a vault/base
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
            EventManager.TriggerCoinAdd(token.owner, bonusCoins);
            Debug.Log($"{token.name} awarded {bonusCoins} bonus coins on placement.");
        }



    }

    /// <summary>
    /// Called when the token interacts with a vault or base.
    /// </summary>
    public virtual void OnVaultInteraction(Token token)
    {
        if (isVaultInteraction && coinsPerCaptureVault > 0)
        {
            Debug.Log("Player " + token.owner + "has recived vault coins");
            EventManager.TriggerCoinAdd(token.owner, coinsPerCaptureVault);

        }
    }
    /// <summary>
    /// Called when the token interacts with a vault or base.
    /// </summary>
    public virtual void OnBaseCapture(Token token)
    {
        if (coinsPerCaptureBase > 0)
        {
            RaycastHit2D hit = Physics2D.Raycast(token.transform.position, Vector3.back, -1, LayerMask.GetMask("Water", "Land", "Base"));
            if (hit.collider != null)
            {
                if (hit.collider.CompareTag("Base"))
                {
                    Base baseS = hit.collider.gameObject.GetComponent<Base>();

                    if (baseS == null) return;

                    if (baseS.ownerID != token.owner) // Only capture opponent bases
                    {

                        //EventManager.TriggerCoinAdd(token.owner, coinsPerCaptureBase);
                        EventManager.TriggerGloryPointAdd(token.owner, coinsPerCaptureBase);
                        Debug.Log("Player " + token.owner + "has Captured the base");
                    }

                }

            }
        }
    }

    /// <summary>
    /// Validates the token's final position based on its ability.
    /// </summary>
    public virtual bool ValidateFinalPosition(Token token)
    {
        RaycastHit2D hit = Physics2D.Raycast(token.transform.position, Vector3.back, -2, LayerMask.GetMask("Water", "Land", "Base"));

        if (hit.collider != null)
        {
            // Mermaid-specific logic
            if (token.characterData.characterType == CharacterType.Mermaid)
            {
                if (hit.collider.CompareTag("Water"))
                {
                    Debug.Log($"{token.characterData.characterName} is safely on water.");
                    return true; // Valid: Mermaid is on water
                }
                if (hit.collider.CompareTag("Land") || hit.collider.CompareTag("Base"))
                {
                    Debug.LogError($"{token.characterData.characterName} is on invalid terrain: {hit.collider.gameObject.name}");
                    return false; // Invalid: Mermaid cannot stop on land or base
                }
            }
            // General logic for other characters
            else
            {
                if (hit.collider.CompareTag("Land") || hit.collider.CompareTag("Base"))
                {
                    Debug.Log($"{token.characterData.characterName} is safely on land or base.");
                    return true; // Valid for land-based characters
                }
                if (hit.collider.CompareTag("Water"))
                {
                    Debug.LogError($"{token.characterData.characterName} cannot be on water. ");
                    return false; // Invalid for non-water characters
                }
            }
        }

        Debug.LogError($"{token.characterData.characterName} is not on any valid terrain.");
        return false; // Default invalid
    }


}
