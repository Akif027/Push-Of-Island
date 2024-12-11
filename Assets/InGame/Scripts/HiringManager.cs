using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HiringManager : MonoBehaviour
{
    public GameObject characterCardPrefab; // Reference to the prefab for character cards
    public Transform gridLayout;          // Parent grid layout transform
    public GameManager gameManager;       // Reference to the GameManager
    public GameObject hiringPanel;        // Reference to the hiring panel
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
        // Check if the space key is pressed
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OpenHiringPanel();
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
}
