using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class InputFieldGrabber : MonoBehaviour
{
    [SerializeField]public string pref;
    [SerializeField]private string inputText;

    public void SetInputField(string value)
    {
        inputText = value;
        GetComponent<TMPro.TMP_InputField>().text = value;
    }

    public void GrabFromInputField(string input)
    {
        inputText = input;
        GameManager.instance.SavePref(pref, input);
    }
}
