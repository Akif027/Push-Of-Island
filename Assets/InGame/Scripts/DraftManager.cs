using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class DraftManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform characterGrid; // Grid container for character cards

    public Image SelectedCardImage; // Image for selected card
    public Button CancelButton; // Button to cancel selection
    public Button ConfirmButton; // Button to confirm a character (was EliminateButton)


    public Image DraftMainIcon;
    public GameObject ElaminationTextImage;
    public GameObject ChooseTextImage;
    public GameObject DraftCanvas;
    public GameObject Mapobj;

    public SpriteRenderer[] AllCoatOfArmsLand1;
    public SpriteRenderer[] AllCoatOfArmsLand2;



    [Header("Game Data")]
    public GameData gameData; // Reference to the GameData ScriptableObject

    private List<GameObject> instantiatedCards = new List<GameObject>(); // Track instantiated cards
    private CharacterData selectedCharacter = null;
    private GameObject selectedCardObject = null; // Reference to the selected card GameObject
    public List<CharacterData> player1Characters = new List<CharacterData>(); // List of Player 1's selected characters
    public List<CharacterData> player2Characters = new List<CharacterData>(); // List of Player 2's selected characters

    private List<PlacementManager> PlacementManager = new List<PlacementManager>(); // List of placement managers

    public GameObject WinnerFirstPlayerSelectedCard;
    public GameObject WinnerSecondPlayerSelectedCard;



    public GameObject FirstPlayerSelectedCard;
    public GameObject SecondPlayerSelectedCard;
    [SerializeField] private Tilemap tilemap; // Reference to your Tilemap

    private bool isSelectionLocked = false;


    // Start Placement Phase
    public void StartPlacementPhase()
    {
        GameManager.Instance.ChangePlayerTurn(1);
        MapScroll.Instance.SmoothTransitionToPosition(GameManager.Instance.spawnTokenPositionPlayer1.position, 0.5f);

        LowerTilemapOpacity(0.3f);
        InstantiateAllPlayerTokens(player1Characters, 1);
        InstantiateAllPlayerTokens(player2Characters, 2);

        EventManager.Subscribe<PlacementManager>("TokenPlaced", HandleTokenPlaced);
    }
    private void InstantiateAllPlayerTokens(List<CharacterData> characterList, int playerNumber)
    {
        // Determine the spawn area for the player
        Transform spawnAreaCenter = playerNumber == 1 ? GameManager.Instance.spawnTokenPositionPlayer1 : GameManager.Instance.spawnTokenPositionPlayer2;

        // Handle special spawn logic for Mermaid tokens
        Transform mermaidSpawnArea = playerNumber == 1 ? GameManager.Instance.MermaidSpawnAreaPlayer1 : GameManager.Instance.MermaidSpawnAreaPlayer2;

        // Parameters for spacing and positioning
        float spawnRadius = 1f; // Radius within which tokens are spawned
        float tokenSpacing = 0.5f; // Minimum distance between tokens

        List<Vector3> usedPositions = new List<Vector3>(); // Tracks used positions to avoid overlap
        List<Token> tokensList = new List<Token>(); // List to store tokens for this player

        foreach (CharacterData character in characterList)
        {
            // Use special spawn area for Mermaid tokens
            Transform spawnPosition = (character.characterType == CharacterType.Mermaid) ? mermaidSpawnArea : spawnAreaCenter;

            // Generate a random spawn position within the radius
            Vector3 spawnPositionFinal = GetNonOverlappingSpawnPosition(spawnPosition.position, usedPositions, spawnRadius, tokenSpacing);

            // Instantiate the token prefab
            GameObject tokenObject = Instantiate(character.TokenPrefab, spawnPositionFinal, Quaternion.identity, spawnPosition);

            // Configure the token
            Token tokenComponent = tokenObject.GetComponent<Token>();
            PlacementManager placementManager = tokenObject.GetComponent<PlacementManager>();

            if (tokenComponent != null)
            {
                tokenComponent.characterData = character;
                tokenComponent.owner = playerNumber;
                tokenComponent.IsUnlocked = true; // Unlock the token
                tokensList.Add(tokenComponent);
            }

            if (placementManager != null)
            {

                placementManager.isTokenPlaced = false; // Mark token as not placed initially
                PlacementManager.Add(placementManager);
            }

            Debug.Log($"Token for {character.characterName} instantiated for Player {playerNumber}.");
        }

        // Create and set PlayerInfo for this player
        PlayerInfo playerInfo = new PlayerInfo(playerNumber, tokensList);
        GameManager.Instance.SetPlayerInfo(playerNumber, playerInfo);

    }

    /// <summary>
    /// Generate a non-overlapping spawn position for a token.
    /// </summary>
    private Vector3 GetNonOverlappingSpawnPosition(Vector3 spawnAreaCenter, List<Vector3> usedPositions, float spawnRadius, float tokenSpacing)
    {
        Vector3 randomPosition;
        int maxAttempts = 10; // Limit the number of attempts to prevent infinite loops
        int attempts = 0;

        do
        {
            // Generate a random position within the radius
            float randomX = UnityEngine.Random.Range(-spawnRadius, spawnRadius);
            float randomY = UnityEngine.Random.Range(-spawnRadius, spawnRadius);

            randomPosition = spawnAreaCenter + new Vector3(randomX, randomY, -0.6f); // Adjust Z position
            attempts++;
        } while (IsOverlapping(randomPosition, usedPositions, tokenSpacing) && attempts < maxAttempts);

        // Add the position to the list of used positions
        usedPositions.Add(randomPosition);
        return randomPosition;
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
        SetCoatOfArmsToLand();
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
                UIManager.Instance.OpenDraftPanel();
                UIManager.Instance.CloseSelectedCardPanel();
                break;

            case GamePhase.Selection:
                ChooseTextImage.SetActive(true);
                ElaminationTextImage.SetActive(false);
                GameManager.Instance.ChangePlayerTurn(1);
                UIManager.Instance.OpenDraftPanel();
                UIManager.Instance.CloseSelectedCardPanel();
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
        GameManager.Instance.spawnTokenPositionPlayer1.gameObject.SetActive(false);
        GameManager.Instance.spawnTokenPositionPlayer2.gameObject.SetActive(false);
        GameManager.Instance.MermaidSpawnAreaPlayer1.gameObject.SetActive(false);
        GameManager.Instance.MermaidSpawnAreaPlayer2.gameObject.SetActive(false);
        LowerTilemapOpacity(1f);
        Mapobj.SetActive(false);
        UIManager.Instance.CloseDraftPanel();
        DraftCanvas.SetActive(true);
        UIManager.Instance.OpenDisplayAllCardPanel();
        DisplaySelectedCards();
        EventManager.Unsubscribe<PlacementManager>("TokenPlaced", HandleTokenPlaced);

        StartCoroutine(Delay());
    }

    IEnumerator Delay()
    {
        yield return new WaitForSeconds(3);
        Mapobj.SetActive(true);
        DraftCanvas.SetActive(false);
        UIManager.Instance.CloseDisplayAllCardPanel();
        GameManager.Instance.spawnTokenPositionPlayer1.gameObject.SetActive(true);
        GameManager.Instance.spawnTokenPositionPlayer2.gameObject.SetActive(true);
        GameManager.Instance.MermaidSpawnAreaPlayer1.gameObject.SetActive(true);
        GameManager.Instance.MermaidSpawnAreaPlayer2.gameObject.SetActive(true);
        UIManager.Instance.OpenPlayLowerPanel();
        UIManager.Instance.OpenPlayUpperPanel();
        MapScroll.Instance.SmoothTransitionToPosition(GameManager.Instance.spawnTokenPositionPlayer1.position, 0.5f);
    }

    private void DisplaySelectedCards()
    {
        if (gameData.CharacterCardPrefab == null)
        {
            Debug.LogError("CharacterCardPrefab is not assigned in GameData!");
            return;
        }

        // Clear existing child objects from FirstPlayerSelectedCard
        foreach (Transform child in FirstPlayerSelectedCard.transform)
        {
            Destroy(child.gameObject);
        }

        // Clear existing child objects from SecondPlayerSelectedCard
        foreach (Transform child in SecondPlayerSelectedCard.transform)
        {
            Destroy(child.gameObject);
        }
        // Clear existing child objects from FirstPlayerSelectedCard
        foreach (Transform child in WinnerFirstPlayerSelectedCard.transform)
        {
            Destroy(child.gameObject);
        }

        // Clear existing child objects from SecondPlayerSelectedCard
        foreach (Transform child in WinnerSecondPlayerSelectedCard.transform)
        {
            Destroy(child.gameObject);
        }
        // Instantiate new cards for Player 1
        foreach (var character in player1Characters)
        {
            GameObject card = Instantiate(gameData.CharacterCardNoDescriptionPrefab, FirstPlayerSelectedCard.transform);
            card.GetComponent<RectTransform>().sizeDelta = new Vector2(340, 500);
            InitializeCharacterCard(card, character, false);
        }

        // Instantiate new cards for Player 2
        foreach (var character in player2Characters)
        {
            GameObject card = Instantiate(gameData.CharacterCardNoDescriptionPrefab, SecondPlayerSelectedCard.transform);
            card.GetComponent<RectTransform>().sizeDelta = new Vector2(340, 500);
            InitializeCharacterCard(card, character, false);
        }

        // Instantiate new cards for Player 1
        foreach (var character in player1Characters)
        {
            GameObject card = Instantiate(gameData.CharacterCardNoDescriptionPrefab, WinnerFirstPlayerSelectedCard.transform);
            card.GetComponent<RectTransform>().sizeDelta = new Vector2(340, 500);
            InitializeCharacterCard(card, character, false);
        }

        // Instantiate new cards for Player 2
        foreach (var character in player2Characters)
        {
            GameObject card = Instantiate(gameData.CharacterCardNoDescriptionPrefab, WinnerSecondPlayerSelectedCard.transform);
            card.GetComponent<RectTransform>().sizeDelta = new Vector2(340, 500);
            InitializeCharacterCard(card, character, false);
        }
    }

    private UnityAction okButtonListener;
    private void HandleTokenPlaced(PlacementManager p)
    {
        UIManager.Instance.OkButton.gameObject.SetActive(true);

        // Remove previous listener if it exists
        if (okButtonListener != null)
        {
            UIManager.Instance.OkButton.onClick.RemoveListener(okButtonListener);
        }

        // Assign a new listener reference
        okButtonListener = () => OnTokenPlaced(p);

        // Add the listener
        UIManager.Instance.OkButton.onClick.AddListener(okButtonListener);
    }


    public void HandleSingleTokenPlaced(PlacementManager placementManager)
    {
        UIManager.Instance.OkButton.gameObject.SetActive(true);

        // Remove previous listener if it exists
        if (okButtonListener != null)
        {
            UIManager.Instance.OkButton.onClick.RemoveListener(okButtonListener);
        }

        // Assign a new listener reference
        okButtonListener = () => OnSingleTokenPlaced(placementManager);

        // Add the listener
        UIManager.Instance.OkButton.onClick.AddListener(okButtonListener);

    }

    private void OnSingleTokenPlaced(PlacementManager p)
    {
        p.ConfirmPlacement();
        SoundManager.Instance?.PlayDuringHiringChipPlaced();
        UIManager.Instance.OkButton.gameObject.SetActive(false);

        // Clean up: Remove the listener to prevent future issues
        UIManager.Instance.OkButton.onClick.RemoveListener(okButtonListener);
        okButtonListener = null;
    }
    public void SetCoatOfArmsToLand()
    {

        foreach (var item in AllCoatOfArmsLand1)
        {
            item.sprite = GameManager.Instance.GetPlayerIcon(1);
        }
        foreach (var item in AllCoatOfArmsLand2)
        {
            item.sprite = GameManager.Instance.GetPlayerIcon(2);
        }


    }
    private void OnTokenPlaced(PlacementManager p)
    {
        p.ConfirmPlacement();

        if (AllTokenPlacedCheck())
        {
            Debug.Log("All tokens placed.");
            GameManager.Instance.ChangePhase(GamePhase.GamePlay);
            TransitionToPhase(GamePhase.GamePlay);
        }
        UIManager.Instance.OkButton.gameObject.SetActive(false);

        // Clean up: Remove the listener to prevent future issues
        UIManager.Instance.OkButton.onClick.RemoveListener(okButtonListener);
        okButtonListener = null;
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

            InitializeCharacterCard(card, character, true);
            instantiatedCards.Add(card);
        }
    }

    public void ChangeCurrentDraftMainICon(Int32 player)
    {
        DraftMainIcon.sprite = GameManager.Instance.GetCurrentPlayerIcon();
    }

    public void InitializeCharacterCard(GameObject card, CharacterData character, bool CanaddListner)
    {
        // Set the card's name
        card.name = character.characterName;

        // Get and set the card's image
        Image cardImage = card.GetComponent<Image>();
        if (cardImage != null)
        {
            cardImage.sprite = CanaddListner == true ? character.characterCardSprite : character.characterCardNoDescriptionSprite;
        }
        else
        {
            Debug.LogError($"No Image component found on: {card.name}");
        }
        if (!CanaddListner) return;
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
        SoundManager.Instance?.PlayCharacterChoose();
        selectedCharacter = character;
        selectedCardObject = card;

        Debug.Log($"Selected character: {character.characterName}");

        UIManager.Instance.CloseDraftPanel();
        UIManager.Instance.OpenSelectedCardPanel();
        SelectedCardImage.sprite = character.characterCardNoDescriptionSprite;

        CancelButton.onClick.RemoveAllListeners();
        CancelButton.onClick.AddListener(() => OnCancelSelection());

        ConfirmButton.onClick.RemoveAllListeners();
        if (GameManager.Instance.currentPhase == GamePhase.Elimination)
        {
            ConfirmButton.onClick.AddListener(() => OnEliminateSelection());

        }
        else if (GameManager.Instance.currentPhase == GamePhase.Selection)
        {
            isSelectionLocked = true;
            ConfirmButton.onClick.AddListener(() => OnConfirmCharacterSelection());
        }
    }

    private void OnCancelSelection()
    {
        SoundManager.Instance?.PlayButtonTap();
        isSelectionLocked = false;
        Debug.Log("Selection canceled.");
        UIManager.Instance.CloseSelectedCardPanel();
        UIManager.Instance.OpenDraftPanel();
    }

    private void OnEliminateSelection()
    {
        if (selectedCardObject != null && GameManager.Instance.currentPhase == GamePhase.Elimination)
        {
            SoundManager.Instance?.PlayDiscardFirstCard();
            Debug.Log($"Eliminated character: {selectedCharacter.characterName}");

            instantiatedCards.Remove(selectedCardObject);
            UIManager.Instance.CloseSelectedCardPanel();
            UIManager.Instance.OpenDraftPanel();
            // Animate the card to scale to zero
            DoTweenHelper.ScaleToZero(
                selectedCardObject,
                0.5f, // Duration of the animation
                () =>
                {
                    Debug.Log($"{selectedCardObject.name} destroyed after scaling down.");
                    Destroy(selectedCardObject); // Destroy the card after animation
                    selectedCharacter = null;
                    selectedCardObject = null;

                    GameManager.Instance.ChangePhase(GamePhase.Selection);
                    TransitionToPhase(GamePhase.Selection);


                }
            );
        }
    }


    private void OnConfirmCharacterSelection()
    {
        if (selectedCharacter == null || selectedCardObject == null) return;
        SoundManager.Instance?.PlayButtonTap();
        if (GameManager.Instance.GetCurrentPlayer() == 1)
        {
            player1Characters.Add(selectedCharacter);
        }
        else if (GameManager.Instance.GetCurrentPlayer() == 2)
        {
            player2Characters.Add(selectedCharacter);
        }

        GameObject duplicateCard = Instantiate(selectedCardObject, UIManager.Instance.DraftPanel.transform);
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
                    RemoveLastCard();
                    return;
                }

                GameManager.Instance.ChangePlayerTurn(GameManager.Instance.GetCurrentPlayer() == 1 ? 2 : 1);
                ChangeCurrentDraftMainICon(GameManager.Instance.currentPlayer);
            }
        );

        UIManager.Instance.CloseSelectedCardPanel();
        UIManager.Instance.OpenDraftPanel();
        selectedCharacter = null;
        selectedCardObject = null;
    }

    private void RemoveLastCard()
    {
        if (instantiatedCards.Count != 1) return;

        GameObject lastCard = instantiatedCards[0];
        CharacterData lastCharacter = gameData.GetCharacterByName(lastCard.name);

        instantiatedCards.Remove(lastCard);

        // Animate the card to scale to zero
        DoTweenHelper.ScaleToZero(
            lastCard,
           0.5f, // Duration of the animation
            () =>
            {
                Debug.Log($"{lastCard.name} destroyed after scaling down.");
                Destroy(lastCard); // Destroy the card after animation
                GameManager.Instance.ChangePhase(GamePhase.Placement);
                TransitionToPhase(GamePhase.Placement);
                UIManager.Instance.CloseSelectedCardPanel();
            }
        );
    }


}
