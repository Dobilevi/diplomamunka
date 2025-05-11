using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifePickup : MonoBehaviour
{
    public GameObject pickUpEvent;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Health collidedHealth = other.gameObject.GetComponent<Health>();

        if (collidedHealth != null)
        {
            if (collidedHealth.teamId == 0)
            {
                collidedHealth.currentLives++;
                Destroy(this.gameObject);
                if (pickUpEvent != null)
                {
                    Instantiate(pickUpEvent);
                }
            }
        }
    }
}
