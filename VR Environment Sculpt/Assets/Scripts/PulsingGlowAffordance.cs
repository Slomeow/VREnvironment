using UnityEngine;

/// <summary>
/// Simple pulsing emissive affordance for Oculus/Meta grabbables.
/// Keeps an instance material and pulses emission while not grabbed.
/// Call DisableGlow() when the object is grabbed (so pulsing stops).
/// </summary>
[DisallowMultipleComponent]
public class PulsingGlowAffordance : MonoBehaviour
{
    [Header("Renderer & Material")]
    [Tooltip("Renderer whose material will pulse. If empty, will try to use the Renderer on this GameObject.")]
    public Renderer targetRenderer;

    [Tooltip("Property name for emission color (commonly _EmissionColor).")]
    public string emissionProperty = "_EmissionColor";

    [Header("Pulse Settings (heartbeat)")]
    public Color baseGlowColor = Color.white;
    [Tooltip("How fast the heartbeat pulse is.")]
    public float pulseSpeed = 1.2f;
    [Tooltip("Minimum emission multiplier.")]
    [Range(0f, 5f)] public float minIntensity = 0.15f;
    [Tooltip("Maximum emission multiplier.")]
    [Range(0f, 10f)] public float maxIntensity = 1.0f;

    [Header("Grab Behavior")]
    [Tooltip("When grabbed, optionally set emission to this multiplier (set to 0 to turn off).")]
    public float grabbedIntensity = 0f;

    // runtime
    private Material instancedMaterial;
    private bool isGrabbed = false;
    private int emissionPropertyId;

    void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        if (targetRenderer == null)
        {
            Debug.LogWarning($"PulsingGlowAffordance: No Renderer found on '{name}'. Disabling.");
            enabled = false;
            return;
        }

        // Instance the material so changes don't affect shared materials
        instancedMaterial = targetRenderer.material;
        emissionPropertyId = Shader.PropertyToID(emissionProperty);

        // Ensure emission keyword is enabled if shader needs it
        if (instancedMaterial.HasProperty(emissionPropertyId))
        {
            // Set an initial emission
            instancedMaterial.EnableKeyword("_EMISSION");
            instancedMaterial.SetColor(emissionPropertyId, baseGlowColor * minIntensity);
        }
        else
        {
            Debug.LogWarning($"PulsingGlowAffordance: Material on '{name}' does not have property '{emissionProperty}'.");
        }
    }

    void Update()
    {
        if (isGrabbed || instancedMaterial == null) return;

        // Heartbeat-like pulsing (sin wave remapped 0..1)
        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        float intensity = Mathf.Lerp(minIntensity, maxIntensity, t);

        instancedMaterial.SetColor(emissionPropertyId, baseGlowColor * intensity);
    }

    /// <summary>
    /// Call to stop the pulse and set emission for grabbed state.
    /// Safe to call multiple times.
    /// </summary>
    public void DisableGlow()
    {
        if (isGrabbed) return;
        isGrabbed = true;

        if (instancedMaterial != null && instancedMaterial.HasProperty(emissionPropertyId))
        {
            instancedMaterial.SetColor(emissionPropertyId, baseGlowColor * grabbedIntensity);
        }
    }

    /// <summary>
    /// Optional: call to re-enable the pulse (if you ever respawn or reset the object).
    /// </summary>
    public void EnableGlow()
    {
        isGrabbed = false;
    }

    void OnDestroy()
    {
        // If the material was instanced, destroy it to avoid leaks in editor/playmode
        if (instancedMaterial != null)
        {
#if UNITY_EDITOR
            // In editor, use DestroyImmediate to avoid leak warnings
            DestroyImmediate(instancedMaterial);
#else
            Destroy(instancedMaterial);
#endif
        }
    }
}
