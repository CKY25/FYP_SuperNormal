using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class InputActionManagerXR : MonoBehaviour
{
    public GameObject inputActionManager;

    void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }

    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //ReinitializeXRDevices();
        inputActionManager.GetComponent<InputActionManager>().enabled = true;
    }

    void ReinitializeXRDevices()
    {
        // Disable and enable XR to refresh settings
        XRSettings.enabled = false;
        XRSettings.enabled = true;

        // Check and reset input devices
        var inputDevices = new List<InputDevice>();
        InputDevices.GetDevices(inputDevices);

        // Reinitialize XRInputSubsystem if necessary
        XRInputSubsystem inputSubsystem = GetXRInputSubsystem();
        if (inputSubsystem != null)
        {
            inputSubsystem.TryRecenter();
        }
    }

    XRInputSubsystem GetXRInputSubsystem() 
    {
        List<XRInputSubsystem> subsystems = new List<XRInputSubsystem>(); 
        SubsystemManager.GetInstances(subsystems); if (subsystems.Count > 0) 
        { 
            return subsystems[0]; 
        } 
        return null; 
    }
}
