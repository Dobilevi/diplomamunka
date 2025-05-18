using UnityEngine;

public class InputFieldGrabber : MonoBehaviour
{
    public string pref;
    private string inputText;

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
