using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NonToggleableButton : MonoBehaviour, IPointerClickHandler
{
    public int buttonID;
    public bool disabled = false;
    public GameObject VoronoiMaster;

    // Start is called before the first frame update
    void Start()
    {
        if (disabled)
        {
            GetComponent<Button>().interactable = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
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

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!disabled)
        {
            VoronoiMaster.GetComponent<VoronoiMaster>().ButtonInputs(buttonID);
        }
    }
}
