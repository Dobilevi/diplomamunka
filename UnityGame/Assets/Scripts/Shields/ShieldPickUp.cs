using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldPickUp : MonoBehaviour
{
    public GameObject shieldPrefab;
    public float maxLifeTime = 10.0f;
    public GameObject pickUpEvent;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Health collidedHEalth = other.GetComponent<Health>();

        if (collidedHEalth != null)
        {
            if (collidedHEalth.teamId == 0)
            {
                if (shieldPrefab != null)
                {
                    GameObject shield = Instantiate(shieldPrefab, other.transform);
                    Shield shieldComponent = shield.GetComponent<Shield>();
                    shieldComponent.maxLifeTime = maxLifeTime;
                    shieldComponent.teamId = 0;
                    shield.transform.localScale = new Vector2(shield.transform.localScale.x / other.transform.localScale.x, shield.transform.localScale.y / other.transform.localScale.y);
                }
                Destroy(this.gameObject);
                if (pickUpEvent != null)
                {
                    Instantiate(pickUpEvent);
                }
            }
        }
    }
}
