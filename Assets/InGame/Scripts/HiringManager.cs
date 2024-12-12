using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class HiringManager : MonoBehaviour
{
    [SerializeField] private GameObject characterCardPrefab;
    [SerializeField] private Transform gridLayout;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject hiringPanel;
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private Transform infoGridLayout;
    [SerializeField] private Transform playerIcon1;
    [SerializeField] private Transform playerIcon2;
    [SerializeField] private Transform spawnTokenPositionPlayer1;
    [SerializeField] private Transform spawnTokenPositionPlayer2;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private TMP_Text coinTextTMP;

    public void OpenHiringPanel()
    {
        ShowPlayerCharacters(gameManager.GetCurrentPlayer());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OpenHiringPanel();
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            OpenInfoPanel();
            Debug.Log("Info Panel Opened.");
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            CloseInfoPanel();
            Debug.Log("Info Panel Closed.");
        }
    }

    public void ShowPlayerCharacters(int playerNumber)
    {
        hiringPanel.SetActive(true);
        foreach (Transform child in gridLayout)
        {
            Destroy(child.gameObject);
        }

        List<CharacterData> playerCharacters = gameManager.GetAllTokensOfPlayer(playerNumber);
        if (playerCharacters == null || playerCharacters.Count == 0) return;

        foreach (CharacterData character in playerCharacters)
        {
            GameObject card = Instantiate(characterCardPrefab, gridLayout);
            InitializeCharacterCard(card, character, true);
        }
    }

    private void InitializeCharacterCard(GameObject card, CharacterData character, bool enableButton)
    {
        card.name = character.characterName;
        Image cardImage = card.GetComponent<Image>();
        if (cardImage != null) cardImage.sprite = character.characterCardSprite;

        Button cardButton = card.GetComponent<Button>();
        if (cardButton != null)
        {
            cardButton.onClick.RemoveAllListeners();
            if (enableButton)
            {
                cardButton.onClick.AddListener(() => PurchaseCharacter(character));
            }
            else
            {
                Destroy(cardButton);
            }
        }
    }

    private void PurchaseCharacter(CharacterData character)
    {
        int playerNumber = gameManager.GetCurrentPlayer();
        int playerCoins = scoreManager.GetCoins(playerNumber);

        if (playerCoins < character.CharacterCost)
        {
            Debug.Log($"Player {playerNumber} doesn't have enough coins to purchase {character.characterName}.");
            return;
        }

        EventManager.TriggerCoinDeduct(playerNumber, (int)character.CharacterCost);
        Transform spawnPosition = playerNumber == 1 ? spawnTokenPositionPlayer1 : spawnTokenPositionPlayer2;
        gameManager.InstantiateSinglePlayerToken(character, spawnPosition, playerNumber);
        gameManager.AddTokenToPlayer(playerNumber, character.characterType);

        Debug.Log($"{character.characterName} purchased by Player {playerNumber}.");
    }

    public void OpenInfoPanel()
    {
        int currentPlayer = gameManager.GetCurrentPlayer();
        int nextPlayer = currentPlayer == 1 ? 2 : 1;
        infoPanel.SetActive(true);

        foreach (Transform child in infoGridLayout)
        {
            Destroy(child.gameObject);
        }

        UpdatePlayerIcons(currentPlayer, nextPlayer);
        DisplayNextPlayerCharacters(nextPlayer);
        UpdateCoinText(nextPlayer);
    }

    private void UpdatePlayerIcons(int currentPlayer, int nextPlayer)
    {
        Sprite player1Icon = gameManager.GetPlayerIcon(1);
        Sprite player2Icon = gameManager.GetPlayerIcon(2);

        Image icon1Image = playerIcon1.GetComponent<Image>();
        Image icon2Image = playerIcon2.GetComponent<Image>();

        if (icon1Image != null && icon2Image != null)
        {
            icon1Image.sprite = currentPlayer == 1 ? player2Icon : player1Icon;
            icon2Image.sprite = currentPlayer == 2 ? player1Icon : player2Icon;
        }
    }

    private void DisplayNextPlayerCharacters(int nextPlayer)
    {
        List<CharacterData> nextPlayerCharacters = gameManager.GetAllTokensOfPlayer(nextPlayer);
        if (nextPlayerCharacters == null || nextPlayerCharacters.Count == 0) return;

        foreach (CharacterData character in nextPlayerCharacters)
        {
            GameObject card = Instantiate(characterCardPrefab, infoGridLayout);
            InitializeCharacterCard(card, character, false);
        }
    }

    private void UpdateCoinText(int nextPlayer)
    {
        if (coinTextTMP != null)
        {
            int nextPlayerCoins = scoreManager.GetCoins(nextPlayer);
            coinTextTMP.text = $"{nextPlayerCoins}";
            Debug.Log($"Updated Coins: Next Player {nextPlayer} has {nextPlayerCoins} coins.");
        }
        else
        {
            Debug.LogError("Coin Text TMP reference is missing!");
        }
    }

    public void CloseInfoPanel()
    {
        infoPanel.SetActive(false);
    }
    public void CloseHiringPanel()
    {
        hiringPanel.SetActive(false);
    }
}
