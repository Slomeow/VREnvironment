using UnityEngine;

public class ImageOpacityController : MonoBehaviour
{
    [Header("Target Renderer")]
    [Tooltip("The object's Renderer whose material opacity will be controlled")]
    public Renderer targetRenderer;

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

    private Material targetMaterial;
    private float targetOpacity = 0f;
    private float currentOpacity = 0f;

    private bool isResetting = false;


    void Start()
    {
        if (targetRenderer == null)
        {
            Debug.LogError("MaterialOpacityController: Target Renderer not assigned!");
            return;
        }

        // use materialInstance so it doesn't overwrite shared material
        targetMaterial = targetRenderer.material;

        // force shader mode to Fade so alpha works
        SetMaterialToFadeMode(targetMaterial);

        // start fully transparent
        SetMaterialOpacity(0f);

        Debug.Log("MaterialOpacityController: Initialized with Fade shader mode.");
    }

    void Update()
    {
        if (targetMaterial == null) return;

        // smooth fade
        if (Mathf.Abs(currentOpacity - targetOpacity) > 0.001f)
        {
            currentOpacity = Mathf.Lerp(currentOpacity, targetOpacity, Time.deltaTime * fadeSpeed);
            SetMaterialOpacity(currentOpacity);
        }
    }

    public void OnGroupChanged(int groupIndex)
    {
        if (isResetting) return;
        if (targetMaterial == null) return;

        Debug.Log($"MaterialOpacityController: Group changed to {groupIndex}");

        if (groupIndex < startAtGroupIndex)
        {
            targetOpacity = 0f;
            return;
        }

        int groupsPastStart = groupIndex - startAtGroupIndex;

        targetOpacity = startOpacity + (groupsPastStart * opacityIncreasePerGroup);
        targetOpacity = Mathf.Clamp(targetOpacity, 0f, maxOpacity);

        Debug.Log($"MaterialOpacityController: Target opacity set to {targetOpacity:F2}");
    }

    private void SetMaterialOpacity(float opacity)
    {
        if (targetMaterial == null) return;

        Color c = targetMaterial.color;
        c.a = Mathf.Clamp01(opacity);
        targetMaterial.color = c;
    }

    public void SetOpacity(float opacity)
    {
        targetOpacity = Mathf.Clamp01(opacity);
    }

    public void ResetOpacity()
    {
        isResetting = true;
        targetOpacity = 0f;
        currentOpacity = 0f;
        SetMaterialOpacity(0f);
        Debug.Log("MaterialOpacityController: Opacity reset and ready to increase again.");

    }

    public void BlackOut()
    {
        targetOpacity = 1f;
        currentOpacity = 1f;
        SetMaterialOpacity(1f);
    }

    public void ResumeIncreasing()
    {
        isResetting = false;
    }

    // Ensures material supports alpha transparency
    private void SetMaterialToFadeMode(Material mat)
    {
        mat.SetFloat("_Mode", 2); // 2 = Fade, 3 = Transparent
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
    }
}
