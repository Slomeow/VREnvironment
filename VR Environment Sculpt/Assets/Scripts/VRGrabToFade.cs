using UnityEngine;
using Oculus.Interaction;

public class VRGrabToFade : MonoBehaviour
{
    [Header("Fade Settings")]
    [Tooltip("Duration of the fade out effect (in seconds)")]
    public float fadeOutDuration = 2f;

    [Tooltip("Delay before fading starts after grab (in seconds)")]
    public float fadeDelay = 0f;

    [Header("Optional Audio")]
    public AudioClip grabSound;

    // near the top of VRGrabToFade
    public PulsingGlowAffordance glowAffordance; // assign in inspector (optional)

    private Renderer objRenderer;
    private Material objMaterial;
    private AudioSource audioSource;
    private bool isFading = false;
    private Grabbable grabbable;

    void Start()
    {
        // Get the renderer component
        objRenderer = GetComponent<Renderer>();

        if (objRenderer == null)
        {
            UnityEngine.Debug.LogError("VRGrabToFade: No Renderer found on " + gameObject.name);
            enabled = false;
            return;
        }

        // Create a new material instance to avoid affecting other objects
        objMaterial = objRenderer.material;

        // Pre-configure material for transparency
        SetMaterialTransparent(objMaterial);

        // Setup audio if clip is assigned
        if (grabSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = grabSound;
            audioSource.playOnAwake = false;
        }

        // Get Grabbable component
        grabbable = GetComponent<Grabbable>();

        if (grabbable == null)
        {
            UnityEngine.Debug.LogError("VRGrabToFade: No Grabbable component found on " + gameObject.name);
            enabled = false;
            return;
        }

        // Subscribe to pointer event in Start (not OnEnable)
        grabbable.WhenPointerEventRaised += OnPointerEvent;
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (grabbable != null)
        {
            grabbable.WhenPointerEventRaised -= OnPointerEvent;
        }
    }

    private void OnPointerEvent(PointerEvent pointerEvent)
    {
        // Check if it's an Unselect event (release after grabbing)
        if (pointerEvent.Type == PointerEventType.Select && !isFading)
        {
            StartFade();
        }
    }

    private void StartFade()
    {
        if (isFading) return;

        isFading = true;

        // Notify the glow affordance (if assigned)
        if (glowAffordance != null)
        {
            glowAffordance.DisableGlow();
        }

        // Play sound
        if (audioSource != null && grabSound != null)
        {
            audioSource.Play();
        }


        // Start fade sequence
        StartCoroutine(FadeOutSequence());
    }

    private System.Collections.IEnumerator FadeOutSequence()
    {
        // Wait for the specified delay
        if (fadeDelay > 0)
        {
            yield return new WaitForSeconds(fadeDelay);
        }

        // Safety check
        if (objMaterial == null || objRenderer == null)
        {
            yield break;
        }

        // Get starting alpha
        Color startColor = objMaterial.color;
        float startAlpha = startColor.a;
        float elapsed = 0f;

        // Gradually fade out
        while (elapsed < fadeOutDuration)
        {
            if (objMaterial == null) yield break;

            elapsed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeOutDuration);

            // Update material color alpha
            Color currentColor = objMaterial.color;
            objMaterial.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);

            yield return null;
        }

        // Ensure fully transparent at the end
        if (objMaterial != null)
        {
            Color finalColor = objMaterial.color;
            objMaterial.color = new Color(finalColor.r, finalColor.g, finalColor.b, 0f);
        }

        // Small delay to ensure final frame is rendered
        yield return new WaitForSeconds(0.1f);

        // Destroy the object
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }

    private void SetMaterialTransparent(Material mat)
    {
        if (mat == null) return;

        try
        {
            // Check if URP shader
            if (mat.shader.name.Contains("Universal Render Pipeline"))
            {
                // URP Lit shader settings
                mat.SetFloat("_Surface", 1);
                mat.SetFloat("_Blend", 0);

                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);

                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");

                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }
            else
            {
                // Standard Shader settings
                if (mat.HasProperty("_Mode"))
                {
                    mat.SetFloat("_Mode", 3);
                }

                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);

                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
            }
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError("VRGrabToFade: Error setting material transparent: " + e.Message);
        }
    }
}