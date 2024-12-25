using UnityEngine;

[CreateAssetMenu(fileName = "Satyr Ability", menuName = "Abilities/Satyr Ability")]
public class SatyrAbility : CharacterAbility
{
    public override void Activate(Token token)
    {
        Debug.Log($"{token.name} (Satyr) activated: Cannot be knocked off the edge and reflects off edges.");
    }


}
