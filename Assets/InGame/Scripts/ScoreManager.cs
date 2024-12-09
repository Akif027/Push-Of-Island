using UnityEngine;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    private Dictionary<int, int> playerCoins = new Dictionary<int, int>(); // Stores coin balance per player

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
    private void OnCoinAdd(int playerNumber, int coinAmount)
    {
        if (!playerCoins.ContainsKey(playerNumber))
        {
            playerCoins[playerNumber] = 0;
        }

        playerCoins[playerNumber] += coinAmount;
        UpdateCoinUI(playerNumber); // Update UI to reflect changes
    }

    /// <summary>
    /// Handles the CoinDeduct event.
    /// </summary>
    private void OnCoinDeduct(int playerNumber, int coinAmount)
    {
        if (!playerCoins.ContainsKey(playerNumber))
        {
            playerCoins[playerNumber] = 0;
        }

        playerCoins[playerNumber] = Mathf.Max(0, playerCoins[playerNumber] - coinAmount); // Ensure coins don’t go below 0
        UpdateCoinUI(playerNumber); // Update UI to reflect changes
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
    /// Updates the coin UI for a specific player.
    /// </summary>
    private void UpdateCoinUI(int playerNumber)
    {
        // Implement UI update logic here (e.g., finding a player UI component and setting coin text)
        Debug.Log($"Player {playerNumber} has {playerCoins[playerNumber]} coins.");
    }
}
