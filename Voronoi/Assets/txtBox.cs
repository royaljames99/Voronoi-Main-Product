using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class txtBox : MonoBehaviour
{
    public string output =  "";

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<TMP_InputField>().contentType = TMP_InputField.ContentType.IntegerNumber;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setValue(int s)
    {
        GetComponent<TMP_InputField>().text = Convert.ToString(s);
    }
}
