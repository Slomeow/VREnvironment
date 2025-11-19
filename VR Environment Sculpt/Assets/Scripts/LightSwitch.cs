using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Oculus.Interaction;

public class LightSwitch : MonoBehaviour
{
    // HOW THIS WORKS:
    // Create a Grab Interactable cube.
    // Place it where you would want your door handle and disable (uncheck) its mesh renderer.
    // Add this script to it. 
    //
    // Drag the object you want to move (like a door or platform) into the "Object To Move" slot.

    [Header("Object Reference")]
    [Tooltip("Drag the object you want to move here")]
    public Transform objectToMove;

    [Header("Movement Settings")]
    [Tooltip("How far down the object moves on Y axis")]
    public float moveDistance = 1f;

    [Tooltip("Speed of the movement")]
    public float moveSpeed = 2f;

    [Tooltip("Time the object takes to move")]
    public float moveTime = 2f;

    [Tooltip("Time to wait before moving back up")]
    public float waitTime = 1f;

    [Header("Handle Settings")]
    [Tooltip("Should the handle itself return to its original position?")]
    public bool resetHandlePosition = true;

    [Header("Object Deactivation")]
    [Tooltip("Drag the GameObject you want to toggle on/off here")]
    public GameObject objectToToggle;

    [Tooltip("When should the object be toggled?")]
    public ToggleTime whenToToggle = ToggleTime.OnGrab;

    public enum ToggleTime
    {
        OnGrab,          // Toggle immediately when grabbed
        WhenMovedDown,   // Toggle when object reaches bottom
        WhenMovedUp      // Toggle when object returns to top
    }

    private Grabbable grabbable;
    private bool isMoving = false;
    private Vector3 startPosition;
    private Vector3 downPosition;
    private Vector3 handleStartPosition;

    void Start()
    {
        // Get the Grabbable component on this handle
        grabbable = GetComponent<Grabbable>();
        if (grabbable == null)
        {
            Debug.LogError("No Grabbable component found on " + gameObject.name);
            return;
        }

        if (objectToMove == null)
        {
            Debug.LogError("No object assigned! Drag the object into the Inspector.");
            return;
        }

        // Set up positions
        startPosition = objectToMove.position;
        downPosition = startPosition + new Vector3(0, -moveDistance, 0);

        // Store handle's starting position
        handleStartPosition = transform.position;

        // Listen for grab events
        grabbable.WhenPointerEventRaised += OnPointerEvent;
    }

    void OnDestroy()
    {
        if (grabbable != null)
        {
            grabbable.WhenPointerEventRaised -= OnPointerEvent;
        }
    }

    private void OnPointerEvent(PointerEvent pointerEvent)
    {
        // When the handle is grabbed, move the object
        if (pointerEvent.Type == PointerEventType.Select)
        {
            if (!isMoving)
            {
                // Toggle on grab if set
                if (whenToToggle == ToggleTime.OnGrab && objectToToggle != null)
                {
                    ToggleObject();
                }

                StartCoroutine(MoveObject());
            }
        }

        // When handle is released, return it to original position
        if (pointerEvent.Type == PointerEventType.Unselect && resetHandlePosition)
        {
            StartCoroutine(ReturnHandleToPosition());
        }
    }

    private void ToggleObject()
    {
        bool newState = !objectToToggle.activeSelf;
        objectToToggle.SetActive(newState);
        Debug.Log(objectToToggle.name + " is now " + (newState ? "active" : "inactive"));
    }

    private IEnumerator MoveObject()
    {
        isMoving = true;

        // Move down
        float elapsedTime = 0f;
        while (elapsedTime < moveTime)
        {
            objectToMove.position = Vector3.Lerp(startPosition, downPosition, elapsedTime / moveTime);
            elapsedTime += Time.deltaTime * moveSpeed;
            yield return null;
        }
        objectToMove.position = downPosition;

        // Toggle when moved down if set
        if (whenToToggle == ToggleTime.WhenMovedDown && objectToToggle != null)
        {
            ToggleObject();
        }

        // Wait
        yield return new WaitForSeconds(waitTime);

        // Move back up
        elapsedTime = 0f;
        while (elapsedTime < moveTime)
        {
            objectToMove.position = Vector3.Lerp(downPosition, startPosition, elapsedTime / moveTime);
            elapsedTime += Time.deltaTime * moveSpeed;
            yield return null;
        }
        objectToMove.position = startPosition;

        // Toggle when moved up if set
        if (whenToToggle == ToggleTime.WhenMovedUp && objectToToggle != null)
        {
            ToggleObject();
        }

        isMoving = false;
    }

    private IEnumerator ReturnHandleToPosition()
    {
        float returnTime = 0.5f; // Quick return
        float elapsedTime = 0f;
        Vector3 currentPos = transform.position;

        while (elapsedTime < returnTime)
        {
            transform.position = Vector3.Lerp(currentPos, handleStartPosition, elapsedTime / returnTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = handleStartPosition;
    }
}