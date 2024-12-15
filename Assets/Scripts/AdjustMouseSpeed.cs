using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class AdjustMouseSpeed : MonoBehaviour
{
    [SerializeField] private Slider senseSlider;

    void Start()
    {
        if (!PlayerPrefs.HasKey("sense"))
        {
            PlayerPrefs.SetFloat("sense", 0.2f);
            Load();
        }
        else
        {
            Load();
        }
    }

    public void changeSense()
    {
        if (!isPresent())
        {
            PlayerController controller = transform.parent.parent.parent.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.lookSpeed = senseSlider.value * 10;
                Save();
            }
        }
        else
        {
            transform.parent.parent.parent.GetChild(10).GetChild(0).GetComponent<ActionBasedContinuousTurnProvider>().turnSpeed = senseSlider.value * 100;
            Save();
        }
    }

    private void Load()
    {
        senseSlider.value = PlayerPrefs.GetFloat("sense");
    }

    public void Save()
    {
        PlayerPrefs.SetFloat("sense", senseSlider.value);
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
