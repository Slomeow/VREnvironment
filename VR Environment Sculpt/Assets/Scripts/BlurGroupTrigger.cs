using UnityEngine;

public class BlurGroupTrigger : MonoBehaviour
{
    [Header("References")]
    public SequentialTextDisplay sequentialDisplay;
    public VRBlurEffect vrBlurEffect;

    [Header("Blur Window 1")]
    public int blurOnGroup1 = 2;
    public int blurOffGroup1 = 4;

    [Header("Blur Window 2")]
    public int blurOnGroup2 = 5;
    public int blurOffGroup2 = 7;

    void OnEnable()
    {
        sequentialDisplay.onGroupChanged.AddListener(OnGroupChanged);
    }

    void OnDisable()
    {
        sequentialDisplay.onGroupChanged.RemoveListener(OnGroupChanged);
    }

    private void OnGroupChanged(int groupIndex)
    {
        if (groupIndex == blurOnGroup1 || groupIndex == blurOnGroup2)
        {
            vrBlurEffect.StartBlur();
        }
        else if (groupIndex == blurOffGroup1 || groupIndex == blurOffGroup2)
        {
            vrBlurEffect.StartBlurAndRecover();
        }
    }
}