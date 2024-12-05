using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameData", menuName = "Game/GameData", order = 1)]
public class GameData : ScriptableObject
{
    [Header("Character Data")]
    public List<Character> characters; // List of all character data
    public GameObject CharacterCardPrefab;


    /// <summary>
    /// Retrieve a character by name.
    /// </summary>
    public Character GetCharacterByName(string name)
    {
        return characters.Find(character => character.characterName == name);
    }

    /// <summary>
    /// Retrieve a character by index.
    /// </summary>
    public Character GetCharacterByIndex(int index)
    {
        if (index >= 0 && index < characters.Count)
        {
            return characters[index];
        }
        return null; // Return null if index is out of bounds
    }
}
[System.Serializable]
public class Character
{
    public string characterName; // Name of the character
    public Sprite characterSprite; // Sprite representing the character

    public Vector3 InitialPosition;
    public GameObject characterTokenPrefab; // Prefab for the character's token
}// Example enum for character types
public enum CharacterType
{
    Mermaid,
    Knight,
    Golem,
    Default
}
