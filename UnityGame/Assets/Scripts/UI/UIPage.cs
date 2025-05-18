using UnityEngine;

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
