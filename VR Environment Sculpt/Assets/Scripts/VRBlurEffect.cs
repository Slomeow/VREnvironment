using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VRBlurEffect : MonoBehaviour
{
    public Volume postProcessVolume;
    public float duration = 1.5f;
    public float maxBlurIntensity = 15f; // Gaussian blur radius

    private DepthOfField _dof;

    void Start()
    {
        postProcessVolume.profile.TryGet(out _dof);

        if (_dof != null)
        {
            _dof.mode.value = DepthOfFieldMode.Gaussian; // Gaussian = cheaper, good for VR
            _dof.gaussianStart.value = 0f;   // Everything starts in focus
            _dof.gaussianEnd.value = 0.01f;  // Tight focus band = blurs most of scene
        }
    }

    public void StartBlur() => StartCoroutine(BlurRoutine());
    public void StartBlurAndRecover() => StartCoroutine(BlurAndRecoverRoutine());

    // Blur in, then hold
    private IEnumerator BlurRoutine()
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = Mathf.Sin((elapsed / duration) * Mathf.PI * 0.5f); // Ease in
            if (_dof != null)
                _dof.gaussianMaxRadius.value = Mathf.Lerp(0f, maxBlurIntensity, t);

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    // Blur in, then recover (good for hits, stuns, waking up)
    private IEnumerator BlurAndRecoverRoutine()
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float curve = Mathf.Sin(t * Mathf.PI); // Peaks at midpoint, returns to 0

            if (_dof != null)
                _dof.gaussianMaxRadius.value = Mathf.Lerp(0f, maxBlurIntensity, curve);

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (_dof != null) _dof.gaussianMaxRadius.value = 0f;
    }
}