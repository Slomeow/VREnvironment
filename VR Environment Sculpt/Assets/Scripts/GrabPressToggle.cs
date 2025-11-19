using UnityEngine;
using Oculus.Interaction;

public class GrabPressToggle : MonoBehaviour
{
    [Header("Pull Settings")]
    public float pullDistance = 0.1f; // meters
    public float pullSpeed = 7f;

    [Header("Target Object to Toggle")]
    public GameObject targetToToggle;

    [Header("Optional Sound")]
    public AudioClip pullSound;
    private AudioSource audioSource;

    private Vector3 initialLocalPos;
    private bool isPulled = false;
    private Grabbable grabbable;

    void Start()
    {
        initialLocalPos = transform.localPosition;

        // Set up audio if any
        if (pullSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = pullSound;
            audioSource.playOnAwake = false;
        }

        // Set up Grabbable
        grabbable = GetComponent<Grabbable>();
        if (grabbable == null)
        {
            Debug.LogError("PullString_MetaSDK: No Grabbable component found!");
            enabled = false;
            return;
        }

        grabbable.WhenPointerEventRaised += OnPointerEvent;
    }

    private void OnDestroy()
    {
        if (grabbable != null)
            grabbable.WhenPointerEventRaised -= OnPointerEvent;
    }

    private void OnPointerEvent(PointerEvent evt)
    {
        // Trigger pull animation when the string is grabbed
        if (evt.Type == PointerEventType.Select && !isPulled)
        {
            StartCoroutine(PullAnimation());
        }
    }

    private System.Collections.IEnumerator PullAnimation()
    {
        isPulled = true;

        if (audioSource != null)
            audioSource.Play();

        Vector3 pulledPos = initialLocalPos + Vector3.down * pullDistance;

        // Animate pull down
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * pullSpeed;
            Vector3 newPos = Vector3.Lerp(initialLocalPos, pulledPos, t);
            transform.localPosition = new Vector3(initialLocalPos.x, newPos.y, initialLocalPos.z);
            yield return null;
        }

        // Toggle target object
        if (targetToToggle != null)
            targetToToggle.SetActive(!targetToToggle.activeSelf);

        // Animate return
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * pullSpeed;
            Vector3 newPos = Vector3.Lerp(pulledPos, initialLocalPos, t);
            transform.localPosition = new Vector3(initialLocalPos.x, newPos.y, initialLocalPos.z);
            yield return null;
        }

        transform.localPosition = initialLocalPos;
        isPulled = false;
    }

    void LateUpdate()
    {
        // Prevent any grab movement outside Y-axis
        transform.localPosition = new Vector3(initialLocalPos.x, transform.localPosition.y, initialLocalPos.z);
    }
}
