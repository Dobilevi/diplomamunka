using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class Enemy : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    public int scoreValue = 5;

    public Transform followTarget = null;
    public float followRange = 10.0f;

    public List<ShootingController> guns = new List<ShootingController>();

    private bool isDead = false;

    public enum ShootMode { None, ShootAll };

    public ShootMode shootMode = ShootMode.ShootAll;

    public enum MovementModes { NoMovement, FollowTarget, Scroll };

    public MovementModes movementMode = MovementModes.FollowTarget;

    //The direction that this enemy will try to scroll if it is set as a scrolling enemy.
    [SerializeField] private Vector3 scrollDirection = Vector3.right;

    private bool isMultiplayerServer = false;
    private bool isMultiplayerClient = false;
    private NetworkManagerServer networkManagerServer = null;
    private NetworkManagerClient networkManagerClient = null;

    private void LateUpdate()
    {
        HandleBehaviour();       
    }

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
        else if (movementMode == MovementModes.FollowTarget && followTarget == null)
        {
            if (GameManager.instance != null && GameManager.instance.player != null)
            {
                Debug.Log("Follow target set!");
                followTarget = GameManager.instance.player.transform;
            }
        }
    }

    private void HandleBehaviour()
    {
        // Check if the target is in range, then move
        if (followTarget != null && (followTarget.position - transform.position).magnitude < followRange)
        {
            Debug.Log("MoveEnemy");
            MoveEnemy();
        }
        else if (isMultiplayerServer)
        {
            followTarget = networkManagerServer.GetClosestPlayer(gameObject);
        }
        else if (isMultiplayerClient)
        {
            Debug.Log("Trying to find target!");
            followTarget = networkManagerClient.GetClosestEnemy(gameObject);
        }
        // Attempt to shoot, according to this enemy's shooting mode
        TryToShoot();
    }

    public void DoBeforeDestroy()
    {
        if (!isDead)
        {
            AddToScore();
            IncrementEnemiesDefeated();
            isDead = true;
        }
    }

    private void AddToScore()
    {
        if ((GameManager.instance != null) && !GameManager.instance.gameIsOver)
        {
            GameManager.AddScore(scoreValue);
        }
    }

    private void IncrementEnemiesDefeated()
    {
        if (GameManager.instance != null && !GameManager.instance.gameIsOver)
        {
            GameManager.instance.IncrementEnemiesDefeated();
        }       
    }

    private void MoveEnemy()
    {
        // Determine correct movement
        Vector3 movement = GetDesiredMovement();

        // Determine correct rotation
        Quaternion rotationToTarget = GetDesiredRotation();

        // Move and rotate the enemy
        transform.position += movement;
        transform.rotation = rotationToTarget;
    }

    protected virtual Vector3 GetDesiredMovement()
    {
        Vector3 movement;
        switch(movementMode)
        {
            case MovementModes.FollowTarget:
                movement = GetFollowPlayerMovement();
                break;
            case MovementModes.Scroll:
                movement = GetScrollingMovement();
                break;
            default:
                movement = Vector3.zero;
                break;
        }
        return movement;
    }

    protected virtual Quaternion GetDesiredRotation()
    {
        Quaternion rotation;
        switch (movementMode)
        {
            case MovementModes.FollowTarget:
                rotation = GetFollowPlayerRotation();
                break;
            case MovementModes.Scroll:
                rotation = GetScrollingRotation();
                break;
            default:
                rotation = transform.rotation; ;
                break;
        }
        return rotation;
    }

    private void TryToShoot()
    {
        switch (shootMode)
        {
            case ShootMode.None:
                break;
            case ShootMode.ShootAll:
                foreach (ShootingController gun in guns)
                {
                    gun.Fire();
                }
                break;
        }
    }

    private Vector3 GetFollowPlayerMovement()
    {
        Vector3 moveDirection = (followTarget.position - transform.position).normalized;
        Vector3 movement = moveDirection * moveSpeed * Time.deltaTime;
        return movement;
    }

    private Quaternion GetFollowPlayerRotation()
    {
        float angle = Vector3.SignedAngle(isMultiplayerClient ? Vector3.up : Vector3.down, (followTarget.position - transform.position).normalized, Vector3.forward);
        Quaternion rotationToTarget = Quaternion.Euler(0, 0, angle);
        return rotationToTarget;
    }

    private Vector3 GetScrollingMovement()
    {
        scrollDirection = GetScrollDirection();
        Vector3 movement = scrollDirection * moveSpeed * Time.deltaTime;
        return movement;
    }

    private Quaternion GetScrollingRotation()
    {
        return Quaternion.identity;
    }

    private Vector3 GetScrollDirection()
    {
        Camera camera = Camera.main;
        if (camera != null)
        {
            Vector2 screenPosition = camera.WorldToScreenPoint(transform.position);
            Rect screenRect = camera.pixelRect;
            if (!screenRect.Contains(screenPosition))
            {
                return scrollDirection * -1;
            }
        }
        return scrollDirection;
    }
}
