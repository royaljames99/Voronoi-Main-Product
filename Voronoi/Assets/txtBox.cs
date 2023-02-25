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
        GetComponent<TMP_InputField>().text = "12";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void valueChanged()
    {
        string v = GetComponent<TMP_InputField>().text;
        Debug.Log(v);
    }
}
