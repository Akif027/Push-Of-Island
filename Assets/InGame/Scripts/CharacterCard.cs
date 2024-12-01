using UnityEngine;
using UnityEngine.UI;

public class CharacterCard : MonoBehaviour
{
    public string characterName;
    public Sprite characterImage;
    public GameObject prefab; // Reference to the card's prefab

    private Image cardImage;
    private bool isHighlighted = false;

    void Start()
    {
        cardImage = GetComponent<Image>();
        cardImage.sprite = characterImage;
    }

    public void SetHighlight(bool highlight)
    {
        isHighlighted = highlight;
        cardImage.color = highlight ? Color.yellow : Color.white; // Change color to indicate selection
    }
}
