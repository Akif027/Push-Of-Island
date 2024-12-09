using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class DraftManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform characterGrid; // Grid container for character cards
    //public TMP_Text playerTurnIndicator; // Displays the active player's turn
    public GameObject DraftPanel; // Main draft panel
    public GameObject SelectedCardPanel; // Panel for selected card
    public Image SelectedCardImage; // Image for selected card
    public Button CancelButton; // Button to cancel selection
    public Button ConfirmButton; // Button to confirm a character (was EliminateButton)

    public Image DraftMainIcon;
    public GameObject ElaminationTextImage;
    public GameObject ChooseTextImage;
    public GameObject DraftCanvas;
    public GameObject Mapobj;

    [Header("Spawn Positions")]
    public Transform player1SpawnPosition; // Spawn position for Player 1
    public Transform player2SpawnPosition; // Spawn position for Player 2
    [Header("Game Data")]
    public GameData gameData; // Reference to the GameData ScriptableObject

    private List<GameObject> instantiatedCards = new List<GameObject>(); // Track instantiated cards
    private CharacterData selectedCharacter = null;
    private GameObject selectedCardObject = null; // Reference to the selected card GameObject
    public List<CharacterData> player1Characters = new List<CharacterData>(); // List of Player 1's selected characters
    public List<CharacterData> player2Characters = new List<CharacterData>(); // List of Player 2's selected characters
    public List<CharacterData> RemainingCards = new List<CharacterData>(); // List of Player 2's selected characters
    public GameManager gameManager;

    [SerializeField] private Tilemap tilemap; // Reference to your Tilemap
    private bool isSelectionLocked = false;
    // Start Placement Phase
    public void StartPlacementPhase()
    {
        GameManager.Instance.ChangePlayerTurn(1);
        LowerTilemapOpacity(0.3f);
        InstantiatePlayerTokens(player1Characters, player1SpawnPosition, 1);
        InstantiatePlayerTokens(player2Characters, player2SpawnPosition, 2);
    }

    private void InstantiatePlayerTokens(List<CharacterData> characterList, Transform spawnPosition, int playerNumber)
    {
        Vector3 spawnAreaCenter = spawnPosition.position;
        float spawnRadius = 0.5f; // Radius around the spawn position
        float tokenSpacing = 1f; // Minimum spacing between tokens

        List<Vector3> usedPositions = new List<Vector3>();

        foreach (var character in characterList)
        {
            Vector3 randomPosition;
            int maxAttempts = 10; // Prevent infinite loops
            int attempts = 0;

            // Find a random position that doesn't overlap
            do
            {
                float randomX = UnityEngine.Random.Range(-spawnRadius, spawnRadius);
                float randomY = UnityEngine.Random.Range(-spawnRadius, spawnRadius);


                randomPosition = spawnAreaCenter + new Vector3(randomX, randomY, -0.6f);
                attempts++;
            } while (IsOverlapping(randomPosition, usedPositions, tokenSpacing) && attempts < maxAttempts);

            // Add the position to the list of used positions
            usedPositions.Add(randomPosition);

            // Instantiate the token
            GameObject token = Instantiate(gameData.TokenPrefab, randomPosition, Quaternion.identity);

            // Assign PlacementManager values
            var placementManager = token.GetComponent<PlacementManager>();
            if (placementManager != null)
            {
                placementManager.characterType = character.characterType;
                placementManager.InitialPosition = randomPosition;
                placementManager.InvalidValidToken = character.invalidTokenSprite;
                placementManager.ValidToken = character.characterTokenSprite;
                placementManager.raycastMask = character.Mask;
            }
        }
    }
    public void LowerTilemapOpacity(float newOpacity)
    {
        if (tilemap == null)
        {
            Debug.LogWarning("Tilemap reference is missing! ");
            return;
        }

        // Clamp the opacity value between 0 (fully transparent) and 1 (fully opaque)
        newOpacity = Mathf.Clamp(newOpacity, 0f, 1f);

        // Get the current color of the tilemap
        Color tilemapColor = tilemap.color;

        // Update the alpha (opacity) value
        tilemapColor.a = newOpacity;

        // Apply the updated color back to the tilemap
        tilemap.color = tilemapColor;

        Debug.Log($"Tilemap opacity set to {newOpacity}");
    }
    // Helper method to check if a position overlaps with existing tokens
    private bool IsOverlapping(Vector3 position, List<Vector3> usedPositions, float spacing)
    {
        foreach (var usedPosition in usedPositions)
        {
            if (Vector3.Distance(position, usedPosition) < spacing)
            {
                return true;
            }
        }
        return false;
    }

    private void TransitionToPlacementPhase()
    {
        DraftCanvas.SetActive(false); // Hide draft UI
        Mapobj.SetActive(true); // Show the map

        // Instantiate tokens for both players
        StartPlacementPhase();
    }

    public void TransitionToPhase(GamePhase newPhase)
    {


        switch (newPhase)
        {
            case GamePhase.Elimination:
                ChooseTextImage.SetActive(false);
                ElaminationTextImage.SetActive(true);
                GameManager.Instance.ChangePlayerTurn(2);
                Debug.Log("Phase 2: Elimination");
                InitializeDraftUI(); // Initialize grid for elimination
                DraftPanel.SetActive(true);
                SelectedCardPanel.SetActive(false);
                //  playerTurnIndicator.text = "Elimination Phase: Player 2's Turn";
                break;

            case GamePhase.Selection:
                ChooseTextImage.SetActive(true);
                ElaminationTextImage.SetActive(false);
                Debug.Log("Phase 3: Selection");
                GameManager.Instance.ChangePlayerTurn(1);
                DraftPanel.SetActive(true);
                SelectedCardPanel.SetActive(false);
                ChangeCurrentDraftMainICon(1);
                break;
            case GamePhase.PlaceMent:
                TransitionToPlacementPhase();
                break;
        }
    }

    private void InitializeDraftUI()
    {
        ChangeCurrentDraftMainICon(2);
        // Clear any existing cards
        foreach (var card in instantiatedCards)
        {
            Destroy(card);
        }
        instantiatedCards.Clear();

        if (gameData.CharacterCardPrefab == null)
        {
            Debug.LogError("CharacterCardPrefab is not assigned in GameData!");
            return;
        }

        // Populate the grid with character cards from GameData
        foreach (var character in gameData.characters)
        {
            GameObject card = Instantiate(gameData.CharacterCardPrefab, characterGrid);
            InitializeCharacterCard(card, character);
            instantiatedCards.Add(card);
        }
    }
    public void ChangeCurrentDraftMainICon(Int32 Player)
    {
        DraftMainIcon.sprite = GameManager.Instance.GetCurrentPlayerIcon();
        Debug.Log(GameManager.Instance.GetCurrentPlayerIcon());

    }
    private void InitializeCharacterCard(GameObject card, CharacterData character)
    {
        card.name = character.characterName;

        Image cardImage = card.GetComponent<Image>();
        if (cardImage != null)
        {
            cardImage.sprite = character.characterCardSprite;
        }

        Button cardButton = card.GetComponent<Button>();
        if (cardButton != null)
        {
            // Remove any existing listeners and add a new one
            cardButton.onClick.RemoveAllListeners();
            cardButton.onClick.AddListener(() => OnCharacterClicked(character, card));
        }
        else
        {
            Debug.LogError($"No Button component found on: {card.name}");
        }
    }

    private void OnCharacterClicked(CharacterData character, GameObject card)
    {
        if (GameManager.Instance.currentPhase == GamePhase.Selection && isSelectionLocked == true) return;

        selectedCharacter = character;
        selectedCardObject = card;

        Debug.Log($"Selected character: {character.characterName}");

        // Disable the draft panel and enable the selected card panel
        DraftPanel.SetActive(false);
        SelectedCardPanel.SetActive(true);
        SelectedCardImage.sprite = card.GetComponent<Image>().sprite;

        CancelButton.onClick.RemoveAllListeners();
        CancelButton.onClick.AddListener(() => OnCancelSelection());

        ConfirmButton.onClick.RemoveAllListeners();
        if (GameManager.Instance.currentPhase == GamePhase.Elimination)
        {
            ConfirmButton.onClick.AddListener(() => OnEliminateSelection());
            RemainingCards.Add(character);
            //  ConfirmButton.GetComponentInChildren<TMP_Text>().text = "Eliminate";
        }
        else if (GameManager.Instance.currentPhase == GamePhase.Selection)
        {
            isSelectionLocked = true;
            ConfirmButton.onClick.AddListener(() => OnConfirmCharacterSelection());
            // ConfirmButton.GetComponentInChildren<TMP_Text>().text = "Confirm";
        }

    }

    private void OnCancelSelection()
    {
        isSelectionLocked = false;
        Debug.Log("Selection canceled.");
        // Hide SelectedCardPanel and return to DraftPanel
        SelectedCardPanel.SetActive(false);
        DraftPanel.SetActive(true);
    }

    private void OnEliminateSelection()
    {
        if (selectedCardObject != null && GameManager.Instance.currentPhase == GamePhase.Elimination)
        {
            Debug.Log($"Eliminated character: {selectedCharacter.characterName}");

            instantiatedCards.Remove(selectedCardObject);
            Destroy(selectedCardObject);

            selectedCharacter = null;
            selectedCardObject = null;
            GameManager.Instance.ChangePhase(GamePhase.Selection);
            // Transition to the selection phase after all eliminations
            TransitionToPhase(GamePhase.Selection);

            // Return to draft panel for the next elimination
            SelectedCardPanel.SetActive(false);
            DraftPanel.SetActive(true);
        }
    }
    private void OnConfirmCharacterSelection()
    {
        if (selectedCharacter == null || selectedCardObject == null) return;

        // Save the selected character to the appropriate player's list
        if (GameManager.Instance.GetCurrentPlayer() == 1)
        {
            player1Characters.Add(selectedCharacter);
        }
        else if (GameManager.Instance.GetCurrentPlayer() == 2)
        {
            player2Characters.Add(selectedCharacter);
        }

        // Duplicate the selected card outside the grid
        GameObject duplicateCard = Instantiate(selectedCardObject, DraftPanel.transform);
        duplicateCard.transform.position = selectedCardObject.transform.position;
        duplicateCard.transform.localScale = selectedCardObject.transform.localScale;

        // Destroy the original card in the grid
        instantiatedCards.Remove(selectedCardObject);
        Destroy(selectedCardObject);

        // Animate the duplicate card flying to the DraftMainIcon
        DoTweenHelper.FlyToTarget(
            duplicateCard,            // The duplicated card
            DraftMainIcon.transform,  // The target DraftMainIcon
            1.5f,                     // Animation duration
            1.5f,                     // Scale factor (scale up during flight)
            () =>
            {
                isSelectionLocked = false;
                // Check if only one card is left
                if (instantiatedCards.Count == 1)
                {
                    AssignLastCardToCurrentPlayer();
                    return;
                }

                // Switch to the next player's turn
                GameManager.Instance.ChangePlayerTurn(GameManager.Instance.GetCurrentPlayer() == 1 ? 2 : 1);
                ChangeCurrentDraftMainICon(GameManager.Instance.currentPlayer);


            }
        );
        // Update UI for the next turn
        SelectedCardPanel.SetActive(false);
        DraftPanel.SetActive(true);
        // Reset selection
        selectedCharacter = null;
        selectedCardObject = null;
    }
    private void AssignLastCardToCurrentPlayer()
    {
        if (instantiatedCards.Count != 1) return;

        // Get the last card and its associated character
        GameObject lastCard = instantiatedCards[0];
        CharacterData lastCharacter = gameData.GetCharacterByName(lastCard.name);

        RemainingCards.Add(lastCharacter);

        // Remove the last card from the grid
        instantiatedCards.Remove(lastCard);
        Destroy(lastCard);

        gameManager.ChangePhase(GamePhase.PlaceMent);
        TransitionToPhase(GamePhase.PlaceMent);

        // Reset selection UI
        SelectedCardPanel.SetActive(false);
    }

}

