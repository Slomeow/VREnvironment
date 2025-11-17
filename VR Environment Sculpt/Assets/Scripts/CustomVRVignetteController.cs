//using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
//using static System.Net.Mime.MediaTypeNames;

public class CustomVRVignetteController : MonoBehaviour
{
    [Header("Vignette UI")]
    [Tooltip("The Image component for the vignette overlay (leave empty to auto-create)")]
    public Image vignetteImage;

    [Tooltip("Your custom vignette sprite (optional - only needed if auto-creating)")]
    public Sprite customVignetteSprite;

    [Header("Camera")]
    [Tooltip("The VR camera to parent the vignette canvas to (usually CenterEyeAnchor)")]
    public Camera vrCamera;

    [Header("Progression Settings")]
    [Tooltip("Button group index to start the vignette effect")]
    public int startAtGroupIndex = 4;

    [Tooltip("Initial opacity when vignette starts (0-1)")]
    [Range(0f, 1f)]
    public float startOpacity = 0.3f;

    [Tooltip("Maximum opacity the vignette can reach (0-1)")]
    [Range(0f, 1f)]
    public float maxOpacity = 0.9f;

    [Tooltip("How much opacity increases per button group")]
    [Range(0f, 0.3f)]
    public float opacityIncreasePerGroup = 0.15f;

    [Header("Animation")]
    [Tooltip("Speed of the fade transition")]
    public float fadeSpeed = 2f;

    [Header("Vignette Appearance")]
    [Tooltip("Color of the vignette (usually black)")]
    public Color vignetteColor = Color.black;

    private float targetOpacity = 0f;
    private float currentOpacity = 0f;
    private Canvas vignetteCanvas;

    void Start()
    {
        // Auto-find VR camera if not assigned
        if (vrCamera == null)
        {
            vrCamera = Camera.main;
            if (vrCamera == null)
            {
                Debug.LogError("CustomVRVignette: No camera assigned and couldn't find main camera!");
                return;
            }
        }

        // Create the vignette if it doesn't exist
        if (vignetteImage == null)
        {
            CreateVignetteOverlay();
        }
        else
        {
            // Make sure existing vignette is set up correctly
            SetupVignetteCanvas();
        }

        // Initialize with transparency
        SetVignetteOpacity(0f);

        Debug.Log("CustomVRVignette: Initialized successfully");
    }

    void CreateVignetteOverlay()
    {
        // Create canvas as child of camera
        GameObject canvasObj = new GameObject("VignetteCanvas");
        canvasObj.transform.SetParent(vrCamera.transform);
        canvasObj.transform.localPosition = new Vector3(0, 0, 0.5f); // Slightly in front of camera
        canvasObj.transform.localRotation = Quaternion.identity;
        canvasObj.transform.localScale = Vector3.one * 0.001f; // Scale down for VR

        vignetteCanvas = canvasObj.AddComponent<Canvas>();
        vignetteCanvas.renderMode = RenderMode.WorldSpace;
        vignetteCanvas.sortingOrder = 9999; // Render on top

        // Add Canvas Scaler
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 100;

        // Add Graphic Raycaster (but we'll disable raycast on image)
        canvasObj.AddComponent<GraphicRaycaster>();

        // Set canvas size
        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(1000, 1000);

        // Create image
        GameObject imageObj = new GameObject("VignetteImage");
        imageObj.transform.SetParent(canvasObj.transform);
        imageObj.transform.localPosition = Vector3.zero;
        imageObj.transform.localRotation = Quaternion.identity;
        imageObj.transform.localScale = Vector3.one;

        vignetteImage = imageObj.AddComponent<Image>();
        vignetteImage.raycastTarget = false; // Don't block raycasts

        // Use custom sprite if provided, otherwise try to find one, otherwise generate
        Sprite vignetteSprite = customVignetteSprite;

        if (vignetteSprite == null)
        {
            // Try Unity's built-in
            vignetteSprite = Resources.Load<Sprite>("UI/Skin/Vignette");
        }

        if (vignetteSprite == null)
        {
            // Last resort: generate one
            Debug.LogWarning("CustomVRVignette: No custom sprite provided, generating one. For best results, assign a vignette sprite!");
            vignetteSprite = CreateVignetteSprite();
        }

        vignetteImage.sprite = vignetteSprite;
        vignetteImage.type = Image.Type.Simple;
        vignetteImage.color = vignetteColor;

        // Stretch to fill
        RectTransform imageRect = imageObj.GetComponent<RectTransform>();
        imageRect.anchorMin = Vector2.zero;
        imageRect.anchorMax = Vector2.one;
        imageRect.sizeDelta = Vector2.zero;
        imageRect.anchoredPosition = Vector2.zero;

        Debug.Log($"CustomVRVignette: Created vignette overlay with sprite: {vignetteSprite.name}");
    }

    void SetupVignetteCanvas()
    {
        // If user provided their own image, make sure it's parented to camera
        if (vignetteImage.transform.parent != null)
        {
            Canvas canvas = vignetteImage.GetComponentInParent<Canvas>();
            if (canvas != null && canvas.transform.parent != vrCamera.transform)
            {
                Debug.LogWarning("CustomVRVignette: Vignette canvas should be parented to VR camera. Auto-fixing...");
                canvas.transform.SetParent(vrCamera.transform);
                canvas.transform.localPosition = new Vector3(0, 0, 0.5f);
                canvas.transform.localRotation = Quaternion.identity;
            }
        }
    }

    Sprite CreateVignetteSprite()
    {
        // Create a texture with radial gradient for vignette effect
        int size = 512;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);

        Vector2 center = new Vector2(size / 2f, size / 2f);
        float maxDistance = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float normalizedDistance = distance / maxDistance;

                // Invert: clear in center (0), dark at edges (1)
                float alpha = Mathf.Clamp01(normalizedDistance);

                // Apply smoothstep for smoother falloff
                alpha = Mathf.SmoothStep(0f, 1f, alpha);

                // Power curve for more dramatic vignette
                alpha = Mathf.Pow(alpha, 2f);

                // White with varying alpha (will be tinted by vignetteColor)
                texture.SetPixel(x, y, new Color(1, 1, 1, alpha));
            }
        }

        texture.Apply();

        Debug.Log("CustomVRVignette: Created vignette sprite with radial gradient");

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    void Update()
    {
        if (vignetteImage == null) return;

        // Smoothly lerp to target opacity
        if (Mathf.Abs(currentOpacity - targetOpacity) > 0.001f)
        {
            currentOpacity = Mathf.Lerp(currentOpacity, targetOpacity, Time.deltaTime * fadeSpeed);
            SetVignetteOpacity(currentOpacity);
        }
    }

    // Called by SequentialButtonDisplay
    public void OnGroupChanged(int groupIndex)
    {
        Debug.Log($"CustomVRVignette: Group changed to {groupIndex}");

        if (groupIndex < startAtGroupIndex)
        {
            targetOpacity = 0f;
            Debug.Log($"CustomVRVignette: Before start group, opacity = 0");
            return;
        }

        // Calculate opacity based on groups past start
        int groupsPastStart = groupIndex - startAtGroupIndex;
        targetOpacity = startOpacity + (groupsPastStart * opacityIncreasePerGroup);
        targetOpacity = Mathf.Clamp(targetOpacity, 0f, maxOpacity);

        Debug.Log($"CustomVRVignette: Setting target opacity to {targetOpacity:F2}");
    }

    void SetVignetteOpacity(float opacity)
    {
        if (vignetteImage == null) return;

        Color color = vignetteColor;
        color.a = opacity;
        vignetteImage.color = color;
    }

    // Optional helpers
    public void SetOpacity(float opacity)
    {
        targetOpacity = Mathf.Clamp01(opacity);
    }

    public void ResetVignette()
    {
        targetOpacity = 0f;
        currentOpacity = 0f;
        SetVignetteOpacity(0f);
    }
}