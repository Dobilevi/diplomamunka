using System;
using System.Collections.Generic;
using System.IO;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class ShootingControllerNet : NetworkBehaviour
{
    public NetworkObject projectilePrefab = null;
    public NetworkObject rocketPrefab = null;
    public Transform projectileHolder = null;

    public bool isPlayerControlled = false;

    public float fireRate = 0.05f;
    public int rocketCount = 3;

    public float projectileSpread = 1.0f;

    private float lastFired = Mathf.NegativeInfinity;

    public GameObject fireEffect;
    public GameObject fireRocketEffect;

    private InputManager inputManager = null;

    public ulong teamId = 0;

    private float start, end;
    List<float> responseTimes = new List<float>();

    private void Update()
    {
        if (IsOwner)
        {
            ProcessInput();
        }
    }

    private void Start()
    {
        if (!CompareTag("Enemy"))
        {
            if (!((transform.parent != null) && (transform.parent.CompareTag("Enemy"))))
            {
                teamId = OwnerClientId + 1;
                GetComponent<EnemyNet>().shootMode = EnemyNet.ShootMode.None;
            }
        }
        SetupInput();
    }

    void SetupInput()
    {
        if (!IsOwner)
        {
            return;
        }

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

    public void Fire()
    {
        // If the cooldown is over fire a projectile
        if ((Time.timeSinceLevelLoad - lastFired) > fireRate)
        {
            start = Time.realtimeSinceStartup;
            // Launches a projectile
            if (IsServer)
            {
                SpawnProjectile();
            }
            else
            {
                SpawnProjectileServerRpc();
            }

            if (fireEffect != null)
            {
                Instantiate(fireEffect,  transform.position, transform.rotation);
            }

            // Restart the cooldown
            lastFired = Time.timeSinceLevelLoad;
        }
    }

    public void FireRocket()
    {
        // If the cooldown is over fire a projectile
        if ((rocketCount > 0) && ((Time.timeSinceLevelLoad - lastFired) > fireRate))
        {
            // Launches a projectile
            SpawnRocketServerRpc();

            if (fireRocketEffect != null)
            {
                Instantiate(fireRocketEffect, transform.position, transform.rotation, null);
                rocketCount--;
            }

            // Restart the cooldown
            lastFired = Time.timeSinceLevelLoad;
        }
    }

	[ClientRpc]
	private void SignalClientRpc() {
        if (IsOwner)
        {
            end = Time.realtimeSinceStartup;
            responseTimes.Add((end - start) * 1000);
            responseTimes.Add(NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetCurrentRtt(NetworkManager.ServerClientId));
        }
	}

    [ServerRpc]
    private void SpawnProjectileServerRpc()
    {
        SpawnProjectile();
    }

    private void SpawnProjectile()
    {
        // Check that the prefab is valid
        if (projectilePrefab != null)
        {
            // Create the projectile
            // GameObject projectileGameObject = Instantiate(projectilePrefab, transform.position, transform.rotation, projectileHolder);
            var projectileGameObject = NetworkManager.SpawnManager.InstantiateAndSpawn(projectilePrefab, OwnerClientId, true, false, true, transform.position, transform.rotation);
            // var projectileGameObject = Instantiate(projectilePrefab);
            // projectileGameObject.GetComponent<NetworkObject>().Spawn(true);

            projectileGameObject.GetComponent<DamageNet>().teamId = teamId;

            // Account for spread
            Vector3 rotationEulerAngles = projectileGameObject.transform.rotation.eulerAngles;
            rotationEulerAngles.z += Random.Range(-projectileSpread, projectileSpread);
            projectileGameObject.transform.rotation = Quaternion.Euler(rotationEulerAngles);

            // Keep the heirarchy organized
            if (projectileHolder != null)
            {
                projectileGameObject.transform.SetParent(projectileHolder);
            }
        }

        SignalClientRpc();
    }

    [ServerRpc]
    public void SpawnRocketServerRpc()
    {
        // Check that the prefab is valid
        if (rocketPrefab != null)
        {
            // Create the projectile
            // GameObject rocketGameObject = Instantiate(rocketPrefab, transform.position, transform.rotation, projectileHolder);
            var rocketGameObject = NetworkManager.SpawnManager.InstantiateAndSpawn(rocketPrefab, OwnerClientId, false, false, true, transform.position, transform.rotation);
            rocketGameObject.GetComponent<DamageNet>().teamId = teamId;

            // Account for spread
            Vector3 rotationEulerAngles = rocketGameObject.transform.rotation.eulerAngles;
            rotationEulerAngles.z += Random.Range(-projectileSpread, projectileSpread);
            rocketGameObject.transform.rotation = Quaternion.Euler(rotationEulerAngles);

            // Keep the heirarchy organized
            if (projectileHolder != null)
            {
                rocketGameObject.transform.SetParent(projectileHolder);
            }
        }
    }

    public override void OnDestroy()
    {
        if (IsOwner && tag.Equals("Player"))
        {
            StreamWriter fs = new StreamWriter($"response_times_netcode_{name}_{DateTime.Now.ToString("yyyyMMddHHmmss")}_{Time.realtimeSinceStartup}.txt");
            fs.WriteLine(responseTimes.Count);
            foreach (var responseTime in responseTimes)
            {
                fs.WriteLine(responseTime);
            }

            fs.Close();
        }
    }
}
