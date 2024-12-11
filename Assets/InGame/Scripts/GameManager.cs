using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
    public List<PlayerInfo> playerInfos = new List<PlayerInfo>();

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
        EventManager.Subscribe("TossResult", (Action<Sprite, Sprite>)OnTossResult);
    }

    private void OnTossResult(Sprite winnerSprite, Sprite loserSprite)
    {
        SetPlayerIcons(1, winnerSprite);
        SetPlayerIcons(2, loserSprite);

        ChangePhase(GamePhase.Elimination);
        draftManager?.TransitionToPhase(GamePhase.Elimination);

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

        currentPlayer = playerNumber;
        Debug.Log($"Player turn changed to: Player {currentPlayer}");
    }

    public void SetPlayerIcons(int playerNumber, Sprite icon)
    {
        if (playerIcons.ContainsKey(playerNumber))
        {
            playerIcons[playerNumber] = icon;
        }
        else
        {
            playerIcons.Add(playerNumber, icon);
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
    /// Set or update PlayerInfo for a specific player.
    /// </summary>
    public void SetPlayerInfo(int playerNumber, PlayerInfo playerInfo)
    {
        var existingPlayerInfo = playerInfos.Find(p => p.PlayerNumber == playerNumber);

        if (existingPlayerInfo != null)
        {
            existingPlayerInfo.tokens = playerInfo.tokens ?? new List<Token>();
            Debug.Log($"Updated PlayerInfo for Player {playerNumber}.");
        }
        else
        {
            playerInfos.Add(playerInfo);
            Debug.Log($"Added PlayerInfo for Player {playerNumber}.");
        }
    }

    /// <summary>
    /// Get PlayerInfo by player number.
    /// </summary>
    public PlayerInfo GetPlayerInfo(int playerNumber)
    {
        var playerInfo = playerInfos.Find(p => p.PlayerNumber == playerNumber);

        if (playerInfo == null)
        {
            Debug.LogError($"PlayerInfo for Player {playerNumber} not found!");
        }

        return playerInfo;
    }

    public CharacterData GetCharacter(string name)
    {
        if (gameData == null)
        {
            Debug.LogError("GameData is not assigned!");
            return null;
        }

        return gameData.GetCharacterByName(name);
    }

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


    public void InstantiateSinglePlayerToken(CharacterData character, Transform spawnPosition, int playerNumber)
    {
        Vector3 spawnAreaCenter = spawnPosition.position;
        float spawnRadius = 0.5f;

        Vector3 randomPosition = spawnAreaCenter + new Vector3(
            UnityEngine.Random.Range(-spawnRadius, spawnRadius),
            UnityEngine.Random.Range(-spawnRadius, spawnRadius),
            -0.6f
        );

        GameObject token = Instantiate(gameData.TokenPrefab, randomPosition, Quaternion.identity, spawnPosition);

        var placementManager = token.GetComponent<PlacementManager>();
        var tokenComponent = token.GetComponent<Token>();

        if (tokenComponent != null)
        {
            tokenComponent.characterData = character;
        }

        if (placementManager != null)
        {
            placementManager.characterType = character.characterType;
            placementManager.InitialPosition = randomPosition;
            placementManager.InvalidValidToken = character.invalidTokenSprite;
            placementManager.ValidToken = character.characterTokenSprite;
            placementManager.raycastMask = character.Mask;
            placementManager.owner = playerNumber;
        }

        if (tokenComponent != null)
        {
            tokenComponent.IsUnlocked = true;
            Debug.Log($"Token instantiated for Player {playerNumber}: {character.characterName}");
        }

        PlayerInfo playerInfo = GetPlayerInfo(playerNumber);
        if (playerInfo == null)
        {
            playerInfo = new PlayerInfo(playerNumber, new List<Token>());
            SetPlayerInfo(playerNumber, playerInfo);
        }

        playerInfo.tokens.Add(tokenComponent);
    }
    public void AddTokenToPlayer(int playerNumber, CharacterType characterType)
    {
        PlayerInfo playerInfo = GetPlayerInfo(playerNumber);
        if (playerInfo == null)
        {
            Debug.LogError($"PlayerInfo for player {playerNumber} not found!");
            return;
        }

        CharacterData character = gameData.characters.FirstOrDefault(c => c.characterType == characterType);
        if (character == null)
        {
            Debug.LogError($"Character with type {characterType} not found in gameData!");
            return;
        }

        Token newToken = new Token { characterData = character };
        playerInfo.tokens.Add(newToken);
        Debug.Log($"Added token of type {characterType} to Player {playerNumber}.");
    }
    public void RemoveTokenFromPlayer(int playerNumber, CharacterType characterType)
    {
        PlayerInfo playerInfo = GetPlayerInfo(playerNumber);
        if (playerInfo == null)
        {
            Debug.LogError($"PlayerInfo for player {playerNumber} not found!");
            return;
        }

        Token tokenToRemove = playerInfo.tokens.FirstOrDefault(t => t.characterData != null && t.characterData.characterType == characterType);
        if (tokenToRemove == null)
        {
            Debug.LogError($"Token with type {characterType} not found for Player {playerNumber}!");
            return;
        }

        playerInfo.tokens.Remove(tokenToRemove);
        Debug.Log($"Removed token of type {characterType} from Player {playerNumber}.");
    }
    public List<CharacterData> GetAllTokensOfPlayer(int playerNumber)
    {
        List<CharacterData> playerTokens = new List<CharacterData>();

        // Get the PlayerInfo for the specified player
        PlayerInfo playerInfo = GetPlayerInfo(playerNumber);
        if (playerInfo == null)
        {
            Debug.LogError($"PlayerInfo for player {playerNumber} not found!");
            return playerTokens; // Return empty list if PlayerInfo doesn't exist
        }

        // Extract CharacterData from each token and add it to the list
        playerTokens = playerInfo.tokens
            .Where(token => token.characterData != null) // Exclude null characterData
            .Select(token => token.characterData)
            .ToList();

        Debug.Log($"Player {playerNumber} has {playerTokens.Count} tokens.");
        return playerTokens;
    }
}

[System.Serializable]
public class PlayerInfo
{
    public Int32 PlayerNumber;
    public List<Token> tokens;

    public PlayerInfo(Int32 PlayerNumber_, List<Token> _tokens = null)
    {
        PlayerNumber = PlayerNumber_;
        tokens = _tokens ?? new List<Token>(); // Initialize tokens if null
        Debug.Log($"{tokens.Count} tokens initialized for Player {PlayerNumber_}");
    }
}

public enum GamePhase
{
    CoinToss,
    Elimination,
    Selection,
    Placement,
    GamePlay
}
