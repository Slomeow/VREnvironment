using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SequentialTextDisplay : MonoBehaviour
{
    [Header("Button Groups")]
    [Tooltip("Each group represents a set of 3 buttons to display")]
    public ButtonGroup[] buttonGroups;

    [Header("Timing")]
    [Tooltip("Time in seconds between each button appearance")]
    public float intervalBetweenButtons = 10f;

    [Header("Options")]
    [Tooltip("Start sequence automatically on scene load")]
    public bool autoStart = true;

    [Header("Events")]
    [Tooltip("Triggered when a new button group starts displaying")]
    public UnityEvent<int> onGroupChanged;

    [Header("Scene Transition")]
    public SceneTransitionOnGroup sceneTransition;

    private int currentGroupIndex = 0;
    private Coroutine displayCoroutine;
    private bool waitingForSelection = false;

    public int CurrentGroupIndex => currentGroupIndex;

    [System.Serializable]
    public class ButtonGroup
    {
        public Button button1;
        public Button button2;
        public Button button3;
    }

    void Start()
    {
        // Hide all buttons at start and set up click listeners
        foreach (ButtonGroup group in buttonGroups)
        {
            SetupButton(group.button1);
            SetupButton(group.button2);
            SetupButton(group.button3);
        }

        if (autoStart)
        {
            StartSequence();
        }
    }

    private void SetupButton(Button btn)
    {
        if (btn != null)
        {
            btn.gameObject.SetActive(false);
            // Add our listener without removing existing ones
            btn.onClick.AddListener(() => OnAnyButtonClicked(btn));
        }
    }

    private void OnAnyButtonClicked(Button clickedButton)
    {
        // This just handles the sequence progression
        // The button's original onClick events will still fire
        OnButtonSelected(clickedButton);
    }

    public void StartSequence()
    {
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
        }

        currentGroupIndex = 0;
        displayCoroutine = StartCoroutine(DisplayButtonGroups());
    }

    public void StopSequence()
    {
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
            displayCoroutine = null;
        }
    }

    public void ResetSequence()
    {
        StopSequence();

        foreach (ButtonGroup group in buttonGroups)
        {
            HideButton(group.button1);
            HideButton(group.button2);
            HideButton(group.button3);
        }

        currentGroupIndex = 0;
        waitingForSelection = false;
    }

    private void OnButtonSelected(Button selectedButton)
    {
        if (!waitingForSelection) return;

        Debug.Log($"Button selected: {selectedButton.name} from group {currentGroupIndex}");

        // Note: The button's original onClick events have already fired at this point
        // This method just handles hiding the group and moving to the next one

        // Hide current group
        ButtonGroup currentGroup = buttonGroups[currentGroupIndex];
        HideButton(currentGroup.button1);
        HideButton(currentGroup.button2);
        HideButton(currentGroup.button3);

        // Move to next group
        currentGroupIndex++;
        Debug.Log($"Moving to group index: {currentGroupIndex}");
        sceneTransition.OnGroupChanged(currentGroupIndex);

        waitingForSelection = false;

        // Continue the sequence
        if (currentGroupIndex < buttonGroups.Length && displayCoroutine != null)
        {
            // The coroutine will continue automatically
        }
        else if (currentGroupIndex >= buttonGroups.Length)
        {
            Debug.Log("All button groups completed!");
            displayCoroutine = null;
        }
    }

    private IEnumerator DisplayButtonGroups()
    {
        while (currentGroupIndex < buttonGroups.Length)
        {
            Debug.Log($"Starting display for button group index: {currentGroupIndex}");
            ButtonGroup currentGroup = buttonGroups[currentGroupIndex];

            // Notify listeners that a new group is starting
            onGroupChanged?.Invoke(currentGroupIndex);

            // Show button 1
            if (currentGroup.button1 != null)
            {
                currentGroup.button1.gameObject.SetActive(true);
                yield return new WaitForSeconds(intervalBetweenButtons);
            }

            // Show button 2
            if (currentGroup.button2 != null)
            {
                currentGroup.button2.gameObject.SetActive(true);
                yield return new WaitForSeconds(intervalBetweenButtons);
            }

            // Show button 3
            if (currentGroup.button3 != null)
            {
                currentGroup.button3.gameObject.SetActive(true);
            }

            // Wait for user selection
            waitingForSelection = true;
            Debug.Log($"Waiting for button selection from group {currentGroupIndex}");
            yield return new WaitUntil(() => !waitingForSelection);
            Debug.Log($"Selection received, currentGroupIndex is now: {currentGroupIndex}");
        }

        displayCoroutine = null;
    }

    private void HideButton(Button btn)
    {
        if (btn != null)
        {
            btn.gameObject.SetActive(false);
        }
    }

    // Optional: Get which button was selected (call this to retrieve selection data)
    public void OnButton1Selected()
    {
        Debug.Log("User selected Option 1");
        // Add your custom logic here
    }

    public void OnButton2Selected()
    {
        Debug.Log("User selected Option 2");
        // Add your custom logic here
    }

    public void OnButton3Selected()
    {
        Debug.Log("User selected Option 3");
        // Add your custom logic here
    }
}