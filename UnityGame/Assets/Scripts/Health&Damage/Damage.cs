using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cpp.Messages;

public class Damage : MonoBehaviour
{
    public Spawnable type;
    public ulong teamId = 0;
    public ulong projectileId = 0;
    public bool isMultiplayerServer = false;

    public int damageAmount = 1;
    public GameObject hitEffect = null;
    public bool destroyAfterDamage = true;
    public bool dealDamageOnTriggerEnter = false;
    public bool dealDamageOnTriggerStay = false;
    public bool dealDamageOnCollision = false;

    private NetworkManagerServer networkManagerServer = null;

    private void Start()
    {
        if (isMultiplayerServer)
        {
            networkManagerServer = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkManagerServer>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (dealDamageOnTriggerEnter)
        {
            DealDamage(collision.gameObject);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (dealDamageOnTriggerStay)
        {
            DealDamage(collision.gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (dealDamageOnCollision)
        {
            DealDamage(collision.gameObject);
        }
    }

    private void DealDamage(GameObject collisionGameObject)
    {
        Debug.Log("DealDamage");
        Health collidedHealth = collisionGameObject.GetComponent<Health>();
        if (collidedHealth != null)
        {
            if (collidedHealth.teamId != teamId)
            {
                collidedHealth.TakeDamage(damageAmount);
                if (hitEffect != null)
                {
                    Instantiate(hitEffect, transform.position, transform.rotation, null);
                }
                if (destroyAfterDamage)
                {
                    if (gameObject.GetComponent<Enemy>() != null)
                    {
                        gameObject.GetComponent<Enemy>().DoBeforeDestroy();
                    }
                    Destroy(gameObject);
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (isMultiplayerServer)
        {
            networkManagerServer.DeleteProjectile(projectileId);
        }
    }
}
