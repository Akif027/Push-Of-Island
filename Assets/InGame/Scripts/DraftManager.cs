using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class DraftManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform characterGrid; // Grid container for character cards
    public GameObject DraftPanel; // Main draft panel
    public GameObject SelectedCardPanel; // Panel for selected card
    public Image SelectedCardImage; // Image for selected card
    public Button CancelButton; // Button to cancel selection
    public Button ConfirmButton; // Button to confirm a character (was EliminateButton)

    public Button OkButton;
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
    public List<CharacterData> RemainingCards = new List<CharacterData>(); // Remaining cards after elimination
    public List<PlacementManager> PlacementManager = new List<PlacementManager>(); // List of placement managers
                                                                                   //  public List<CharacterData> AllCharacterCard = new List<CharacterData>(); // List of placement managers

    public GameManager gameManager;
    // public MapScroll mapScroll;

    public GameObject DisplayAllCardPanel;
    [SerializeField] private Tilemap tilemap; // Reference to your Tilemap

    private bool isSelectionLocked = false;
    Transform currentCamPostion;

    // Start Placement Phase
    public void StartPlacementPhase()
    {
        GameManager.Instance.ChangePlayerTurn(1);
        MapScroll.Instance.SmoothTransitionToPosition(player1SpawnPosition.position, 0.5f);
        currentCamPostion = player1SpawnPosition;
        LowerTilemapOpacity(0.3f);
        InstantiatePlayerTokens(player1Characters, player1SpawnPosition, 1);
        InstantiatePlayerTokens(player2Characters, player2SpawnPosition, 2);
        EventManager.Subscribe<bool>("TokenPlaced", HandleTokenPlaced);
    }

    private void InstantiatePlayerTokens(List<CharacterData> characterList, Transform spawnPosition, Int32 playerNumber)
    {
        Vector3 spawnAreaCenter = spawnPosition.position;
        float spawnRadius = 1f; // Reduced radius
        float tokenSpacing = 0.5f; // Reduced spacing for tighter grouping

        List<Vector3> usedPositions = new List<Vector3>();
        List<Token> TokensList = new List<Token>();

        foreach (var character in characterList)
        {
            Vector3 randomPosition;
            int maxAttempts = 10; // Prevent infinite loops
            int attempts = 0;

            do
            {
                // Generate random offsets within the spawn radius
                float randomX = UnityEngine.Random.Range(-spawnRadius, spawnRadius);
                float randomY = UnityEngine.Random.Range(-spawnRadius, spawnRadius);

                randomPosition = spawnAreaCenter + new Vector3(randomX, randomY, -0.6f); // Keep Z consistent
                attempts++;
            } while (IsOverlapping(randomPosition, usedPositions, tokenSpacing) && attempts < maxAttempts);

            usedPositions.Add(randomPosition);

            // Instantiate the token
            GameObject token = Instantiate(character.TokenPrefab, randomPosition, Quaternion.identity, spawnPosition);

            var placementManager = token.GetComponent<PlacementManager>();
            Token tokens = token.GetComponent<Token>();

            // Assign character data to the token
            tokens.characterData = character;
            tokens.owner = playerNumber;
            placementManager.owner = playerNumber;

            PlacementManager.Add(placementManager);
            TokensList.Add(tokens);

        }

        foreach (Token item in TokensList)
        {
            item.IsUnlocked = true;
            Debug.Log(item.IsUnlocked + " " + item);
        }

        // Create and set PlayerInfo
        PlayerInfo info = new PlayerInfo(playerNumber, TokensList);
        GameManager.Instance.SetPlayerInfo(playerNumber, info);
    }



    private bool AllTokenPlacedCheck()
    {
        foreach (PlacementManager t in PlacementManager)
        {
            if (!t.isTokenPlaced)
            {

                return false;
            }
        }
        return true;
    }

    public void LowerTilemapOpacity(float newOpacity)
    {
        if (tilemap == null)
        {
            Debug.LogWarning("Tilemap reference is missing!");
            return;
        }

        newOpacity = Mathf.Clamp(newOpacity, 0f, 1f);
        Color tilemapColor = tilemap.color;
        tilemapColor.a = newOpacity;
        tilemap.color = tilemapColor;
        Debug.Log($"Tilemap opacity set to {newOpacity}");
    }

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
        DraftCanvas.SetActive(false);
        Mapobj.SetActive(true);
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
                InitializeDraftUI();
                DraftPanel.SetActive(true);
                SelectedCardPanel.SetActive(false);
                break;

            case GamePhase.Selection:
                ChooseTextImage.SetActive(true);
                ElaminationTextImage.SetActive(false);
                GameManager.Instance.ChangePlayerTurn(1);
                DraftPanel.SetActive(true);
                SelectedCardPanel.SetActive(false);
                ChangeCurrentDraftMainICon(1);
                break;

            case GamePhase.Placement:
                TransitionToPlacementPhase();
                break;

            case GamePhase.GamePlay:
                TransitionToGameplayPhase();
                break;
        }
    }

    private void TransitionToGameplayPhase()
    {
        LowerTilemapOpacity(1f);
        Mapobj.SetActive(false);
        DraftPanel.SetActive(false);
        DraftCanvas.SetActive(true);
        DisplayAllCardPanel.SetActive(true);
        player1SpawnPosition.gameObject.SetActive(false);
        player2SpawnPosition.gameObject.SetActive(false);
        EventManager.Unsubscribe<bool>("TokenPlaced", HandleTokenPlaced);

        StartCoroutine(Delay());
    }

    IEnumerator Delay()
    {
        yield return new WaitForSeconds(3);
        Mapobj.SetActive(true);
        DraftCanvas.SetActive(false);
        DisplayAllCardPanel.SetActive(false);
        player1SpawnPosition.gameObject.SetActive(true);
        player2SpawnPosition.gameObject.SetActive(true);
    }

    private void HandleTokenPlaced(bool isPlaced)
    {

        if (isPlaced)
        {
            OkButton.gameObject.SetActive(true);
            OkButton.onClick.RemoveListener(OnTokenPlaced);
            OkButton.onClick.AddListener(OnTokenPlaced);
        }
    }
    private void OnTokenPlaced()
    {

        GameManager.Instance.ChangePlayerTurn(GameManager.Instance.GetCurrentPlayer() == 1 ? 2 : 1);
        currentCamPostion = currentCamPostion == player1SpawnPosition ? player2SpawnPosition : player1SpawnPosition;
        MapScroll.Instance.SmoothTransitionToPosition(currentCamPostion.position, 0.5f);

        if (AllTokenPlacedCheck())
        {
            Debug.Log("All tokens placed .");
            GameManager.Instance.ChangePhase(GamePhase.GamePlay);
            TransitionToPhase(GamePhase.GamePlay);
        }
        OkButton.gameObject.SetActive(false);

    }
    private void InitializeDraftUI()
    {
        ChangeCurrentDraftMainICon(2);
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

        foreach (var character in gameData.characters)
        {
            GameObject card = Instantiate(gameData.CharacterCardPrefab, characterGrid);

            InitializeCharacterCard(card, character);
            instantiatedCards.Add(card);
        }
    }

    public void ChangeCurrentDraftMainICon(Int32 player)
    {
        DraftMainIcon.sprite = GameManager.Instance.GetCurrentPlayerIcon();
    }

    public void InitializeCharacterCard(GameObject card, CharacterData character)
    {
        // Set the card's name
        card.name = character.characterName;

        // Get and set the card's image
        Image cardImage = card.GetComponent<Image>();
        if (cardImage != null)
        {
            cardImage.sprite = character.characterCardSprite;
        }
        else
        {
            Debug.LogError($"No Image component found on: {card.name}");
        }

        // Get the button component from the card
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
        if (GameManager.Instance.currentPhase == GamePhase.Selection && isSelectionLocked) return;

        selectedCharacter = character;
        selectedCardObject = card;

        Debug.Log($"Selected character: {character.characterName}");

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
        }
        else if (GameManager.Instance.currentPhase == GamePhase.Selection)
        {
            isSelectionLocked = true;
            ConfirmButton.onClick.AddListener(() => OnConfirmCharacterSelection());
        }
    }

    private void OnCancelSelection()
    {
        isSelectionLocked = false;
        Debug.Log("Selection canceled.");
        SelectedCardPanel.SetActive(false);
        DraftPanel.SetActive(true);
    }

    private void OnEliminateSelection()
    {
        if (selectedCardObject != null && GameManager.Instance.currentPhase == GamePhase.Elimination)
        {
            Debug.Log($"Eliminated  character: {selectedCharacter.characterName}");

            instantiatedCards.Remove(selectedCardObject);
            Destroy(selectedCardObject);

            selectedCharacter = null;
            selectedCardObject = null;
            GameManager.Instance.ChangePhase(GamePhase.Selection);
            TransitionToPhase(GamePhase.Selection);

            SelectedCardPanel.SetActive(false);
            DraftPanel.SetActive(true);
        }
    }

    private void OnConfirmCharacterSelection()
    {
        if (selectedCharacter == null || selectedCardObject == null) return;

        if (GameManager.Instance.GetCurrentPlayer() == 1)
        {
            player1Characters.Add(selectedCharacter);
        }
        else if (GameManager.Instance.GetCurrentPlayer() == 2)
        {
            player2Characters.Add(selectedCharacter);
        }

        GameObject duplicateCard = Instantiate(selectedCardObject, DraftPanel.transform);
        duplicateCard.transform.position = selectedCardObject.transform.position;
        duplicateCard.transform.localScale = selectedCardObject.transform.localScale;

        instantiatedCards.Remove(selectedCardObject);
        Destroy(selectedCardObject);

        DoTweenHelper.FlyToTarget(
            duplicateCard,
            DraftMainIcon.transform,
            1.5f,
            1.5f,
            () =>
            {
                isSelectionLocked = false;
                if (instantiatedCards.Count == 1)
                {
                    AssignLastCard();
                    return;
                }

                GameManager.Instance.ChangePlayerTurn(GameManager.Instance.GetCurrentPlayer() == 1 ? 2 : 1);
                ChangeCurrentDraftMainICon(GameManager.Instance.currentPlayer);
            }
        );

        SelectedCardPanel.SetActive(false);
        DraftPanel.SetActive(true);
        selectedCharacter = null;
        selectedCardObject = null;
    }

    private void AssignLastCard()
    {
        if (instantiatedCards.Count != 1) return;

        GameObject lastCard = instantiatedCards[0];
        CharacterData lastCharacter = gameData.GetCharacterByName(lastCard.name);

        RemainingCards.Add(lastCharacter);

        instantiatedCards.Remove(lastCard);
        Destroy(lastCard);

        gameManager.ChangePhase(GamePhase.Placement);
        TransitionToPhase(GamePhase.Placement);

        SelectedCardPanel.SetActive(false);
    }


}
