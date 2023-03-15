using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ToggleableButton : MonoBehaviour, IPointerClickHandler
{
    Image image;
    public Color originalColor;
    public Color checkedColor;
    public Color disabledColor;
    public bool isPressed;
    public int buttonID;
    public GameObject VoronoiMaster;
    public bool unclickable; //can the button be turned off or must another option be selected instead
    public bool disabled = false;

    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
        updateColour();
    }
    public void disableButton()
    {
        disabled = true;
        GetComponent<Button>().interactable = false;
    }
    public void enableButton()
    {
        disabled = false;
        GetComponent<Button>().interactable = true;
    }

    public void toggle()
    {
        isPressed = !isPressed;
        updateColour();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!disabled)
        {
            if (!isPressed | (unclickable && isPressed))
            {
                toggle();
                VoronoiMaster.GetComponent<VoronoiMaster>().ButtonInputs(buttonID);
            }
        }
    }

    private void updateColour()
    {
        if (isPressed)
        {
            image.color = checkedColor;
        }
        else
        {
            image.color = originalColor;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
