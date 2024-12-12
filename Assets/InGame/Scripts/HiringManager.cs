using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HiringManager : MonoBehaviour
{
    public GameObject characterCardPrefab; // Reference to the prefab for character cards
    public Transform gridLayout;          // Parent grid layout transform for the hiring panel
    public GameManager gameManager;       // Reference to the GameManager
    public GameObject hiringPanel;        // Reference to the hiring panel
    public GameObject infoPanel;          // Reference to the info panel
    public Transform infoGridLayout;      // Grid layout transform for the info panel
    public Transform playerIcon1;         // UI element for Player 1's icon in the info panel
    public Transform playerIcon2;         // UI element for Player 2's icon in the info panel
    public Transform spawnTokenPositionPlayer1; // Spawn position for Player 1 tokens
    public Transform spawnTokenPositionPlayer2; // Spawn position for Player 2 tokens
    public ScoreManager scoreManager;     // Reference to the ScoreManager

    /// <summary>
    /// Open the hiring panel and display player characters.
    /// </summary>
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

    /// <summary>
    /// Displays the player's characters as cards in the grid layout.
    /// </summary>
    public void ShowPlayerCharacters(int playerNumber)
    {
        if (characterCardPrefab == null || gridLayout == null)
        {
            Debug.LogError("Character card prefab or grid layout is not assigned!");
            return;
        }

        // Activate the hiring panel
        hiringPanel.SetActive(true);

        // Clear existing cards in the grid
        foreach (Transform child in gridLayout)
        {
            Destroy(child.gameObject);
        }

        // Fetch player characters
        List<CharacterData> playerCharacters = gameManager.GetAllTokensOfPlayer(playerNumber);

        if (playerCharacters == null || playerCharacters.Count == 0)
        {
            Debug.Log("No characters found for the player.");
            return;
        }

        // Instantiate and initialize a card for each character
        foreach (CharacterData character in playerCharacters)
        {
            GameObject card = Instantiate(characterCardPrefab, gridLayout);
            InitializeCharacterCard(card, character);
        }
    }

    /// <summary>
    /// Initializes a character card with the provided data.
    /// </summary>
    private void InitializeCharacterCard(GameObject card, CharacterData character)
    {
        if (card == null || character == null)
        {
            Debug.LogError("Card or CharacterData is null!");
            return;
        }

        // Set the card's name to the character's name
        card.name = character.characterName;

        // Assign the sprite to the card's image
        Image cardImage = card.GetComponent<Image>();
        if (cardImage != null)
        {
            cardImage.sprite = character.characterCardSprite;
        }
        else
        {
            Debug.LogError($"No Image component found on the card for character: {character.characterName}");
        }

        // Assign functionality to the button
        Button cardButton = card.GetComponent<Button>();
        if (cardButton != null)
        {
            cardButton.onClick.RemoveAllListeners();
            cardButton.onClick.AddListener(() => PurchaseCharacter(character));
        }
        else
        {
            Debug.LogError($"No Button component found on the card for character: {character.characterName}");
        }
    }

    /// <summary>
    /// Handle the purchase of a character.
    /// </summary>
    private void PurchaseCharacter(CharacterData character)
    {
        int playerNumber = gameManager.GetCurrentPlayer();
        float characterCost = character.CharacterCost; // Get the character cost

        // Check if the player has enough coins using ScoreManager
        int playerCoins = scoreManager.GetCoins(playerNumber);
        if (playerCoins < characterCost)
        {
            Debug.Log($"Player {playerNumber} does not have enough coins to purchase {character.characterName}. Required: {characterCost}, Available: {playerCoins}");
            return;
        }

        // Deduct coins from the current player
        EventManager.TriggerCoinDeduct(playerNumber, (int)characterCost);

        // Determine the correct spawn position based on the player number
        Transform spawnPosition = playerNumber == 1 ? spawnTokenPositionPlayer1 : spawnTokenPositionPlayer2;

        // Spawn the token
        gameManager.InstantiateSinglePlayerToken(character, spawnPosition, playerNumber);

        // Update player info
        gameManager.AddTokenToPlayer(playerNumber, character.characterType);

        Debug.Log($"Character {character.characterName} purchased by Player {playerNumber} for {characterCost} coins.");
    }

    /// <summary>
    /// Close the hiring panel.
    /// </summary>
    public void CloseHiringPanel()
    {
        hiringPanel.SetActive(false);
    }

    /// <summary>
    /// Open the info panel and display the next player's characters and icon.
    /// </summary>
    public void OpenInfoPanel()
    {
        if (characterCardPrefab == null || infoGridLayout == null || infoPanel == null || playerIcon1 == null || playerIcon2 == null)
        {
            Debug.LogError("Character card prefab, info grid layout, info panel, or player icon UI elements are not assigned!");
            return;
        }

        // Determine the next player's number
        int currentPlayer = gameManager.GetCurrentPlayer();
        int nextPlayer = currentPlayer == 1 ? 2 : 1;

        // Activate the info panel
        infoPanel.SetActive(true);

        // Clear existing cards in the info grid
        foreach (Transform child in infoGridLayout)
        {
            Destroy(child.gameObject);
        }

        // Update the player icons in the UI
        Sprite player1Icon = gameManager.GetPlayerIcon(1);
        Sprite player2Icon = gameManager.GetPlayerIcon(2);

        if (player1Icon != null && player2Icon != null)
        {
            Image icon1Image = playerIcon1.GetComponent<Image>();
            Image icon2Image = playerIcon2.GetComponent<Image>();

            if (icon1Image != null && icon2Image != null)
            {
                icon1Image.sprite = currentPlayer == 1 ? player2Icon : player1Icon;
                icon2Image.sprite = currentPlayer == 2 ? player1Icon : player2Icon;
            }
        }
        else
        {
            Debug.LogError("Player icons could not be found or assigned.");
        }

        // Fetch next player's characters
        List<CharacterData> nextPlayerCharacters = gameManager.GetAllTokensOfPlayer(nextPlayer);

        if (nextPlayerCharacters == null || nextPlayerCharacters.Count == 0)
        {
            Debug.Log("No characters found for the next player.");
            return;
        }

        // Instantiate and initialize a card for each character
        foreach (CharacterData character in nextPlayerCharacters)
        {
            GameObject card = Instantiate(characterCardPrefab, infoGridLayout);
            InitializeCharacterCard(card, character);
        }
    }

    /// <summary>
    /// Close the info panel.
    /// </summary>
    public void CloseInfoPanel()
    {
        infoPanel.SetActive(false);
    }
}
