using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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
    [Header("Game Data")]
    public GameData gameData; // Reference to the GameData ScriptableObject

    private List<GameObject> instantiatedCards = new List<GameObject>(); // Track instantiated cards
    private Character selectedCharacter = null;
    private GameObject selectedCardObject = null; // Reference to the selected card GameObject
    public List<Character> player1Characters = new List<Character>(); // List of Player 1's selected characters
    public List<Character> player2Characters = new List<Character>(); // List of Player 2's selected characters
    public GameManager gameManager;
    // void Start()
    // {
    //     gameManager = GameManager.Instance;
    //     UpdateTurnIcons();
    // }
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
                ChangeCurrentDraftMainICon(1);
                Debug.Log("Phase 3: Selection");
                GameManager.Instance.ChangePlayerTurn(1);
                DraftPanel.SetActive(true);
                SelectedCardPanel.SetActive(false);
                break;
            case GamePhase.PlaceMent:
                Mapobj.SetActive(true);
                DraftCanvas.SetActive(false);
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
        DraftMainIcon.sprite = GameManager.Instance.GetPlayerInfo(Player).PlayerIcon;

    }
    private void InitializeCharacterCard(GameObject card, Character character)
    {
        card.name = character.characterName;

        Image cardImage = card.GetComponent<Image>();
        if (cardImage != null)
        {
            cardImage.sprite = character.characterSprite;
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

    private void OnCharacterClicked(Character character, GameObject card)
    {
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
            //  ConfirmButton.GetComponentInChildren<TMP_Text>().text = "Eliminate";
        }
        else if (GameManager.Instance.currentPhase == GamePhase.Selection)
        {
            ConfirmButton.onClick.AddListener(() => OnConfirmCharacterSelection());
            // ConfirmButton.GetComponentInChildren<TMP_Text>().text = "Confirm";
        }
    }

    private void OnCancelSelection()
    {
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
        // Switch to the next player's turn
        GameManager.Instance.ChangePlayerTurn(GameManager.Instance.GetCurrentPlayer() == 1 ? 2 : 1);
        ChangeCurrentDraftMainICon(GameManager.Instance.currentPlayer);
        // Get the last card and its associated character
        GameObject lastCard = instantiatedCards[0];
        Character lastCharacter = gameData.GetCharacterByName(lastCard.name);

        // Assign the last card to the current player
        if (GameManager.Instance.GetCurrentPlayer() == 1)
        {
            player1Characters.Add(lastCharacter);
        }
        else if (GameManager.Instance.GetCurrentPlayer() == 2)
        {
            player2Characters.Add(lastCharacter);
        }

        // Duplicate the last card outside the grid
        GameObject duplicateCard = Instantiate(lastCard, DraftPanel.transform);

        // Ensure duplicate card retains proper size and appearance
        RectTransform duplicateRect = duplicateCard.GetComponent<RectTransform>();
        RectTransform lastCardRect = lastCard.GetComponent<RectTransform>();

        if (duplicateRect != null && lastCardRect != null)
        {
            duplicateRect.sizeDelta = lastCardRect.sizeDelta; // Copy size
            duplicateRect.position = lastCardRect.position; // Copy position
            duplicateRect.localScale = lastCardRect.localScale; // Copy scale
        }
        else
        {
            Debug.LogError("RectTransform is missing on one of the cards.");
        }

        // Remove the last card from the grid
        instantiatedCards.Remove(lastCard);
        Destroy(lastCard);

        // Animate the duplicate card flying to the DraftMainIcon
        DoTweenHelper.FlyToTarget(
            duplicateCard,
            DraftMainIcon.transform,
            1.5f,
            1.5f,
            () =>
            {
                Debug.Log("Last card successfully animated to DraftMainIcon.");

                // Ensure duplicateCard is destroyed after animation
                Destroy(duplicateCard);
                gameManager.ChangePhase(GamePhase.PlaceMent);
                TransitionToPhase(GamePhase.PlaceMent);
                // End selection phase
                Debug.Log("Selection Phase Complete. Game setup is done!");
            }
        );

        // Reset selection UI
        SelectedCardPanel.SetActive(false);
    }
}

