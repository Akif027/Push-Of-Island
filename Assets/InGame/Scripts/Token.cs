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
}
