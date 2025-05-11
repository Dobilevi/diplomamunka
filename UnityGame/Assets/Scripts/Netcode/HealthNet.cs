using Unity.Netcode;
using UnityEngine;

public class HealthNet : NetworkBehaviour
{
    public ulong teamId = 0;

    public int defaultHealth = 1;
    public int maximumHealth = 1;
    public int currentHealth = 1;
    public float invincibilityTime = 3f;
    public bool isAlwaysInvincible = false;

    public bool useLives = false;
    public int currentLives = 3;
    public int maximumLives = 5;

    void Start()
    {
        if (!CompareTag("Enemy"))
        {
            teamId = OwnerClientId + 1;
        }

        if (IsOwner)
        {
            SetRespawnPoint(transform.position);
        }
    }

    void Update()
    {
        InvincibilityCheck();
    }

    private float timeToBecomeDamagableAgain = 0;
    private bool isInvincableFromDamage = false;

    private void InvincibilityCheck()
    {
        if (timeToBecomeDamagableAgain <= Time.time)
        {
            isInvincableFromDamage = false;
        }
    }

    private Vector3 respawnPosition;

    private void SetRespawnPoint(Vector3 newRespawnPosition)
    {
        respawnPosition = newRespawnPosition;
    }

    [ClientRpc]
    public void RespawnClientRpc()
    {
        if (IsOwner && CompareTag("Player"))
        {
            GetComponent<ClientNetworkTransform>().Teleport(respawnPosition, Quaternion.identity, Vector3.one);
        }
    }

    [ClientRpc]
    void HitEffectClientRpc()
    {
        Instantiate(hitEffect, transform.position, transform.rotation, null);
    }

    public void TakeDamage(int damageAmount)
    {
        if (isInvincableFromDamage || isAlwaysInvincible)
        {
            return;
        }
        else
        {
            if (hitEffect != null)
            {
                HitEffectClientRpc();
                Instantiate(hitEffect, transform.position, transform.rotation, null);
            }
            timeToBecomeDamagableAgain = Time.time + invincibilityTime;
            isInvincableFromDamage = true;
            currentHealth -= damageAmount;
            CheckDeath();
        }
    }

    public GameObject deathEffect;
    public GameObject hitEffect;

    bool CheckDeath()
    {
        if (currentHealth <= 0)
        {
            Die();
            return true;
        }
        return false;
    }

    [ClientRpc]
    void DeathEffectClientRpc()
    {
        Instantiate(deathEffect, transform.position, transform.rotation, null);
    }

    public void Die()
    {
        if (deathEffect != null)
        {
            DeathEffectClientRpc();
            Instantiate(deathEffect, transform.position, transform.rotation, null);
        }

        if (useLives)
        {
            HandleDeathWithLives();
        }
        else
        {
            HandleDeathWithoutLives();
        }      
    }

    void HandleDeathWithLives()
    {
        RespawnClientRpc();
    }

    void HandleDeathWithoutLives()
    {
        if (gameObject.GetComponent<Enemy>() != null)
        {
            gameObject.GetComponent<Enemy>().DoBeforeDestroy();
        }
        GetComponent<NetworkObject>().Despawn();
    }
}
