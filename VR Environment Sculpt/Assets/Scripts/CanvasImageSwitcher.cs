using UnityEngine;
using UnityEngine.UI;

public class CanvasImageSwitcher : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The Image component on the separate canvas to switch")]
    public Image targetImage;

    [Header("Images per Button Group")]
    [Tooltip("Array of sprites - one for each button group")]
    public Sprite[] groupImages;

    [Header("Options")]
    [Tooltip("Hide image when no button group is active")]
    public bool hideWhenInactive = false;

    private void Start()
    {
        if (targetImage == null)
        {
            Debug.LogError("Target Image is not assigned!");
            return;
        }

        if (hideWhenInactive)
        {
            targetImage.gameObject.SetActive(false);
        }
    }

    // This method will be called by the SequentialButtonDisplay script
    public void OnGroupChanged(int groupIndex)
    {
        Debug.Log($"OnGroupChanged called with index: {groupIndex}");

        if (targetImage == null)
        {
            Debug.LogError("Target Image is not assigned!");
            return;
        }

        if (groupIndex < 0 || groupIndex >= groupImages.Length)
        {
            Debug.LogWarning($"Group index {groupIndex} is out of range for groupImages array (size: {groupImages.Length})!");
            return;
        }

        // Show the image if it was hidden
        if (!targetImage.gameObject.activeSelf)
        {
            targetImage.gameObject.SetActive(true);
        }

        // Switch to the corresponding image
        targetImage.sprite = groupImages[groupIndex];

        Debug.Log($"Switched to image for group {groupIndex}: {groupImages[groupIndex].name}");
    }

    // Optional: Method to manually switch to a specific group's image
    public void SwitchToGroup(int groupIndex)
    {
        OnGroupChanged(groupIndex);
    }

    // Optional: Hide the image
    public void HideImage()
    {
        if (targetImage != null)
        {
            targetImage.gameObject.SetActive(false);
        }
    }

    // Optional: Show the image
    public void ShowImage()
    {
        if (targetImage != null)
        {
            targetImage.gameObject.SetActive(true);
        }
    }
}