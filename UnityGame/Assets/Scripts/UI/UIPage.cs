using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPage : MonoBehaviour
{
    public GameObject defaultSelected;

    public void SetSelectedUIToDefault()
    {
        if (defaultSelected != null)
        {
            GameManager.instance.uiManager.eventSystem.SetSelectedGameObject(null);
            GameManager.instance.uiManager.eventSystem.SetSelectedGameObject(defaultSelected);
        }
        
    }
}
