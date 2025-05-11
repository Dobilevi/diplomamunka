using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketPickup : MonoBehaviour
{
    public GameObject pickUpEvent;

    private void OnTriggerEnter2D(Collider2D other)
    {
        ShootingController controller = other.gameObject.GetComponent<ShootingController>();

        if (controller != null)
        {
            if (controller.isPlayerControlled)
            {
                controller.rocketCount++;
                Destroy(this.gameObject);
                if (pickUpEvent != null)
                {
                    Instantiate(pickUpEvent);
                }
            }
        }
    }
}
