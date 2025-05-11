using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class Shield : MonoBehaviour
{
    public ulong teamId = 0;
    public float rotationSpeed = 0.0f;
    public float maxLifeTime = 0.0f;
    private float lifeTime = 0.0f;

    void Update()
    {
        this.transform.Rotate(new Vector3(0, 0, 1), rotationSpeed);
        if (maxLifeTime > 0.0f)
        {
            lifeTime += Time.deltaTime;
            if (lifeTime >= maxLifeTime)
            {
                Destroy(this.gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Damage damage = other.gameObject.GetComponent<Damage>();

        if (damage != null)
        {
            if (damage.teamId != this.teamId)
            {
                if (damage.hitEffect != null)
                {
                    Instantiate(damage.hitEffect, other.transform.position, other.transform.rotation, null);
                }

                Health collidedHealth = other.GetComponent<Health>();

                if (collidedHealth != null)
                {
                    collidedHealth.Die();
                }
                else
                {
                    Destroy(other.gameObject);
                }
            }
        }
    }
}
