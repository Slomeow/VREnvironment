using UnityEngine;

public class BreathingFloat : MonoBehaviour
{
    [Header("Breathing Settings")]
    public float amplitude = 0.1f;   // How far up/down it moves
    public float frequency = 1f;     // Speed of breathing cycle

    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.localPosition;
    }

    private void Update()
    {
        float yOffset = Mathf.Sin(Time.time * frequency) * amplitude;
        transform.localPosition = new Vector3(
            startPos.x,
            startPos.y + yOffset,
            startPos.z
        );
    }
}
