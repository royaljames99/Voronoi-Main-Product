using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class colorInputBox : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<TMP_InputField>().contentType = TMP_InputField.ContentType.Alphanumeric;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void setValue(string s)
    {
        GetComponent<TMP_InputField>().text = s;
    }
}
