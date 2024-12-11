using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameData", menuName = "Game/GameData", order = 1)]
public class GameData : ScriptableObject
{
    [Header("Character Data")]
    public List<CharacterData> characters; // List of all character data
    public GameObject CharacterCardPrefab;
    public GameObject TokenPrefab;

    /// <summary>
    /// Retrieve a character by name.
    /// </summary>
    public CharacterData GetCharacterByName(string name)
    {
        return characters.Find(character => character.characterName == name);
    }

    /// <summary>
    /// Retrieve a character by index.
    /// </summary>
    public CharacterData GetCharacterByIndex(int index)
    {
        if (index >= 0 && index < characters.Count)
        {
            return characters[index];
        }
        return null; // Return null if index is out of bounds
    }
}
[System.Serializable]
public class CharacterData
{
    public string characterName; // Name of the character
    public float CharacterCost = 10f;
    public CharacterType characterType; // Sprite representing the character
    public Sprite characterCardSprite; // Sprite representing the character
    public Sprite characterTokenSprite; // Sprite representing the character
    public Sprite invalidTokenSprite; // Prefab for invalid token
    public LayerMask Mask;

}// Example enum for character types
public enum CharacterType
{
    Mermaid,
    Knight,
    Golem,
    Dwarf,
    Gryphon,
    King,
    Thief,
    Rogue,
    Satyr,
    Enchantress,
    Default
}
