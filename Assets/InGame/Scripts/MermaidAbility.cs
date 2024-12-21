using UnityEngine;

[CreateAssetMenu(fileName = "Mermaid Ability", menuName = "Abilities/Mermaid Ability")]
public class MermaidAbility : CharacterAbility
{
    public override bool ValidateFinalPosition(Token token)
    {
        RaycastHit2D hit = Physics2D.Raycast(token.transform.position, Vector3.back, -2, LayerMask.GetMask("Water", "Land", "Base"));
        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Water"))
            {
                Debug.Log($"{token.characterData.characterName} is safely on water.");
                return true; // Valid: Mermaid is on water
            }
            if (hit.collider.CompareTag("Land") || hit.collider.CompareTag("Base"))
            {
                Debug.Log($"{token.characterData.characterName} stopped on invalid terrain: {hit.collider.gameObject.name}");
                return false; // Invalid: Mermaid cannot stop on land or base
            }
        }
        Debug.Log($"{token.characterData.characterName} is not on a valid surface and will be eliminated.");
        return false;
    }




}
