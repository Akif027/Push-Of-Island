using System;
using System.Collections.Generic;
using UnityEngine;

public class TokenSpawnManager : MonoBehaviour
{
    [Header("Spawn Areas")]
    public RectTransform player1SpawnArea; // RectTransform area for Player 1 tokens
    public RectTransform player2SpawnArea; // RectTransform area for Player 2 tokens

    [Header("Token Offsets")]
    public Vector2 tokenOffset = new Vector2(100, 0); // Offset between tokens for layout

    /// <summary>
    /// Spawns tokens for both players based on their selected characters.
    /// </summary>
    /// <param name="player1Characters">List of Player 1's selected characters</param>
    /// <param name="player2Characters">List of Player 2's selected characters</param>
    public void SpawnPlayerTokens(List<Character> player1Characters, List<Character> player2Characters)
    {
        if (player1SpawnArea == null || player2SpawnArea == null)
        {
            Debug.LogError("Spawn areas are not assigned in TokenSpawnManager!");
            return;
        }

        // Spawn tokens for Player 1
        SpawnTokensForPlayer(player1Characters, player1SpawnArea);

        // Spawn tokens for Player 2
        SpawnTokensForPlayer(player2Characters, player2SpawnArea);
    }

    /// <summary>
    /// Spawns tokens for a specific player in the given spawn area.
    /// </summary>
    /// <param name="characters">List of characters to spawn tokens for</param>
    /// <param name="spawnArea">The RectTransform area where tokens will be spawned</param>
    private void SpawnTokensForPlayer(List<Character> characters, RectTransform spawnArea)
    {
        if (characters == null || characters.Count == 0)
        {
            Debug.LogWarning("No characters provided for spawning.");
            return;
        }

        for (int i = 0; i < characters.Count; i++)
        {
            Character character = characters[i];
            if (character.characterTokenPrefab == null)
            {
                Debug.LogError($"Character {character.characterName} does not have a token prefab assigned!");
                continue;
            }

            // Instantiate the token
            GameObject token = Instantiate(character.characterTokenPrefab, spawnArea);

            // Position the token with an offset
            RectTransform tokenRect = token.GetComponent<RectTransform>();
            if (tokenRect != null)
            {
                tokenRect.anchoredPosition = new Vector2(tokenOffset.x * i, 0);
            }
            else
            {
                Debug.LogWarning($"Token prefab for {character.characterName} is missing a RectTransform component.");
            }
        }
    }
}
