//using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
//using static System.Net.Mime.MediaTypeNames;

public class ImageOpacityController : MonoBehaviour
{
    [Header("Target Image")]
    [Tooltip("The UI Image to control opacity")]
    public Image targetImage;

    [Header("Progression Settings")]
    [Tooltip("Button group index to start increasing opacity")]
    public int startAtGroupIndex = 4;

    [Tooltip("Initial opacity when starting (0-1)")]
    [Range(0f, 1f)]
    public float startOpacity = 0.3f;

    [Tooltip("Maximum opacity (0-1)")]
    [Range(0f, 1f)]
    public float maxOpacity = 0.95f;

    [Tooltip("How much opacity increases per button group")]
    [Range(0f, 0.3f)]
    public float opacityIncreasePerGroup = 0.15f;

    [Header("Animation")]
    [Tooltip("Speed of fade transition (higher = faster)")]
    public float fadeSpeed = 2f;

    private float targetOpacity = 0f;
    private float currentOpacity = 0f;

    void Start()
    {
        if (targetImage == null)
        {
            Debug.LogError("ImageOpacityController: Target Image is not assigned!");
            return;
        }

        // Start with transparent
        SetImageOpacity(0f);

        Debug.Log("ImageOpacityController: Initialized");
    }

    void Update()
    {
        if (targetImage == null) return;

        // Smoothly lerp to target opacity
        if (Mathf.Abs(currentOpacity - targetOpacity) > 0.001f)
        {
            currentOpacity = Mathf.Lerp(currentOpacity, targetOpacity, Time.deltaTime * fadeSpeed);
            SetImageOpacity(currentOpacity);
        }
    }

    // Called by SequentialButtonDisplay
    public void OnGroupChanged(int groupIndex)
    {
        if (targetImage == null) return;

        Debug.Log($"ImageOpacityController: Group changed to {groupIndex}");

        // Before start group - stay transparent
        if (groupIndex < startAtGroupIndex)
        {
            targetOpacity = 0f;
            Debug.Log($"ImageOpacityController: Before start group, opacity = 0");
            return;
        }

        // Calculate opacity based on groups past start
        int groupsPastStart = groupIndex - startAtGroupIndex;
        targetOpacity = startOpacity + (groupsPastStart * opacityIncreasePerGroup);
        targetOpacity = Mathf.Clamp(targetOpacity, 0f, maxOpacity);

        Debug.Log($"ImageOpacityController: Setting target opacity to {targetOpacity:F2}");
    }

    void SetImageOpacity(float opacity)
    {
        if (targetImage == null) return;

        Color color = targetImage.color;
        color.a = Mathf.Clamp01(opacity);
        targetImage.color = color;
    }

    // Optional: Manually set opacity
    public void SetOpacity(float opacity)
    {
        targetOpacity = Mathf.Clamp01(opacity);
    }

    // Optional: Reset to transparent
    public void ResetOpacity()
    {
        targetOpacity = 0f;
        currentOpacity = 0f;
        SetImageOpacity(0f);
    }
}