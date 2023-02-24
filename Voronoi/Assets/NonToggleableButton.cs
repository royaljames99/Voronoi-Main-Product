using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NonToggleableButton : MonoBehaviour, IPointerClickHandler
{
    public int buttonID;
    public bool disabled = false;
    public GameObject VoronoiMaster;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!disabled)
        {
            VoronoiMaster.GetComponent<VoronoiMaster>().ButtonInputs(buttonID);
        }
    }
}
