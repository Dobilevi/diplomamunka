using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour
{
    public ulong teamId = 0;
    public ulong clientId = 0;
    private bool isMultiplayerServer = false;

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
        isMultiplayerServer = SceneManager.GetActiveScene().name.Equals("LevelMultiplayerServer");

        SetRespawnPoint(transform.position);
    }

    void Update()
    {
        InvincibilityCheck();
    }

    // The specific game time when the health can be damged again
    private float timeToBecomeDamagableAgain = 0;
    // Whether or not the health is invincible
    private bool isInvincableFromDamage = false;

    private void InvincibilityCheck()
    {
        if (timeToBecomeDamagableAgain <= Time.time)
        {
            isInvincableFromDamage = false;
        }
    }

    // The position that the health's gameobject will respawn at if lives are being used
    private Vector2 respawnPosition;
    public void SetRespawnPoint(Vector2 newRespawnPosition)
    {
        respawnPosition = newRespawnPosition;
    }

    public void Respawn()
    {
        if (isMultiplayerServer)
        {
            GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkManagerServer>().Respawn(clientId, respawnPosition);
        }
        transform.position = respawnPosition;
        currentHealth = defaultHealth;
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
                Instantiate(hitEffect, transform.position, transform.rotation, null);
            }
            timeToBecomeDamagableAgain = Time.time + invincibilityTime;
            isInvincableFromDamage = true;
            currentHealth -= damageAmount;
            CheckDeath();
        }
    }

    public void ReceiveHealing(int healingAmount)
    {
        currentHealth += healingAmount;
        if (currentHealth > maximumHealth)
        {
            currentHealth = maximumHealth;
        }
        CheckDeath();
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

    public void Die()
    {
        if (deathEffect != null)
        {
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
        if (isMultiplayerServer)
        {
            Respawn();
            return;
        }

        currentLives -= 1;
        if (currentLives > 0)
        {
            Respawn();
        }
        else
        {
            if (gameObject.tag == "Player" && GameManager.instance != null)
            {
                GameManager.instance.GameOver();
            }
            if (gameObject.GetComponent<Enemy>() != null)
            {
                gameObject.GetComponent<Enemy>().DoBeforeDestroy();
            }
            Destroy(gameObject);
        }
    }

    void HandleDeathWithoutLives()
    {
        if (isMultiplayerServer)
        {
            GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkManagerServer>().Despawn(clientId);
            return;
        }

        if (gameObject.tag == "Player" && GameManager.instance != null)
        {
            GameManager.instance.GameOver();
        }
        if (gameObject.GetComponent<Enemy>() != null)
        {
            gameObject.GetComponent<Enemy>().DoBeforeDestroy();
        }
        Destroy(gameObject);
    }
}
