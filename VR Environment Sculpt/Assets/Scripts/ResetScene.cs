using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using System.Collections.Generic;

public class ResetScene : MonoBehaviour
{
    private InputDevice rightHandDevice;
    private bool wasButtonPressed = false;

    void Start()
    {
        InitializeRightHandDevice();
    }

    void InitializeRightHandDevice()
    {
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);

        if (devices.Count > 0)
        {
            rightHandDevice = devices[0];
        }
    }

    void Update()
    {
        // Try to find the device if it hasn't been found yet
        if (!rightHandDevice.isValid)
        {
            InitializeRightHandDevice();
            return;
        }

        rightHandDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool isPressed);

        // Only trigger on the initial press, not while held down
        if (isPressed && !wasButtonPressed)
        {
            ReloadCurrentScene();
        }

        wasButtonPressed = isPressed;
    }

    void ReloadCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }
}