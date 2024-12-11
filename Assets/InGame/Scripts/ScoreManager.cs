using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    private Dictionary<int, int> playerCoins = new Dictionary<int, int>(); // Stores coin balance per player
    public TMP_Text coinText; // Reference to the coin UI text
    private int currentPlayerNumber = 1; // Default current player (set dynamically in your game)

    private void OnEnable()
    {
        EventManager.CoinAdd += OnCoinAdd;
        EventManager.CoinDeduct += OnCoinDeduct;
    }

    private void OnDisable()
    {
        EventManager.CoinAdd -= OnCoinAdd;
        EventManager.CoinDeduct -= OnCoinDeduct;
    }

    /// <summary>
    /// Handles the CoinAdd event.
    /// </summary>
    public void OnCoinAdd(int playerNumber, int coinAmount)
    {
        if (!playerCoins.ContainsKey(playerNumber))
        {
            playerCoins[playerNumber] = 0;
        }

        playerCoins[playerNumber] += coinAmount;
        UpdateCoinUI(playerNumber);
    }

    /// <summary>
    /// Handles the CoinDeduct event.
    /// </summary>
    public void OnCoinDeduct(int playerNumber, int coinAmount)
    {
        if (!playerCoins.ContainsKey(playerNumber))
        {
            playerCoins[playerNumber] = 0;
        }

        playerCoins[playerNumber] = Mathf.Max(0, playerCoins[playerNumber] - coinAmount); // Ensure coins don’t go below 0
        UpdateCoinUI(playerNumber);
    }

    /// <summary>
    /// Get the current coin balance of a player.
    /// </summary>
    public int GetCoins(int playerNumber)
    {
        if (playerCoins.TryGetValue(playerNumber, out int coins))
        {
            return coins;
        }
        return 0;
    }

    /// <summary>
    /// Updates the coin UI for the current player.
    /// </summary>
    public void UpdateCoinUI(int playerNumber)
    {
        if (playerNumber == currentPlayerNumber)
        {
            if (coinText != null)
            {
                coinText.text = $"Coins: {playerCoins[playerNumber]}";
            }
            else
            {
                Debug.LogError("CoinText reference is missing!");
            }
        }
    }

    /// <summary>
    /// Switch to a new current player.
    /// </summary>
    public void SetCurrentPlayer(int playerNumber)
    {
        currentPlayerNumber = playerNumber;

        // Immediately update the UI for the new current player
        if (coinText != null)
        {
            coinText.text = $"Coins: {GetCoins(playerNumber)}";
        }
    }
}
