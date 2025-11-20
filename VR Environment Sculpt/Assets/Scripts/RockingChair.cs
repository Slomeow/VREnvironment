using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Oculus.Interaction;

public class RockingChair : MonoBehaviour
{
    // HOW THIS WORKS:
    // Create a Grab Interactable cube.
    // Place it where you would want your door handle and disable (uncheck) its mesh renderer.
    // Add this script to it. 
    //
    // Make an empty game object and call it DoorPivot
    // Place it on the edge of the door, where you would want/expect it to pivot from.
    // Make the door object and the handle object children of that DoorPivot by dragging them inside of it.
    // You should see them nested under in the hierarchy!
    //
    // In the inspector, set Door To Rock as your DoorPivot object.

    [Header("Door Reference")]
    [Tooltip("Drag the door GameObject here")]
    public Transform doorToRock;

    [Header("Rocking Settings")]
    [Tooltip("Maximum angle the door rocks to")]
    public float rockAngle = 15f;

    [Tooltip("Speed of the rocking motion")]
    public float rockSpeed = 2f;

    [Tooltip("Number of times to rock back and forth")]
    public int rockCycles = 3;

    [Header("Optional Audio")]
    public AudioClip grabSound;

    private AudioSource audioSource;
    private Grabbable grabbable;
    private bool isRocking = false;
    private Quaternion startRotation;
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

        if (doorToRock == null)
        {
            Debug.LogError("No door assigned! Drag the door into the Inspector.");
            return;
        }

        // Setup audio if clip is assigned
        if (grabSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = grabSound;
            audioSource.playOnAwake = false;
        }

        // Store starting rotation and handle position
        startRotation = doorToRock.rotation;
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
        // When the handle is grabbed, start rocking
        if (pointerEvent.Type == PointerEventType.Select)
        {
            if (!isRocking)
            {
                StartCoroutine(RockDoor());
            }
        }

        // When handle is released, return it to original position
        if (pointerEvent.Type == PointerEventType.Unselect)
        {
            StartCoroutine(ReturnHandleToPosition());
        }
    }

    private IEnumerator RockDoor()
    {
        isRocking = true;

        // Play sound
        if (audioSource != null && grabSound != null)
        {
            audioSource.Play();
        }

        // Rock back and forth for the specified number of cycles
        for (int i = 0; i < rockCycles; i++)
        {
            // Rock forward
            float elapsedTime = 0f;
            float cycleDuration = 1f / rockSpeed;

            Quaternion forwardRotation = startRotation * Quaternion.Euler(0, rockAngle, 0);

            while (elapsedTime < cycleDuration)
            {
                doorToRock.rotation = Quaternion.Slerp(startRotation, forwardRotation, elapsedTime / cycleDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Rock backward
            elapsedTime = 0f;
            Quaternion backwardRotation = startRotation * Quaternion.Euler(0, -rockAngle, 0);

            while (elapsedTime < cycleDuration)
            {
                doorToRock.rotation = Quaternion.Slerp(forwardRotation, backwardRotation, elapsedTime / cycleDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Return to center
            elapsedTime = 0f;
            while (elapsedTime < cycleDuration)
            {
                doorToRock.rotation = Quaternion.Slerp(backwardRotation, startRotation, elapsedTime / cycleDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }

        // Ensure door is back at starting rotation
        doorToRock.rotation = startRotation;
        isRocking = false;
    }

    private IEnumerator ReturnHandleToPosition()
    {
        float returnTime = 0.5f;
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