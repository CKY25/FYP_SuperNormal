using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenuXR : MonoBehaviour
{
    public GameObject canvas;

    public void PauseButtonPressedXR(InputAction.CallbackContext context)
    {
        if (context.performed)
            DisplayPauseMenuXR();
    }

    public void DisplayPauseMenuXR()
    {
        if (!canvas.gameObject.activeSelf)
        {
            canvas.gameObject.SetActive(true);
            canvas.transform.parent.GetChild(1).GetChild(1).gameObject.SetActive(false);
            canvas.transform.parent.GetChild(1).GetChild(2).gameObject.SetActive(false);
        }
        else
        {
            canvas.gameObject.SetActive(false);
            canvas.transform.parent.GetChild(1).GetChild(1).gameObject.SetActive(true);
            canvas.transform.parent.GetChild(1).GetChild(2).gameObject.SetActive(true);
        }
    }
}
