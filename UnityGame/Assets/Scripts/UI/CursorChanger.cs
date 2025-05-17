using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorChanger : MonoBehaviour
{
    public Texture2D newCursorSprite;

    void Start()
    {
        ChangeCursor();
    }

    void ChangeCursor()
    {
        Cursor.lockState = CursorLockMode.Confined;

        // The location that clicking actually hits, also positions the clicker
        Vector2 hotSpot = new Vector2();
        // Dividing the width and height by 2 will center it
        hotSpot.x = newCursorSprite.width / 2;
        hotSpot.y = newCursorSprite.height / 2;

        Cursor.SetCursor(newCursorSprite, hotSpot, CursorMode.Auto);
    }
}
