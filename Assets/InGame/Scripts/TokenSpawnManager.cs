using System;
using System.Collections.Generic;
using UnityEngine;

public class TokenSpawnManager : MonoBehaviour
{
    [Header("Spawn Areas")]
    public RectTransform player1SpawnArea; // RectTransform area for Player 1 tokens
    public RectTransform player2SpawnArea; // RectTransform area for Player 2 tokens

    [Header("Token Offsets")]
    public Vector2 tokenSpacing = new Vector2(100, 0); // Horizontal and vertical spacing between tokens
    public float verticalSpacing = 50f;  // Vertical spacing between tokens (if needed)

    // Lists to store references to the spawned tokens for easy access
    private List<GameObject> player1Tokens = new List<GameObject>();
    private List<GameObject> player2Tokens = new List<GameObject>();

    // Public method to spawn tokens for both players
    public void SpawnPlayerTokens(List<Character> player1Characters, List<Character> player2Characters)
    {
        if (player1SpawnArea == null || player2SpawnArea == null)
        {
            Debug.LogError("Spawn areas are not assigned in TokenSpawnManager!");
            return;
        }

        // Clear previously spawned tokens (if any)
        ClearTokens();

        // Spawn tokens for Player 1
        SpawnTokensForPlayer(player1Characters, player1SpawnArea, player1Tokens);

        // Spawn tokens for Player 2
        SpawnTokensForPlayer(player2Characters, player2SpawnArea, player2Tokens);
    }

    // Private method to handle spawning tokens for a single player
    private void SpawnTokensForPlayer(List<Character> characters, RectTransform spawnArea, List<GameObject> tokenList)
    {
        if (characters == null || characters.Count == 0)
        {
            Debug.LogWarning("No characters provided for spawning.");
            return;
        }

        // Calculate starting position (center of spawn area)
        Vector2 spawnPosition = spawnArea.rect.center;

        // Calculate total width to position tokens centered within the area
        float totalWidth = characters.Count * tokenSpacing.x;
        spawnPosition.x -= totalWidth / 2;

        // Spawn each token for the given player
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

            // Position the token with calculated offsets
            RectTransform tokenRect = token.GetComponent<RectTransform>();
            if (tokenRect != null)
            {
                Vector2 offsetPosition = spawnPosition + new Vector2(i * tokenSpacing.x, i * verticalSpacing);
                tokenRect.anchoredPosition = offsetPosition;

                // Add the token to the list for future access
                tokenList.Add(token);
                Debug.Log($"Spawned token for {character.characterName} at {offsetPosition}");
            }
            else
            {
                Debug.LogWarning($"Token prefab for {character.characterName} is missing a RectTransform component.");
            }
        }
    }

    // Public method to access all tokens for a player
    public List<GameObject> GetPlayerTokens(int playerNumber)
    {
        if (playerNumber == 1)
        {
            return player1Tokens;
        }
        else if (playerNumber == 2)
        {
            return player2Tokens;
        }
        else
        {
            Debug.LogError("Invalid player number.");
            return null;
        }
    }

    // Public method to access a specific token by index
    public GameObject GetTokenByIndex(int playerNumber, int tokenIndex)
    {
        List<GameObject> tokenList = (playerNumber == 1) ? player1Tokens : player2Tokens;

        if (tokenIndex >= 0 && tokenIndex < tokenList.Count)
        {
            return tokenList[tokenIndex];
        }
        else
        {
            Debug.LogError("Invalid token index.");
            return null;
        }
    }

    // Method to clear previously spawned tokens
    public void ClearTokens()
    {
        // Destroy all player 1 tokens
        foreach (var token in player1Tokens)
        {
            Destroy(token);
        }
        player1Tokens.Clear();

        // Destroy all player 2 tokens
        foreach (var token in player2Tokens)
        {
            Destroy(token);
        }
        player2Tokens.Clear();
    }

    // Method to remove a specific token (e.g., when eliminating a character or token)
    public void RemoveToken(int playerNumber, int tokenIndex)
    {
        List<GameObject> tokenList = (playerNumber == 1) ? player1Tokens : player2Tokens;

        if (tokenIndex >= 0 && tokenIndex < tokenList.Count)
        {
            GameObject tokenToRemove = tokenList[tokenIndex];
            tokenList.RemoveAt(tokenIndex);
            Destroy(tokenToRemove);
            Debug.Log($"Removed token {tokenIndex} for Player {playerNumber}");
        }
        else
        {
            Debug.LogError("Invalid token index for removal.");
        }
    }
}
