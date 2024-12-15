using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

public class InputManager : MonoBehaviour
{

    void Start()
    {
        // Initialize input system based on the device connected
        StartCoroutine(InitializeInputSystem());
    }

    private IEnumerator InitializeInputSystem()
    {
        yield return new WaitForSeconds(3);
        if (isPresent())
        {
            SceneManager.LoadScene(2, LoadSceneMode.Single);
        }
        else
        {
            SceneManager.LoadScene(1, LoadSceneMode.Single);
        }
    }

    public static bool isPresent()
    {
        var xrDisplaySubsystems = new List<XRDisplaySubsystem>();
        SubsystemManager.GetInstances<XRDisplaySubsystem>(xrDisplaySubsystems);
        foreach (var xrDisplay in xrDisplaySubsystems)
        {
            if (xrDisplay.running)
            {
                return true;
            }
        }
        return false;
    }
}
