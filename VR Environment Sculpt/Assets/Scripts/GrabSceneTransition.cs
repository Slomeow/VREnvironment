using Oculus.Interaction;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GrabSceneTransition : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string sceneToLoad = "NextScene";
    [SerializeField] private float transitionDelay = 0.5f;

    private Grabbable grabbable;
    private bool hasTriggered = false;
    private AsyncOperation asyncLoad;

    async void Start()
    {
        // Get the Grabbable component
        grabbable = GetComponent<Grabbable>();

        if (grabbable == null)
        {
            Debug.LogError("GrabSceneTransition: No Grabbable component found on " + gameObject.name);
            return;
        }

        // Subscribe to grab events
        grabbable.WhenPointerEventRaised += OnGrabEvent;

        // Validate scene name before loading
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError("Scene name is empty! Please set the scene name in the inspector.");
            return;
        }

        Debug.Log($"Pre-loading scene: {sceneToLoad}");
        asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            await System.Threading.Tasks.Task.Yield();
        }

        Debug.Log($"Scene {sceneToLoad} is pre-loaded and ready to activate!");
    }

    void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (grabbable != null)
        {
            grabbable.WhenPointerEventRaised -= OnGrabEvent;
        }
    }

    private void OnGrabEvent(PointerEvent pointerEvent)
    {
        // Check if this is a grab event (not release)
        if (pointerEvent.Type == PointerEventType.Select && !hasTriggered)
        {
            hasTriggered = true;
            Debug.Log("Object grabbed! Activating scene: " + sceneToLoad);

            if (asyncLoad == null)
            {
                Debug.LogError("Scene was not pre-loaded! Cannot activate.");
                return;
            }

            if (transitionDelay > 0)
            {
                Invoke(nameof(ActivateScene), transitionDelay);
            }
            else
            {
                ActivateScene();
            }
        }
    }

    private void ActivateScene()
    {
        // Disable GrabAndLocate components to prevent null reference errors
        var grabAndLocates = FindObjectsOfType<Meta.XR.MRUtilityKit.BuildingBlocks.GrabAndLocate>();
        foreach (var component in grabAndLocates)
        {
            if (component != null)
            {
                component.enabled = false;
            }
        }

        asyncLoad.allowSceneActivation = true;
        Debug.Log("Scene activated");
    }
}