using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    private Dictionary<int, int> playerCoins = new Dictionary<int, int>();
    public TMP_Text coinText;
    public string coinTextFormat = "{0}"; // Customizable format string
    private int currentPlayerNumber = 1;

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

    private void Start()
    {
        InitializePlayers();
    }

    private void InitializePlayers()
    {
        for (int playerNumber = 1; playerNumber <= 2; playerNumber++) // Adjust for more players
        {
            if (!playerCoins.ContainsKey(playerNumber))
            {
                playerCoins[playerNumber] = 30; // Starting coins
            }
        }

        UpdateCoinUI(currentPlayerNumber); // Show the initial coins for the current player
    }

    public void OnCoinAdd(int playerNumber, int coinAmount)
    {
        SetupPlayer(playerNumber);
        playerCoins[playerNumber] += coinAmount;
        Debug.Log($"Player {playerNumber} added {coinAmount} coins. Total: {playerCoins[playerNumber]}.");
        UpdateCoinUI(playerNumber);
    }

    public void OnCoinDeduct(int playerNumber, int coinAmount)
    {
        SetupPlayer(playerNumber);
        playerCoins[playerNumber] = Mathf.Max(0, playerCoins[playerNumber] - coinAmount);
        Debug.Log($"Player {playerNumber} deducted {coinAmount} coins. Total: {playerCoins[playerNumber]}.");
        UpdateCoinUI(playerNumber);
    }

    public int GetCoins(int playerNumber)
    {
        return playerCoins.TryGetValue(playerNumber, out int coins) ? coins : 0;
    }

    public void UpdateCoinUI(int playerNumber)
    {
        if (playerNumber == currentPlayerNumber && coinText != null)
        {
            coinText.text = string.Format(coinTextFormat, playerCoins[playerNumber]);
        }
    }

    public void SetCurrentPlayer(int playerNumber)
    {
        currentPlayerNumber = playerNumber;
        if (coinText != null)
        {
            coinText.text = string.Format(coinTextFormat, GetCoins(playerNumber));
        }
    }

    private void SetupPlayer(int playerNumber)
    {
        if (!playerCoins.ContainsKey(playerNumber))
        {
            playerCoins[playerNumber] = 0;
        }
    }
}
