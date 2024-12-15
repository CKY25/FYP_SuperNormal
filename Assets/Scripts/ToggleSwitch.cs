using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class ToggleSwitch : MonoBehaviour, IPointerDownHandler
{
    [SerializeField]
    private bool _isOn = false;
    public bool isOn
    {
        get { return _isOn; }
        set { _isOn = value; }
    }

    [SerializeField]
    private RectTransform toggleIndicator;
    [SerializeField]
    private Image backgroundImage;

    private float offX;
    private float onX;
    [SerializeField]
    private float tweenTime = 0.25f;
    
    public delegate void ValueChanged(bool value);
    public event ValueChanged valueChanged;

    // Start is called before the first frame update
    void Start()
    {
        offX = toggleIndicator.anchoredPosition.x;
        onX = backgroundImage.rectTransform.rect.width - toggleIndicator.rect.width;
    }

    private void OnEnable()
    {
        Toggle(isOn);
    }

    private void Toggle(bool value, bool playSFX = true)
    {
        if(value != isOn)
        {
            _isOn = value;

            MoveIndicator(isOn);

            if (valueChanged != null)
                valueChanged(isOn);
        }
    }

    private void MoveIndicator(bool value)
    {
        if (value)
            toggleIndicator.DOAnchorPosX(onX, tweenTime);
        else
            toggleIndicator.DOAnchorPosX(offX, tweenTime);
    }    

    public void OnPointerDown(PointerEventData eventData)
    {
        Toggle(!isOn);
    }
    
    public void resetToggle()
    {
        isOn = false;
        MoveIndicator(isOn);
    }
}
