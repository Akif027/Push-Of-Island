using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    private Dictionary<int, int> playerCoins = new Dictionary<int, int>();

    public TMP_Text coinText;
    public TextMapper textMapper;

    public string coinTextFormat = "{0}"; // Customizable format string
    private int currentPlayerNumber = 1;


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
            }

            // Update the UI for all players
            UpdateCoinUI(playerNumber);
        }
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
    void OnGloryPointAdded(int playerNumber, int gloryPoints)
    {

        textMapper.AddPlayerGloryPoints(playerNumber, gloryPoints);

    }

    void OnGloryPointDeducted(int playerNumber, int gloryPoints)
    {
        SetupPlayer(playerNumber);
        playerCoins[playerNumber] = Mathf.Max(0, playerCoins[playerNumber] - gloryPoints);

        UpdateCoinUI(playerNumber);
    }
    public int GetCoins(int playerNumber)
    {
        return playerCoins.TryGetValue(playerNumber, out int coins) ? coins : 0;
    }

    public void UpdateCoinUI(int playerNumber)
    {
        // Update UI only for the current player
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
