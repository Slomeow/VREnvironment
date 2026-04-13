using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using System.Collections.Generic;

public class ResetScene : MonoBehaviour
{
    private InputDevice rightHandDevice;
    private bool wasButtonPressed = false;
    private bool isReloading = false;

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
        if (!rightHandDevice.isValid)
        {
            InitializeRightHandDevice();
            return;
        }

        rightHandDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out bool isPressed);

        if (isPressed && !wasButtonPressed && !isReloading)
        {
            isReloading = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        wasButtonPressed = isPressed;
    }
}