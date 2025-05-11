using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

using Cpp.Messages;

public class EnemyNet : NetworkBehaviour
{
    public float moveSpeed = 5.0f;

    public Transform followTarget = null;
    public float followRange = 10.0f;

    public List<ShootingControllerNet> guns = new List<ShootingControllerNet>();

    private bool isDead = false;

    public enum ShootMode { None, ShootAll };

    public ShootMode shootMode = ShootMode.ShootAll;

    public enum MovementModes { NoMovement, FollowTarget, Scroll };

    public MovementModes movementMode = MovementModes.FollowTarget;

    [SerializeField] private Vector3 scrollDirection = Vector3.right;

    public Spawnable type = Spawnable.Enemy;

    private void LateUpdate()
    {
        if (IsOwner)
        {
            HandleBehaviour();
        }
    }

    private void HandleBehaviour()
    {
        // Check if the target is in range, then move
        if ((followTarget != null) && (followTarget.position - transform.position).magnitude < followRange)
        {
            MoveEnemy();
        }
        else
        {
            if (type == Spawnable.Enemy)
            {
                float minDistance = float.PositiveInfinity;
                GameObject closestPlayer = null;
                foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
                {
                    float distance = (player.transform.position - transform.position).magnitude;
                    if (minDistance > distance)
                    {
                        minDistance = distance;
                        closestPlayer = player;
                    }
                }

                followTarget = closestPlayer?.transform;
            }
            else if (type == Spawnable.Player)
            {
                float minDistance = float.PositiveInfinity;
                GameObject closestOpponent = null;
                foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
                {
                    if (player == gameObject)
                    {
                        continue;
                    }

                    float distance = (player.transform.position - transform.position).magnitude;
                    if (minDistance > distance)
                    {
                        minDistance = distance;
                        closestOpponent = player;
                    }
                }
                foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
                {
                    float distance = (enemy.transform.position - transform.position).magnitude;
                    if (minDistance > distance)
                    {
                        minDistance = distance;
                        closestOpponent = enemy;
                    }
                }

                followTarget = closestOpponent?.transform;
            }
        }

        // Attempt to shoot, according to this enemy's shooting mode
        TryToShoot();
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
                foreach (ShootingControllerNet gun in guns)
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
        float angle = Vector3.SignedAngle((type == Spawnable.Enemy) ? Vector3.down : Vector3.up, (followTarget.position - transform.position).normalized, Vector3.forward);
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
