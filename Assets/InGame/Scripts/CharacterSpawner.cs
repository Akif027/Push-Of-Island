using System.Collections.Generic;
using UnityEngine;

public class CharacterSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public List<GameObject> characterTokenPrefabs; // List of prefabs for character tokens
    public RectTransform player1SpawnArea; // RectTransform area where Player 1's tokens will be spawned
    public RectTransform player2SpawnArea; // RectTransform area where Player 2's tokens will be spawned

    private DraftManager draftManager;

    void Start()
    {
        draftManager = FindObjectOfType<DraftManager>();

        if (draftManager == null)
        {
            Debug.LogError("DraftManager is not assigned or available.");
            return;
        }

        // Spawn tokens based on characters selected in the draft
        SpawnPlayerTokens(draftManager.player1Characters, player1SpawnArea, 4);
        SpawnPlayerTokens(draftManager.player2Characters, player2SpawnArea, 4);
    }

    private void SpawnPlayerTokens(List<Character> characters, RectTransform spawnArea, int maxTokens)
    {
        if (characters == null || characters.Count == 0)
        {
            Debug.LogWarning("No characters to spawn.");
            return;
        }

        for (int i = 0; i < Mathf.Min(maxTokens, characters.Count); i++)
        {
            // Ensure the prefab exists for this character
            if (i >= characterTokenPrefabs.Count || characterTokenPrefabs[i] == null)
            {
                Debug.LogWarning($"No prefab assigned for character: {characters[i].characterName}");
                continue;
            }

            // Generate a random position within the RectTransform area
            Vector3 spawnPosition = new Vector3(
                Random.Range(spawnArea.rect.xMin, spawnArea.rect.xMax) + spawnArea.position.x,
                Random.Range(spawnArea.rect.yMin, spawnArea.rect.yMax) + spawnArea.position.y,
                0f
            );

            // Instantiate the token prefab
            GameObject token = Instantiate(characterTokenPrefabs[i], spawnPosition, Quaternion.identity, spawnArea);

            // Set the token's name to the character's name
            token.name = characters[i].characterName;

            // Assign the character's sprite to the token's image
            SpriteRenderer spriteRenderer = token.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = characters[i].characterSprite;
            }
            else
            {
                Debug.LogWarning($"SpriteRenderer is missing on the token prefab for character: {characters[i].characterName}");
            }
        }
    }
}