using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SequentialTextDisplay : MonoBehaviour
{
    [Header("Text Objects")]
    [Tooltip("Array of Text/TMP objects to display sequentially")]
    public GameObject[] textObjects;

    [Header("Timing")]
    [Tooltip("Time in seconds between each text appearance")]
    public float intervalBetweenTexts = 10f;

    [Header("Options")]
    [Tooltip("Hide previous text when showing next one")]
    public bool hidePreviousText = false;

    [Tooltip("Start sequence automatically on scene load")]
    public bool autoStart = true;

    [Tooltip("Loop the sequence when finished")]
    public bool loop = false;

    private int currentIndex = 0;
    private Coroutine displayCoroutine;

    void Start()
    {
        // Hide all text objects at start
        foreach (GameObject textObj in textObjects)
        {
            if (textObj != null)
            {
                textObj.SetActive(false);
            }
        }

        if (autoStart)
        {
            StartSequence();
        }
    }

    public void StartSequence()
    {
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
        }

        currentIndex = 0;
        displayCoroutine = StartCoroutine(DisplayTextsSequentially());
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

        foreach (GameObject textObj in textObjects)
        {
            if (textObj != null)
            {
                textObj.SetActive(false);
            }
        }

        currentIndex = 0;
    }

    private IEnumerator DisplayTextsSequentially()
    {
        do
        {
            for (int i = 0; i < textObjects.Length; i++)
            {
                if (textObjects[i] != null)
                {
                    // Hide previous text if option is enabled
                    if (hidePreviousText && i > 0 && textObjects[i - 1] != null)
                    {
                        textObjects[i - 1].SetActive(false);
                    }

                    // Show current text
                    textObjects[i].SetActive(true);
                    currentIndex = i;

                    // Wait for interval (except after the last text if not looping)
                    if (i < textObjects.Length - 1 || loop)
                    {
                        yield return new WaitForSeconds(intervalBetweenTexts);
                    }
                }
            }

            // If looping, hide the last text before restarting
            if (loop && hidePreviousText && textObjects.Length > 0)
            {
                if (textObjects[textObjects.Length - 1] != null)
                {
                    textObjects[textObjects.Length - 1].SetActive(false);
                }
            }

        } while (loop);

        displayCoroutine = null;
    }

    // Optional: Manually trigger next text
    public void ShowNextText()
    {
        if (currentIndex < textObjects.Length - 1)
        {
            if (hidePreviousText && textObjects[currentIndex] != null)
            {
                textObjects[currentIndex].SetActive(false);
            }

            currentIndex++;
            if (textObjects[currentIndex] != null)
            {
                textObjects[currentIndex].SetActive(true);
            }
        }
    }
}