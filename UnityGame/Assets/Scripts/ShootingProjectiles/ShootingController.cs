using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using Cpp.Messages;
using UnityEngine.SceneManagement;

/// <summary>
/// A class which controls player aiming and shooting
/// </summary>
public class ShootingController : MonoBehaviour
{
    [Header("GameObject/Component References")]
    [Tooltip("The projectile to be fired.")]
    public GameObject projectilePrefab = null;
    [Tooltip("The projectile to be fired.")]
    public GameObject rocketPrefab = null;
    [Tooltip("The transform in the hierarchy which holds projectiles if any")]
    public Transform projectileHolder = null;

    [Header("Input")]
    [Tooltip("Whether this shooting controller is controlled by the player")]
    public bool isPlayerControlled = false;

    [Header("Firing Settings")]
    [Tooltip("The minimum time between projectiles being fired.")]
    public float fireRate = 0.05f;
    public int rocketCount = 3;

    public int RocketCount
    {
        get => rocketCount;
        set => rocketCount = value;
    }

    [Tooltip("The maximum difference between the direction the" +
             " shooting controller is facing and the direction projectiles are launched.")]
    public float projectileSpread = 1.0f;

    // The last time this component was fired
    private float lastFired = Mathf.NegativeInfinity;

    public float LastFired
    {
        set => lastFired = value;
    }

    [Header("Effects")]
    [Tooltip("The effect to create when this fires")]
    public GameObject fireEffect;
    [Tooltip("The effect to create when this fires")]
    public GameObject fireRocketEffect;

    private bool isMultiplayerServer = false;
    private bool isMultiplayerClient = false;
    private NetworkManagerServer networkManagerServer = null;
    private NetworkManagerClient networkManagerClient = null;

    //The input manager which manages player input
    private InputManager inputManager = null;

    /// <summary>
    /// Description:
    /// Standard unity function that runs every frame
    /// Inputs:
    /// none
    /// Returns:
    /// void (no return)
    /// </summary>
    private void Update()
    {
        ProcessInput();
    }

    /// <summary>
    /// Description:
    /// Standard unity function that runs when the script starts
    /// Inputs:
    /// none
    /// Returns:
    /// void (no return)
    /// </summary>
    private void Start()
    {
        isMultiplayerServer = SceneManager.GetActiveScene().name.Equals("LevelMultiplayerServer");
        isMultiplayerClient = SceneManager.GetActiveScene().name.Equals("LevelMultiplayerClient");

        if (isMultiplayerServer)
        {
            networkManagerServer = GameObject.FindWithTag("NetworkManager").GetComponent<NetworkManagerServer>();
        }
        else if (isMultiplayerClient)
        {
            networkManagerClient = GameObject.FindWithTag("NetworkManager").GetComponent<NetworkManagerClient>();
        }

        SetupInput();
    }

    /// <summary>
    /// Description:
    /// Attempts to set up input if this script is player controlled and input is not already correctly set up 
    /// Inputs:
    /// none
    /// Returns:
    /// void (no return)
    /// </summary>
    void SetupInput()
    {
        if (isPlayerControlled)
        {
            if (inputManager == null)
            {
                inputManager = InputManager.instance;
            }
            if (inputManager == null)
            {
                Debug.LogError("Player Shooting Controller can not find an InputManager in the scene, there needs to be one in the " +
                    "scene for it to run");
            }
        }
    }

    /// <summary>
    /// Description:
    /// Reads input from the input manager
    /// Inputs:
    /// None
    /// Returns:
    /// void (no return)
    /// </summary>
    void ProcessInput()
    {
        if (isPlayerControlled)
        {
            if (inputManager.firePressed || inputManager.fireHeld)
            {
                Fire();
            }

            if (inputManager.fireRocketPressed)
            {
                FireRocket();
            }
        }   
    }

    /// <summary>
    /// Description:
    /// Fires a projectile if possible
    /// Inputs: 
    /// none
    /// Returns: 
    /// void (no return)
    /// </summary>
    public void Fire()
    {
        // If the cooldown is over fire a projectile
        if ((Time.timeSinceLevelLoad - lastFired) > fireRate)
        {
            // Launches a projectile
            SpawnProjectile();

            if (fireEffect != null)
            {
                Instantiate(fireEffect, transform.position, transform.rotation, null);
            }

            // Restart the cooldown
            lastFired = Time.timeSinceLevelLoad;
        }
    }

    /// <summary>
    /// Description:
    /// Fires a projectile if possible
    /// Inputs: 
    /// none
    /// Returns: 
    /// void (no return)
    /// </summary>
    public void FireRocket()
    {
        // If the cooldown is over fire a projectile
        if ((rocketCount > 0) && ((Time.timeSinceLevelLoad - lastFired) > fireRate))
        {
            // Launches a projectile
            SpawnRocket();

            if (fireRocketEffect != null)
            {
                Instantiate(fireRocketEffect, transform.position, transform.rotation, null);
            }

            if (!isMultiplayerServer && !isMultiplayerClient)
            {
                // Restart the cooldown
                lastFired = Time.timeSinceLevelLoad;
                rocketCount--;
            }
        }
    }

    /// <summary>
    /// Description:
    /// Spawns a projectile and sets it up
    /// Inputs: 
    /// none
    /// Returns: 
    /// void (no return)
    /// </summary>
    public void SpawnProjectile()
    {
        // Check that the prefab is valid
        if (projectilePrefab != null)
        {
            if (isMultiplayerClient)
            {
                networkManagerClient.Shoot(Spawnable.Fire, new Vector3(transform.position.x, transform.position.y, transform.rotation.eulerAngles.z));
            }
            else if (isMultiplayerServer)
            {
                // Create the projectile
                GameObject projectileGameObject = Instantiate(projectilePrefab, transform.position, transform.rotation, projectileHolder);

                ulong projectileId = networkManagerServer.NextProjectileId;
                networkManagerServer.Shoot(projectileGameObject, Spawnable.EnemyFire, projectileId);

                // Account for spread
                Vector3 rotationEulerAngles = projectileGameObject.transform.rotation.eulerAngles;
                rotationEulerAngles.z += Random.Range(-projectileSpread, projectileSpread);
                projectileGameObject.transform.rotation = Quaternion.Euler(rotationEulerAngles);

                projectileGameObject.GetComponent<Damage>().teamId = 0;
                projectileGameObject.GetComponent<Damage>().projectileId = projectileId;
                projectileGameObject.GetComponent<Damage>().isMultiplayerServer = true;
            }
            else
            {
                // Create the projectile
                GameObject projectileGameObject = Instantiate(projectilePrefab, transform.position, transform.rotation, projectileHolder);

                // Account for spread
                Vector3 rotationEulerAngles = projectileGameObject.transform.rotation.eulerAngles;
                rotationEulerAngles.z += Random.Range(-projectileSpread, projectileSpread);
                projectileGameObject.transform.rotation = Quaternion.Euler(rotationEulerAngles);
            }
        }
    }

    public void SpawnRocket()
    {
        // Check that the prefab is valid
        if (rocketPrefab != null)
        {
            if (isMultiplayerServer || isMultiplayerClient)
            {
                if (!isMultiplayerServer)
                {
                    networkManagerClient.Shoot(Spawnable.Rocket, new Vector3(transform.position.x, transform.position.y, transform.rotation.eulerAngles.z));
                }
            }
            else
            {
                // Create the projectile
                GameObject rocketGameObject = Instantiate(rocketPrefab, transform.position, transform.rotation, projectileHolder);

                // Account for spread
                Vector3 rotationEulerAngles = rocketGameObject.transform.rotation.eulerAngles;
                rotationEulerAngles.z += Random.Range(-projectileSpread, projectileSpread);
                rocketGameObject.transform.rotation = Quaternion.Euler(rotationEulerAngles);
            }
        }
    }
}
