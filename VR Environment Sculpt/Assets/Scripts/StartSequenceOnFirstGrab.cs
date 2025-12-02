using UnityEngine;
using Oculus.Interaction;

public class StartSequenceOnFirstGrab : MonoBehaviour
{
    [Header("Reference to your sequence script")]
    public SequentialTextDisplay sequence;

    private GrabInteractable grabInteractable;
    private bool hasTriggered = false;

    private void Awake()
    {
        grabInteractable = GetComponent<GrabInteractable>();
    }

    private void OnEnable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.WhenStateChanged += OnGrabStateChanged;
        }
    }

    private void OnDisable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.WhenStateChanged -= OnGrabStateChanged;
        }
    }

    private void OnGrabStateChanged(InteractableStateChangeArgs args)
    {
        // Detect first time it enters the "Select" state (grabbed)
        if (!hasTriggered && args.NewState == InteractableState.Select)
        {
            hasTriggered = true;
            Debug.Log("Object grabbed for the first time — starting sequence.");

            if (sequence != null)
            {
                sequence.BeginSequenceFromExternalTrigger();
            }
            else
            {
                Debug.LogWarning("StartSequenceOnFirstGrab: No SequentialTextDisplay assigned!");
            }
        }
    }
}
