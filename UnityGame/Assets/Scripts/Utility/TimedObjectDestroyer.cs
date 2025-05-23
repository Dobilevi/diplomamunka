﻿using UnityEngine;

public class TimedObjectDestroyer : MonoBehaviour
{
    public float lifetime = 5.0f;

    // The amount of time this gameobject has already existed in play mode
    private float timeAlive = 0.0f;

    public bool destroyChildrenOnDeath = true;

    // Flag which tells whether the application is shutting down (helps avoid errors)
    public static bool quitting = false;

    private void OnApplicationQuit()
    {
        quitting = true;
        DestroyImmediate(gameObject);
    }

    void Update()
    {
        if (timeAlive > lifetime)
        {
            Destroy(gameObject);
        }
        else
        {
            timeAlive += Time.deltaTime;
        }
    }

    private void OnDestroy()
    {
        if (destroyChildrenOnDeath && !quitting && Application.isPlaying)
        {
            int childCount = transform.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                GameObject childObject = transform.GetChild(i).gameObject;
                if (childObject != null)
                {
                    DestroyImmediate(childObject);
                }
            }
        }
        transform.DetachChildren();
    }
}
