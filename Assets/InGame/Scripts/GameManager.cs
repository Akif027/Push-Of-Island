using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Singleton instance for global access
    public GameData gameData; // Reference to the GameData ScriptableObject
    public DraftManager draftManager;
    public Int32 currentPlayer; // Current player's turn (1 or 2)
    [Header("Phase Management")]
    public GamePhase currentPhase; // Current phase of the game
    public PlayerInfo[] players = new PlayerInfo[2];
    void Awake()
    {
        // Ensure only one instance of GameManager exists
        if (Instance == null)
        {
            Instance = this;
            //  DontDestroyOnLoad(gameObject); // Persist GameManager across scenes
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
        EventManager.Subscribe("TossResult", OnTossResult);
    }

    private void OnTossResult(object data)
    {
        if (data is TossResult tossResult)
        {

            Debug.Log("Winner " + tossResult.WinnerSprite);
            // Pass both winner and loser sprites to the GameManager or an event
            SetPlayerInfo(tossResult.WinnerSprite, tossResult.LoserSprite);

            // Transition to the elimination phase
            ChangePhase(GamePhase.Elimination);
            if (draftManager != null)
            {
                draftManager.TransitionToPhase(GamePhase.Elimination);
            }
            else
            {
                Debug.Log("DraftManager null");
            }

            EventManager.Unsubscribe("TossResult", OnTossResult);
        }
    }

    public void ChangePlayerTurn(int player)
    {
        // Update the current player
        currentPlayer = player;

        Debug.Log($"Player Turn Changed: {currentPlayer}");
    }

    public void SetPlayerInfo(Sprite firstIcon, Sprite secondIcon)
    {
        players[0] = new PlayerInfo { PlayerIcon = firstIcon, PlayerNumber = 1 };
        players[1] = new PlayerInfo { PlayerIcon = secondIcon, PlayerNumber = 2 };
    }

    public PlayerInfo GetPlayerInfo(int playerNumber)
    {
        foreach (var player in players)
        {
            if (player.PlayerNumber == playerNumber)
                return player;
        }

        throw new Exception("Player not found!");
    }
    public Int32 GetCurrentPlayer()
    {

        return currentPlayer;

    }
    /// <summary>
    /// Get a character by name from the GameData.
    /// </summary>
    public Character GetCharacter(string name)
    {
        if (gameData == null)
        {
            Debug.LogError("GameData is not assigned!");
            return null;
        }

        return gameData.GetCharacterByName(name);
    }
    // public bool IsDragging { get; private set; }

    // public void StartDragging()
    // {
    //     IsDragging = true;
    // }

    // public void StopDragging()
    // {
    //     IsDragging = false;
    // }
    /// <summary>
    /// Get a character by index from the GameData.
    /// </summary>
    public Character GetCharacter(int index)
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
public struct PlayerInfo
{
    public int PlayerNumber;
    public Sprite PlayerIcon;

    public PlayerInfo(int playerNumber, Sprite playerIcon)
    {
        PlayerNumber = playerNumber;
        PlayerIcon = playerIcon;
    }
}
public enum GamePhase
{
    CoinToss,
    Elimination,
    Selection,
    PlaceMent
}
