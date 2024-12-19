using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class HiringManager : MonoBehaviour
{
    [SerializeField] private GameObject characterCardPrefab;
    [SerializeField] private Transform gridLayout;



    [SerializeField] private Transform infoGridLayout;
    [SerializeField] private Transform playerIcon1;
    [SerializeField] private Transform playerIcon2;

    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private TMP_Text coinTextTMP;


    public void OpenHiringPanel()
    {
        ShowPlayerCharacters(GameManager.Instance.GetCurrentPlayer());
    }



    public void ShowPlayerCharacters(int playerNumber)
    {
        UIManager.Instance.OpenHiringPanel();
        foreach (Transform child in gridLayout)
        {
            Destroy(child.gameObject);
        }

        List<Token> playerCharacters = GameManager.Instance.GetPlayerTokens(playerNumber);
        if (playerCharacters == null || playerCharacters.Count == 0) return;

        foreach (Token character in playerCharacters)
        {
            GameObject card = Instantiate(characterCardPrefab, gridLayout);
            InitializeCharacterCard(card, character);
        }
    }

    private void InitializeCharacterCard(GameObject card, Token token)
    {
        card.name = token.characterData.characterName;
        Image cardImage = card.GetComponent<Image>();
        if (cardImage != null) cardImage.sprite = token.characterData.characterCardSprite;

        Button cardButton = card.GetComponent<Button>();
        if (cardButton != null)
        {
            cardButton.onClick.RemoveAllListeners();
            if (!token.IsUnlocked)
            {
                cardButton.onClick.AddListener(() => PurchaseCharacter(token.characterData));
            }
            else
            {
                cardButton.interactable = false;
            }
        }
    }

    private void PurchaseCharacter(CharacterData character)
    {
        int playerNumber = GameManager.Instance.GetCurrentPlayer();
        int playerCoins = scoreManager.GetCoins(playerNumber);

        if (playerCoins < character.CharacterCost)
        {
            Debug.Log($"Player {playerNumber} doesn't have enough coins to purchase {character.characterName}.");
            return;
        }

        EventManager.TriggerCoinDeduct(playerNumber, (int)character.CharacterCost);

        GameManager.Instance.InstantiateSinglePlayerToken(character, playerNumber);
        // gameManager.AddTokenToPlayer(playerNumber, character.characterType);
        UIManager.Instance.CloseHiringPanel();
        Debug.Log($"{character.characterName} purchased by Player {playerNumber}.");
    }

    public void OpenInfoPanel()
    {
        int currentPlayer = GameManager.Instance.GetCurrentPlayer();
        int nextPlayer = currentPlayer == 1 ? 2 : 1;
        UIManager.Instance.OpenInfoPanel();

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
        Sprite player1Icon = GameManager.Instance.GetPlayerIcon(1);
        Sprite player2Icon = GameManager.Instance.GetPlayerIcon(2);

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
        List<Token> nextPlayerCharacters = GameManager.Instance.GetPlayerTokens(nextPlayer);
        if (nextPlayerCharacters == null || nextPlayerCharacters.Count == 0) return;

        foreach (Token character in nextPlayerCharacters)
        {
            GameObject card = Instantiate(characterCardPrefab, infoGridLayout);
            InitializeOtherCharacterCards(card, character);
        }
    }
    private void InitializeOtherCharacterCards(GameObject card, Token token)
    {
        card.name = token.characterData.characterName;
        Image cardImage = card.GetComponent<Image>();
        if (cardImage != null) cardImage.sprite = token.characterData.characterCardSprite;

        Button cardButton = card.GetComponent<Button>();
        if (cardButton != null)
        {
            cardButton.onClick.RemoveAllListeners();
            if (token.IsUnlocked)
            {
                cardButton.interactable = true;
            }
            else
            {
                cardButton.interactable = false;
            }
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
        UIManager.Instance.CloseHiringPanel();
    }
    public void CloseHiringPanel()
    {
        UIManager.Instance.CloseHiringPanel();
    }
}
