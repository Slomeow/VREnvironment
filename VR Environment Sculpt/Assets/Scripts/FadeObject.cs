using UnityEngine;
using Oculus.Interaction;

public class FadeObject : MonoBehaviour
{
    [Header("Texture Settings")]
    [Tooltip("The texture to change to after interaction")]
    public Texture2D newTexture;

    [Tooltip("Material property name (usually _MainTex)")]
    public string texturePropertyName = "_MainTex";

    [Header("Disappear Settings")]
    [Tooltip("Delay before object disappears (in seconds)")]
    public float disappearDelay = 2f;

    [Tooltip("Fade out duration (0 for instant)")]
    public float fadeOutDuration = 1f;

    [Header("Optional Audio")]
    public AudioClip interactionSound;

    private Renderer objRenderer;
    private Material objMaterial;
    private AudioSource audioSource;
    private bool hasBeenInteracted = false;

    // Reference to the interactable component
    private PokeInteractable pokeInteractable;
    private RayInteractable rayInteractable;
    private GrabInteractable grabInteractable;

    void Start()
    {
        // Get the renderer component
        objRenderer = GetComponent<Renderer>();

        if (objRenderer == null)
        {
            UnityEngine.Debug.LogError("VRInteractiveObject: No Renderer found on " + gameObject.name);
            enabled = false;
            return;
        }

        // Create a new material instance to avoid affecting other objects
        objMaterial = objRenderer.material;

        // Setup audio if clip is assigned
        if (interactionSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = interactionSound;
            audioSource.playOnAwake = false;
        }

        // Get Oculus Interactable components
        pokeInteractable = GetComponent<PokeInteractable>();
        rayInteractable = GetComponent<RayInteractable>();
        grabInteractable = GetComponent<GrabInteractable>();

        // Subscribe to events
        if (pokeInteractable != null)
        {
            pokeInteractable.WhenInteractorViewAdded += OnInteraction;
        }

        if (rayInteractable != null)
        {
            rayInteractable.WhenInteractorViewAdded += OnInteraction;
        }

        if (grabInteractable != null)
        {
            grabInteractable.WhenInteractorViewAdded += OnInteraction;
        }

        if (pokeInteractable == null && rayInteractable == null && grabInteractable == null)
        {
            UnityEngine.Debug.LogWarning("VRInteractiveObject: No Oculus Interactable component found. Add PokeInteractable, RayInteractable, or GrabInteractable component.");
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (pokeInteractable != null)
        {
            pokeInteractable.WhenInteractorViewAdded -= OnInteraction;
        }

        if (rayInteractable != null)
        {
            rayInteractable.WhenInteractorViewAdded -= OnInteraction;
        }

        if (grabInteractable != null)
        {
            grabInteractable.WhenInteractorViewAdded -= OnInteraction;
        }
    }

    private void OnInteraction(IInteractorView interactorView)
    {
        if (hasBeenInteracted) return;

        hasBeenInteracted = true;

        // Change texture
        if (newTexture != null && objMaterial != null)
        {
            objMaterial.SetTexture(texturePropertyName, newTexture);
        }

        // Play sound
        if (audioSource != null && interactionSound != null)
        {
            audioSource.Play();
        }

        // Start disappear sequence
        StartCoroutine(DisappearSequence());
    }

    private System.Collections.IEnumerator DisappearSequence()
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(disappearDelay);

        // Fade out if duration > 0
        if (fadeOutDuration > 0 && objMaterial != null)
        {
            // Enable transparency on the material
            SetMaterialTransparent(objMaterial);

            Color originalColor = objMaterial.color;
            float elapsed = 0f;

            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
                objMaterial.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }
        }

        // Destroy the object
        Destroy(gameObject);
    }

    private void SetMaterialTransparent(Material mat)
    {
        // Set render mode to transparent for fade effect
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
    }

    // Alternative: Public method for non-Oculus interaction systems
    public void OnInteract()
    {
        if (hasBeenInteracted) return;

        hasBeenInteracted = true;

        if (newTexture != null && objMaterial != null)
        {
            objMaterial.SetTexture(texturePropertyName, newTexture);
        }

        if (audioSource != null && interactionSound != null)
        {
            audioSource.Play();
        }

        StartCoroutine(DisappearSequence());
    }
}