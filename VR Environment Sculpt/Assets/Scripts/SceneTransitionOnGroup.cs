using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionOnGroup : MonoBehaviour
{
    [Header("Trigger Settings")]
    public int triggerGroupIndex = 9;
    public float delayBeforeTransition = 2f;

    [Header("Audio Fade Settings")]
    public float audioFadeDuration = 2f;

    [Header("Next Scene")]
    public string sceneToLoad = "NextSceneName";

    private bool triggered = false;

    public void OnGroupChanged(int groupIndex)
    {
        if (!triggered && groupIndex == triggerGroupIndex)
        {
            triggered = true;
            StartCoroutine(HandleSceneTransition());
        }
    }

    private IEnumerator HandleSceneTransition()
    {
        // Wait before beginning fade
        yield return new WaitForSeconds(delayBeforeTransition);

        // Fade out audio
        yield return StartCoroutine(FadeOutAllAudio(audioFadeDuration));

        // Load next scene
        SceneManager.LoadScene(sceneToLoad);
    }

    private IEnumerator FadeOutAllAudio(float duration)
    {
        AudioSource[] audioSources = FindObjectsOfType<AudioSource>();

        // Record starting volumes
        float[] startVolumes = new float[audioSources.Length];
        for (int i = 0; i < audioSources.Length; i++)
            startVolumes[i] = audioSources[i].volume;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            for (int i = 0; i < audioSources.Length; i++)
            {
                if (audioSources[i] != null)
                    audioSources[i].volume = Mathf.Lerp(startVolumes[i], 0f, t);
            }

            yield return null;
        }
    }
}

        // Ensure c
