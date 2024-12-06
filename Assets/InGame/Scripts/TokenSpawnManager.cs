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

        // Clear previously spawned tokens (if any)
        ClearTokens();

        // Spawn tokens for Player 1
        SpawnTokensForPlayer(player1Characters, player1SpawnArea, player1Tokens, true);

        // Spawn tokens for Player 2
        SpawnTokensForPlayer(player2Characters, player2SpawnArea, player2Tokens, false);
    }

    /// <summary>
    /// Spawns tokens for a specific player in the given spawn area.
    /// </summary>
    /// <param name="characters">List of characters to spawn tokens for</param>
    /// <param name="spawnArea">The RectTransform area where tokens will be spawned</param>
    /// <param name="tokenList">The list to store spawned tokens</param>
    /// <param name="isPlayer1">Indicates if the tokens are for Player 1</param>
    private void SpawnTokensForPlayer(List<Character> characters, RectTransform spawnArea, List<GameObject> tokenList, bool isPlayer1)
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

            // Enable the appropriate border
            EnablePlayerBorders(token, isPlayer1);
        }
    }

    /// <summary>
    /// Enables the correct border for the token based on the player's team.
    /// </summary>
    /// <param name="token">The token GameObject</param>
    /// <param name="isPlayer1">True if the token is for Player 1, otherwise false</param>
    private void EnablePlayerBorders(GameObject token, bool isPlayer1)
    {
        Transform border1 = token.transform.Find("Player1Border"); // Replace with the exact child name
        Transform border2 = token.transform.Find("Player2Border"); // Replace with the exact child name

        if (border1 != null && border2 != null)
        {
            border1.gameObject.SetActive(isPlayer1);
            border2.gameObject.SetActive(!isPlayer1);
        }
        else
        {
            Debug.LogWarning("Token prefab is missing one or both border child objects.");
        }
    }

    /// <summary>
    /// Gets the list of tokens for a specific player.
    /// </summary>
    /// <param name="playerNumber">The player number (1 or 2)</param>
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

    /// <summary>
    /// Gets a specific token by its index for the given player.
    /// </summary>
    /// <param name="playerNumber">The player number (1 or 2)</param>
    /// <param name="tokenIndex">The index of the token in the list</param>
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

    /// <summary>
    /// Clears all previously spawned tokens for both players.
    /// </summary>
    public void ClearTokens()
    {
        // Destroy all Player 1 tokens
        foreach (var token in player1Tokens)
        {
            Destroy(token);
        }
        player1Tokens.Clear();

        // Destroy all Player 2 tokens
        foreach (var token in player2Tokens)
        {
            Destroy(token);
        }
        player2Tokens.Clear();
    }

    /// <summary>
    /// Removes a specific token by its index for the given player.
    /// </summary>
    /// <param name="playerNumber">The player number (1 or 2)</param>
    /// <param name="tokenIndex">The index of the token to remove</param>
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
