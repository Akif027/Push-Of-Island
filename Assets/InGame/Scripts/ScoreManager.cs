using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    private Dictionary<int, int> playerCoins = new Dictionary<int, int>();


    public TextMapper textMapper;

    public string coinTextFormat = "{0}"; // Customizable format string



    private void OnEnable()
    {
        EventManager.CoinAdd += OnCoinAdd;
        EventManager.CoinDeduct += OnCoinDeduct;
        EventManager.GloryPointAdd += OnGloryPointAdded;
        EventManager.GloryPointDeduct += OnGloryPointDeducted;
    }

    private void OnDisable()
    {
        EventManager.CoinAdd -= OnCoinAdd;
        EventManager.CoinDeduct -= OnCoinDeduct;
        EventManager.GloryPointAdd -= OnGloryPointAdded;
        EventManager.GloryPointDeduct -= OnGloryPointDeducted;
    }

    private void Start()
    {
        InitializePlayers();


    }

    private void InitializePlayers()
    {
        // Initialize coins for both players
        for (int playerNumber = 1; playerNumber <= 2; playerNumber++) // Adjust for more players
        {
            if (!playerCoins.ContainsKey(playerNumber))
            {
                playerCoins[playerNumber] = 30; // Starting coins
                textMapper.AddPlayerCoinsPoints(playerCoins[playerNumber]);
            }

            // Update the UI for all players

        }
    }

    public void OnCoinAdd(int playerNumber, int coinAmount)
    {
        SetupPlayer(playerNumber);
        playerCoins[playerNumber] += coinAmount;
        textMapper.AddPlayerCoinsPoints(playerCoins[playerNumber]);
        Debug.Log($"Player {playerNumber} added {coinAmount} coins. Total: {playerCoins[playerNumber]}.");
        // UpdateCoinUI(playerNumber);
    }

    public void OnCoinDeduct(int playerNumber, int coinAmount)
    {
        SetupPlayer(playerNumber);
        playerCoins[playerNumber] = Mathf.Max(0, playerCoins[playerNumber] - coinAmount);
        Debug.Log($"Player {playerNumber} deducted {coinAmount} coins. Total: {playerCoins[playerNumber]}.");

    }
    void OnGloryPointAdded(int playerNumber, int gloryPoints)
    {

        textMapper.AddPlayerGloryPoints(playerNumber, gloryPoints);

    }

    void OnGloryPointDeducted(int playerNumber, int gloryPoints)
    {
        SetupPlayer(playerNumber);
        playerCoins[playerNumber] = Mathf.Max(0, playerCoins[playerNumber] - gloryPoints);


    }
    public int GetCoins(int playerNumber)
    {
        return playerCoins.TryGetValue(playerNumber, out int coins) ? coins : 0;
    }


    private void SetupPlayer(int playerNumber)
    {
        if (!playerCoins.ContainsKey(playerNumber))
        {
            playerCoins[playerNumber] = 0;
        }
    }
}
