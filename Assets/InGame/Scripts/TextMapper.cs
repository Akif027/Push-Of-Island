using System;
using UnityEngine;
using UnityEngine.UI;

public class TextMapper : MonoBehaviour
{

    [Header("Number Sprites")]
    public Sprite[] numberSprites; // Array of sprites for 0-9.

    [Header("Players GloryPoints")]
    public Transform Player1container; // Parent with child Images (Horizontal Layout Group)
    public Transform Player2container; // Parent with child Images (Horizontal Layout Group)
    [Header("Turn")]
    public Transform TurnContainer; // Parent with child Images (Horizontal Layout Group)
    public GameObject Player1CoatOfArm;
    public GameObject Player2CoatOfArm;

    void Start()
    {
        UpdateTurn(1, 0);
        UpdateTurnIcon(1);
        AddPlayerGloryPoints(1, 0);
        AddPlayerGloryPoints(2, 0);

    }
    public void AddPlayerGloryPoints(Int32 Playerno, int Amount)
    {
        UpdateNumberImages(Amount, Playerno == 1 ? Player1container : Player2container);

    }
    public void UpdateTurnIcon(Int32 Playerno)
    {


        if (Playerno == 1)
        {
            Player1CoatOfArm.SetActive(true);
            Player2CoatOfArm.SetActive(false);
        }
        else
        {
            Player2CoatOfArm.SetActive(true);
            Player1CoatOfArm.SetActive(false);
        }
    }
    public void UpdateTurn(Int32 Playerno, int Amount)
    {

        UpdateNumberImages(Amount, TurnContainer);

    }
    /// <summary>
    /// Updates the images in the container to display the integer as individual digits.
    /// </summary>
    /// <param name="number">The integer to display.</param>
    public void UpdateNumberImages(int number, Transform Cntainer)
    {
        // Convert the number to a string to process individual digits
        string numberString = number.ToString();

        // Ensure we have enough child Image components to replace
        for (int i = 0; i < Cntainer.childCount; i++)
        {
            Transform child = Cntainer.GetChild(i);
            Image childImage = child.GetComponent<Image>();

            if (i < numberString.Length)
            {
                // Get the digit and map it to the corresponding sprite
                int digit = numberString[i] - '0';
                childImage.sprite = numberSprites[digit];
                childImage.enabled = true; // Enable the image
            }
            else
            {
                // If there are extra images, disable them
                childImage.enabled = false;
            }
        }
    }
}
