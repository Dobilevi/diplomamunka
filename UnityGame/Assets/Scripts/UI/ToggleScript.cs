using UnityEngine;
using UnityEngine.UI;

public class ToggleScript : MonoBehaviour
{
    public void SetInitialValue(int value)
    {
        GetComponent<Toggle>().isOn = value != 0;
    }

    public void SetValue(bool value)
    {
        GameManager.instance.aiControlled = value ? 1 : 0;
        PlayerPrefs.SetInt("ai_control", value ? 1 : 0);
    }
}
