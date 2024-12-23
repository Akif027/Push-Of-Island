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
    public TextMapper textMapper;
    public Transform spawnTokenPositionPlayer1;
    public Transform spawnTokenPositionPlayer2;

    public Transform MermaidSpawnAreaPlayer1;
    public Transform MermaidSpawnAreaPlayer2;


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
        EventManager.Subscribe("OnTurnEnd", HandleTurn);
    }
    void OnDisable()
    {
        EventManager.Unsubscribe("OnTurnEnd", HandleTurn);


    }
    private void HandleTurn()
    {
        incrementCurrentPlayerTurn();
        CheckAndRewardThiefVaultInteraction(); // Check for thief-vault interaction

        ChangePlayerTurn(currentPlayer == 1 ? 2 : 1);
        Debug.LogError(currentPlayer);

        MapScroll.Instance.SmoothTransitionToPosition(
            currentPlayer == 1
                ? spawnTokenPositionPlayer1.position
                : spawnTokenPositionPlayer2.position,
            0.5f);


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

        if (currentPhase == GamePhase.GamePlay || playerInfos.Count != 0)
        {
            textMapper.UpdateTurnIcon(currentPlayer);
            textMapper.UpdateTurn(GetPlayerHasTurnCount());
        }
        Debug.Log($"Player turn changed to: Player {currentPlayer}");
    }
    /// <summary>
    /// Checks if a thief token is still triggering the vault.
    /// Awards 5 coins to the player if a thief token is interacting with the vault.
    /// </summary>
    public void CheckAndRewardThiefVaultInteraction()
    {
        foreach (PlayerInfo playerInfo in playerInfos)
        {
            foreach (Token token in playerInfo.tokens)
            {
                if (token == null || token.characterData == null) continue;

                // Check if the token is a thief and is interacting with a vault
                if (token.characterData.characterType == CharacterType.Thief && IsTokenTouchingVault(token))
                {
                    EventManager.TriggerCoinAdd(token.owner, 5); // Award 5 coins
                    Debug.Log($"Player {token.owner} awarded 5 coins for a thief interacting with the vault.");
                }
            }
        }
    }

    /// <summary>
    /// Determines if a token is currently touching a vault.
    /// </summary>
    /// <param name="token">The token to check.</param>
    /// <returns>True if the token is touching a vault; otherwise, false.</returns>
    private bool IsTokenTouchingVault(Token token)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(token.transform.position, 0.1f);
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Vault"))
            {
                return true; // Token is interacting with a vault
            }
        }
        return false;
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

    public GamePhase getCurrentPhase()
    {
        return currentPhase;
    }

    public void InstantiateSinglePlayerToken(CharacterData character, int playerNumber)
    {
        Transform spawnPosition;
        if (character.characterType != CharacterType.Mermaid)
        {
            spawnPosition = playerNumber == 1 ? spawnTokenPositionPlayer1 : spawnTokenPositionPlayer2;
        }
        else
        {
            spawnPosition = playerNumber == 1 ? MermaidSpawnAreaPlayer1 : MermaidSpawnAreaPlayer2;
        }


        // Instantiate the token at the adjusted spawn position
        GameObject token = Instantiate(character.TokenPrefab, spawnPosition.position, Quaternion.identity, spawnPosition);

        // Force the Z-position after instantiation
        Vector3 enforcedPosition = new Vector3(token.transform.position.x, token.transform.position.y, 0);
        token.transform.position = enforcedPosition;

        var placementManager = token.GetComponent<PlacementManager>();
        var tokenComponent = token.GetComponent<Token>();

        if (tokenComponent != null)
        {
            tokenComponent.characterData = character;
            tokenComponent.owner = playerNumber;
        }

        if (placementManager != null)
        {
            placementManager.owner = playerNumber;
            placementManager.isTokenPlaced = true;
        }
     ;
        if (tokenComponent != null)
        {
            tokenComponent.IsUnlocked = true;
            Debug.LogError($"Token instantiated for Player {playerNumber}: {character.characterName}");
        }

        PlayerInfo playerInfo = GetPlayerInfo(playerNumber);
        if (playerInfo == null)
        {
            playerInfo = new PlayerInfo(playerNumber, new List<Token>());
            SetPlayerInfo(playerNumber, playerInfo);
        }

        playerInfo.tokens.Add(tokenComponent);

        draftManager.HandleSingleTokenPlaced(placementManager);


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


    public List<Token> GetAllCurrentTokensOfPlayer(int playerNumber)
    {
        // Get the PlayerInfo for the specified player
        PlayerInfo playerInfo = GetPlayerInfo(playerNumber);
        if (playerInfo == null)
        {
            Debug.LogError($"PlayerInfo for player {playerNumber} not found!");
            return new List<Token>(); // Return empty list if PlayerInfo doesn't exist
        }

        // Return the tokens of the specified player
        Debug.Log($"Player {playerNumber} has {playerInfo.tokens.Count} tokens.");
        return playerInfo.tokens;
    }
    public int GetPlayerHasTurnCount()
    {


        PlayerInfo playerInfo = GetPlayerInfo(currentPlayer);

        return playerInfo.HasTurn;
    }
    public bool IsTurnLimitReachedOrTokensEmpty()
    {
        // Get the PlayerInfo for the current player
        PlayerInfo playerInfo = GetPlayerInfo(currentPlayer);

        if (playerInfo == null)
        {
            Debug.LogError($"PlayerInfo for Player {currentPlayer} not found!");
            return false;
        }

        // Check if HasTurn has reached 20 or the token list is empty
        if (playerInfo.HasTurn >= 20 || playerInfo.tokens.Count == 0)
        {
            return true;
        }

        return false;
    }
    public void incrementCurrentPlayerTurn()
    {
        if (currentPhase != GamePhase.GamePlay && playerInfos != null) return;
        PlayerInfo playerInfo = GetPlayerInfo(currentPlayer);
        textMapper.UpdateTurn(playerInfo.HasTurn);
        playerInfo.HasTurn++;


    }
}

[System.Serializable]
public class PlayerInfo
{
    public Int32 PlayerNumber;
    public List<Token> tokens;
    private Dictionary<CharacterData, bool> PlayerCards = new Dictionary<CharacterData, bool>();
    [SerializeField] private int _hasTurn;

    /// <summary>
    /// Gets or sets the HasTurn value with a maximum cap of 20.
    /// </summary>
    public int HasTurn
    {
        get => _hasTurn;
        set
        {
            _hasTurn = Mathf.Clamp(value, 0, 20); // Ensure HasTurn stays between 0 and 20
            if (value > 20)
            {
                Debug.LogWarning($"Player {PlayerNumber}'s HasTurn capped at 20.");
            }
        }
    }

    public PlayerInfo(Int32 PlayerNumber_, List<Token> _tokens = null)
    {
        PlayerNumber = PlayerNumber_;
        tokens = _tokens ?? new List<Token>(); // Initialize tokens if null
        Debug.Log($"{tokens.Count} tokens initialized for Player {PlayerNumber_}");
        AddPlayerCardsData();
    }

    public Dictionary<CharacterData, bool> GetPlayerCards()
    {
        UpdatePlayerCards();
        // Debug.LogError($"playerCharacters {PlayerCards.Count} : ");
        return PlayerCards;
    }
    public void UpdatePlayerCards()
    {
        // Create a set of current CharacterData from the tokens list
        var currentCharacters = tokens.Where(token => token != null && token.characterData != null)
                                       .Select(token => token.characterData)
                                       .ToHashSet();

        // Check for removed characters and update their IsUnlocked status to false
        foreach (var character in PlayerCards.Keys.ToList()) // Use ToList to safely modify during iteration
        {
            if (!currentCharacters.Contains(character))
            {
                PlayerCards[character] = false; // Set IsUnlocked to false for removed characters
                Debug.Log($"Player {PlayerNumber}: Character {character.characterType} removed. IsUnlocked set to false.");
            }
        }

        // Add or update entries for current tokens
        foreach (var token in tokens)
        {
            if (token != null && token.characterData != null)
            {
                if (PlayerCards.ContainsKey(token.characterData))
                {
                    // Update IsUnlocked to true for existing characters
                    PlayerCards[token.characterData] = true;
                    Debug.Log($"Player {PlayerNumber}: Character {token.characterData.characterType} re-added. IsUnlocked set to true.");
                }
                else
                {
                    // Add new entries for new tokens
                    PlayerCards[token.characterData] = token.IsUnlocked;
                    Debug.Log($"Player {PlayerNumber}: New Character {token.characterData.characterType} added with IsUnlocked = {token.IsUnlocked}.");
                }
            }
        }

        Debug.Log($"Player {PlayerNumber}: PlayerCards updated. Total entries: {PlayerCards.Count}");
    }

    public void AddPlayerCardsData()
    {
        foreach (var token in tokens)
        {
            if (token != null && token.characterData != null)
            {
                // Add the characterData and its IsUnlocked status to the dictionary
                if (!PlayerCards.ContainsKey(token.characterData))
                {
                    PlayerCards[token.characterData] = token.IsUnlocked;
                    //  Debug.LogError($"Player {PlayerNumber}: Character {token.characterData.characterType} added with IsUnlocked = {token.IsUnlocked}");
                }
                else
                {
                    Debug.LogError($"Duplicate characterData detected: {token.characterData.characterType}. Skipping...");
                }
            }
            else
            {
                Debug.LogError($"Player {PlayerNumber}: Invalid token or missing CharacterData.");
            }
        }

        Debug.Log($"Player {PlayerNumber}: PlayerCards dictionary populated with {PlayerCards.Count} entries.");
    }

    // public List<CharacterData> GetAllCharacterDataOfPlayer(int playerNumber)
    // {
    //     List<CharacterData> playerTokens = new List<CharacterData>();


    // }
}

public enum GamePhase
{
    CoinToss,
    Elimination,
    Selection,
    Placement,
    GamePlay
}
