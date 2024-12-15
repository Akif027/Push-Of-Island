using UnityEngine;

[CreateAssetMenu(fileName = "Mermaid Ability", menuName = "Abilities/Mermaid Ability")]
public class MermaidAbility : CharacterAbility
{
    public override bool ValidateFinalPosition(Token token)
    {
        // Check for the surface below the Mermaid
        RaycastHit2D hit = Physics2D.Raycast(token.transform.position, Vector3.back, -1, LayerMask.GetMask("Water", "Land", "Base"));
        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Water"))
            {
                Debug.Log($"{token.characterData.characterName} is safely on water.");
                return true; // Safe: Mermaid is allowed to stay on water
            }

            if (hit.collider.CompareTag("Land") || hit.collider.CompareTag("Base"))
            {
                Debug.Log($"{token.characterData.characterName} stopped on land or base and will be eliminated.");
                return false; // Mermaid cannot stop on land or base
            }
        }

        // Default: Mermaid is eliminated if no valid surface is found
        Debug.Log($"{token.characterData.characterName} is not on a valid surface and will be eliminated.");
        return false;
    }
}
