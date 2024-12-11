using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ForceSlider : MonoBehaviour
{


    public float maxForce = 100f; // Maximum force value
    public Slider slider;         // Reference to the slider
    public GameObject fillParent; // Parent object containing the Fill children
    public Sprite[] fillSprites;  // Array of 12 sprites for each fill step
    public Sprite emptySprite;    // Sprite for empty shells

    private Image[] fillImages;

    void Start()
    {
        // Get all Image components under the parent
        fillImages = fillParent.GetComponentsInChildren<Image>();

        // Add a listener to the slider to update on value change
        slider.onValueChanged.AddListener(UpdateFill);
    }

    void UpdateFill(float value)
    {
        int intValue = Mathf.FloorToInt(value); // Get integer value of slider
        int totalImages = fillSprites.Length;  // Number of images in the array

        for (int i = 0; i < fillImages.Length; i++)
        {
            if (i < intValue)
            {
                // Cycle through the 12 images
                int spriteIndex = i % totalImages;
                fillImages[i].sprite = fillSprites[spriteIndex];
            }
            else
            {
                fillImages[i].sprite = emptySprite; // Empty state
            }
        }
    }

    public float getForce()
    {

        int totalShells = fillImages.Length; // Total number of shells
        int filledShells = Mathf.FloorToInt(slider.value); // Number of filled shells based on slider value

        // Calculate the force as a percentage of filled shells
        float force = (filledShells / (float)totalShells) * maxForce;

        return force;
    }
}

