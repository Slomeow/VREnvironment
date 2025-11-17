//using System.Diagnostics;
using UnityEngine;

public class MetaVignetteController : MonoBehaviour
{
    [Header("Meta SDK Vignette")]
    [Tooltip("Reference to the OVRVignette or TunnelingVignette component")]
    public OVRVignette vignetteEffect;

    [Header("Progression Settings")]
    [Tooltip("Button group index to start the vignette effect (e.g., 4 for group 4)")]
    public int startAtGroupIndex = 4;

    [Tooltip("Initial falloff value when vignette starts")]
    [Range(0f, 1f)]
    public float startFalloff = 0.5f;  // Changed from 0.1 to 0.5 for more noticeable effect

    [Tooltip("Maximum falloff the vignette can reach")]
    [Range(0f, 1f)]
    public float maxFalloff = 1.0f;  // Changed from 0.8 to 1.0 for maximum effect

    [Tooltip("How much falloff increases per button group")]
    [Range(0f, 0.5f)]
    public float falloffIncreasePerGroup = 0.15f;  // Changed from 0.1 to 0.15 for faster progression

    [Header("Animation")]
    [Tooltip("Speed of the fade transition (seconds). Set to 0 for instant.")]
    public float fadeSpeed = 0f;  // Changed to 0 for instant effect during testing

    private float targetFalloff = 0f;
    private float currentFalloff = 0f;

    void Start()
    {
        Debug.Log("MetaVignetteController: Start() called");

        if (vignetteEffect == null)
        {
            Debug.Log("MetaVignetteController: Attempting to auto-find OVRVignette...");
            // Try to find it automatically on CenterEyeAnchor
            vignetteEffect = FindObjectOfType<OVRVignette>();

            if (vignetteEffect == null)
            {
                Debug.LogError("MetaVignetteController: OVRVignette component not found! Please assign it or add it to your CenterEyeAnchor.");
                return;
            }
            else
            {
                Debug.Log($"MetaVignetteController: Found OVRVignette on {vignetteEffect.gameObject.name}");
            }
        }
        else
        {
            Debug.Log($"MetaVignetteController: OVRVignette already assigned: {vignetteEffect.gameObject.name}");
        }

        // Start with vignette off
        vignetteEffect.enabled = true;
        SetVignetteFalloff(0f);
        Debug.Log("MetaVignetteController: Initialized with falloff at 0");
    }

    void Update()
    {
        if (vignetteEffect == null) return;

        // Keep vignette enabled if we have a target falloff > 0
        if (targetFalloff > 0.001f && !vignetteEffect.enabled)
        {
            vignetteEffect.enabled = true;
            Debug.LogWarning("Vignette was disabled externally, re-enabling it!");
        }

        // Smoothly lerp to target falloff OR snap instantly if fadeSpeed is 0
        if (Mathf.Abs(currentFalloff - targetFalloff) > 0.001f)
        {
            if (fadeSpeed <= 0f)
            {
                // Instant snap for testing
                currentFalloff = targetFalloff;
            }
            else
            {
                // Smooth lerp
                currentFalloff = Mathf.Lerp(currentFalloff, targetFalloff, Time.deltaTime * fadeSpeed);
            }
            SetVignetteFalloff(currentFalloff);
            Debug.Log($"Vignette Update: currentFalloff={currentFalloff:F2}, targetFalloff={targetFalloff:F2}");
        }
        // IMPORTANT: Keep applying settings every frame to prevent external overrides
        else if (targetFalloff > 0.001f)
        {
            SetVignetteFalloff(currentFalloff);
        }
    }

    // This method will be called by the SequentialButtonDisplay script
    public void OnGroupChanged(int groupIndex)
    {
        if (vignetteEffect == null) return;

        Debug.Log($"MetaVignetteController: Group changed to {groupIndex}");

        // Only start vignette effect at or after the specified group
        if (groupIndex < startAtGroupIndex)
        {
            targetFalloff = 0f;
            Debug.Log($"Vignette: Group {groupIndex} is before start group {startAtGroupIndex}, staying off");
            return;
        }

        // Calculate how many groups past the start we are
        int groupsPastStart = groupIndex - startAtGroupIndex;

        // Calculate target falloff
        targetFalloff = startFalloff + (groupsPastStart * falloffIncreasePerGroup);
        targetFalloff = Mathf.Clamp(targetFalloff, 0f, maxFalloff);

        Debug.Log($"Vignette: Setting target falloff to {targetFalloff:F2} (group {groupIndex}, {groupsPastStart} past start)");
    }

    // Set the vignette falloff intensity
    private void SetVignetteFalloff(float falloff)
    {
        if (vignetteEffect == null) return;

        // Disable vignette when falloff is near zero
        if (falloff <= 0.001f)
        {
            vignetteEffect.enabled = false;
            Debug.Log("Vignette: DISABLED (falloff = 0)");
        }
        else
        {
            vignetteEffect.enabled = true;

            // Control the vignette intensity by adjusting the Field of View
            // Smaller FOV = more vignette effect - MADE MORE DRASTIC FOR TESTING
            float targetFOV = Mathf.Lerp(180f, 30f, falloff);  // Changed from 60f to 30f for extreme effect
            vignetteEffect.VignetteFieldOfView = targetFOV;

            // Optional: Also adjust the falloff degrees for smoother transitions
            // Smaller degrees = sharper edge - MADE MORE DRAMATIC
            float targetFalloffDegrees = Mathf.Lerp(60f, 5f, falloff);  // Changed from 20f to 5f for sharper edge
            vignetteEffect.VignetteFalloffDegrees = targetFalloffDegrees;

            // You can also adjust the color alpha if needed - MADE DARKER
            Color vignetteColor = vignetteEffect.VignetteColor;
            vignetteColor.a = Mathf.Lerp(0.8f, 1f, falloff);  // Changed from 0.5f to 0.8f
            vignetteEffect.VignetteColor = vignetteColor;

            // Read back the actual values to verify they're being set
            float actualFOV = vignetteEffect.VignetteFieldOfView;
            float actualFalloffDegrees = vignetteEffect.VignetteFalloffDegrees;

            Debug.Log($"Vignette: SET FOV={targetFOV:F1}° (actual={actualFOV:F1}°), FalloffDegrees={targetFalloffDegrees:F1}° (actual={actualFalloffDegrees:F1}°), Alpha={vignetteColor.a:F2}, Enabled={vignetteEffect.enabled}");
        }
    }

    // Optional: Manually set falloff
    public void SetFalloff(float falloff)
    {
        targetFalloff = Mathf.Clamp01(falloff);
    }

    // Optional: Reset vignette
    public void ResetVignette()
    {
        targetFalloff = 0f;
        currentFalloff = 0f;
        SetVignetteFalloff(0f);
    }

    // Optional: Instantly set to target (no lerp)
    public void SnapToTarget()
    {
        currentFalloff = targetFalloff;
        SetVignetteFalloff(currentFalloff);
    }
}