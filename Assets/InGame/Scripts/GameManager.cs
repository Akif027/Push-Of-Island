using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Singleton instance for global access
    public GameData gameData; // Reference to the GameData ScriptableObject
    public DraftManager draftManager;

    public Int32 currentPlayer; // Current player's turn (1 or 2)
    [Header("Phase Management")]
    public GamePhase currentPhase; // Current phase of the game

    [Header("Player Info")]
    public Dictionary<int, Sprite> playerIcons = new Dictionary<int, Sprite>(); // Maps player numbers to their icons

    void Awake()
    {
        // Ensure only one instance of GameManager exists
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Start with the Coin Toss phase
        currentPhase = GamePhase.CoinToss;
        InitializeCoinToss();
    }
    private void InitializeCoinToss()
    {
        Debug.Log("Phase 1: Coin Toss");
        // Explicit cast to avoid method group ambiguity
        EventManager.Subscribe("TossResult", (Action<Sprite, Sprite>)OnTossResult);
    }

    private void OnTossResult(Sprite winnerSprite, Sprite loserSprite)
    {
        // Map player icons
        SetPlayerIcons(1, winnerSprite);
        SetPlayerIcons(2, loserSprite);

        // Transition to the elimination phase
        ChangePhase(GamePhase.Elimination);
        draftManager?.TransitionToPhase(GamePhase.Elimination);
        // Unsubscribe to avoid duplicate callbacks
        EventManager.Unsubscribe("TossResult", (Action<Sprite, Sprite>)OnTossResult);
    }
    public Sprite GetCurrentPlayerIcon()
    {
        return GetPlayerIcon(currentPlayer);
    }
    public void ChangePlayerTurn(int playerNumber)
    {
        if (!playerIcons.ContainsKey(playerNumber))
        {
            Debug.LogError($"Invalid player number: {playerNumber}. Must be a valid key in playerIcons");
            return;
        }

        currentPlayer = playerNumber; // Update the current player
        Debug.Log($"Player turn changed to: Player {currentPlayer}");
    }

    public void SetPlayerIcons(int playerNumber, Sprite icon)
    {
        if (playerIcons.ContainsKey(playerNumber))
        {
            playerIcons[playerNumber] = icon; // Update existing player icon
        }
        else
        {
            playerIcons.Add(playerNumber, icon); // Add new player icon
        }
    }

    public Sprite GetPlayerIcon(int playerNumber)
    {
        if (playerIcons.TryGetValue(playerNumber, out var icon))
        {
            return icon;
        }

        throw new Exception($"Player {playerNumber} not found!");
    }

    public Int32 GetCurrentPlayer()
    {
        return currentPlayer;
    }

    /// <summary>
    /// Get a character by name from the GameData.
    /// </summary>
    public CharacterData GetCharacter(string name)
    {
        if (gameData == null)
        {
            Debug.LogError("GameData is not assigned!");
            return null;
        }

        return gameData.GetCharacterByName(name);
    }

    /// <summary>
    /// Get a character by index from the GameData.
    /// </summary>
    public CharacterData GetCharacter(int index)
    {
        if (gameData == null)
        {
            Debug.LogError("GameData is not assigned!");
            return null;
        }

        return gameData.GetCharacterByIndex(index);
    }

    public void ChangePhase(GamePhase newPhase)
    {
        currentPhase = newPhase;
    }
}

public enum GamePhase
{
    CoinToss,
    Elimination,
    Selection,
    PlaceMent
}
