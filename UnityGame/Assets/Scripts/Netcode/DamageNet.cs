using Unity.Netcode;
using UnityEngine;

public class DamageNet : NetworkBehaviour
{
    public ulong teamId = 0;

    public int damageAmount = 1;
    public GameObject hitEffect = null;
    public bool destroyAfterDamage = true;
    public bool dealDamageOnTriggerEnter = false;
    public bool dealDamageOnTriggerStay = false;
    public bool dealDamageOnCollision = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsServer && dealDamageOnTriggerEnter)
        {
            DealDamage(collision.gameObject);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (IsServer && dealDamageOnTriggerStay)
        {
            DealDamage(collision.gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsServer && dealDamageOnCollision)
        {
            DealDamage(collision.gameObject);
        }
    }

    [ClientRpc]
    void HitEffectClientRpc()
    {
        Instantiate(hitEffect, transform.position, transform.rotation, null);
    }

    private void DealDamage(GameObject collisionGameObject)
    {
        HealthNet collidedHealth = collisionGameObject.GetComponent<HealthNet>();
        if (collidedHealth != null)
        {
            if (collidedHealth.teamId != teamId)
            {
                Debug.Log(collidedHealth.teamId);
                Debug.Log(teamId);
                collidedHealth.TakeDamage(damageAmount);
                if (hitEffect != null)
                {
                    HitEffectClientRpc();
                    Instantiate(hitEffect, transform.position, transform.rotation, null);
                }
                if (destroyAfterDamage)
                {
                    if (gameObject.GetComponent<Enemy>() != null)
                    {
                        gameObject.GetComponent<Enemy>().DoBeforeDestroy();
                    }
                    GetComponent<NetworkObject>().Despawn();
                }
            }
        }
    }
}
