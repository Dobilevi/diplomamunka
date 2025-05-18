using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab = null;
    public Transform target = null;

    [Min(0)]
    public float spawnRangeX = 10.0f;
    [Min(0)]
    public float spawnRangeY = 10.0f;

    public int maxSpawn = 20;
    public bool spawnInfinite = true;

    public int spawnNumber = 20;

    // The number of enemies that have been spawned
    private int currentlySpawned = 0;

    public float spawnDelay = 2.5f;

    // The most recent spawn time
    private float lastSpawnTime = Mathf.NegativeInfinity;

    public Transform projectileHolder = null;

    private void Update()
    {
        CheckSpawnTimer();
    }

    private void CheckSpawnTimer()
    {
        // If it is time for an enemy to be spawned
        if (Time.timeSinceLevelLoad > lastSpawnTime + spawnDelay && (currentlySpawned < maxSpawn || spawnInfinite) && (spawnNumber > 0))
        {
            // Determine spawn location
            Vector3 spawnLocation = GetSpawnLocation();

            // Spawn an enemy
            SpawnEnemy(spawnLocation);
        }
    }

    private void SpawnEnemy(Vector3 spawnLocation)
    {
        // Make sure the prefab is valid
        if (enemyPrefab != null)
        {
            // Create the enemy gameobject
            GameObject enemyGameObject = Instantiate(enemyPrefab, spawnLocation, enemyPrefab.transform.rotation, null);
            Enemy enemy = enemyGameObject.GetComponent<Enemy>();
            ShootingController[] shootingControllers = enemyGameObject.GetComponentsInChildren<ShootingController>();

            // Setup the enemy if necessary
            if (enemy != null)
            {
                enemy.followTarget = target;
            }
            foreach (ShootingController gun in shootingControllers)
            {
                gun.projectileHolder = projectileHolder;
            }

            // Incremment the spawn count
            currentlySpawned++;
            spawnNumber--;
            lastSpawnTime = Time.timeSinceLevelLoad;
        }
    }

    protected virtual Vector3 GetSpawnLocation()
    {
        // Get random coordinates
        float x = Random.Range(0 - spawnRangeX, spawnRangeX);
        float y = Random.Range(0 - spawnRangeY, spawnRangeY);
        // Return the coordinates as a vector
        return new Vector3(transform.position.x + x, transform.position.y + y, 0);
    }
}
