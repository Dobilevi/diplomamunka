using System;
using Cpp;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

using Cpp.Messages;

public class ControllerNet : NetworkBehaviour
{
    public RuntimeAnimatorController animator = null;
    public Rigidbody2D myRigidbody = null;

    public float moveSpeed = 10.0f;
    public float rotationSpeed = 60f;

    private InputManager inputManager;

    public enum AimModes { AimTowardsMouse, AimForwards };

    public AimModes aimMode = AimModes.AimTowardsMouse;

    public enum MovementModes { MoveHorizontally, MoveVertically, FreeRoam, Astroids };

    public MovementModes movementMode = MovementModes.FreeRoam;

    private bool aiControlled = false;
    public GameObject playerNamePrefab = null;
    private NetworkVariable<FixedString64Bytes> playerName = new NetworkVariable<FixedString64Bytes>("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private GameObject playerNameObject = null;

    public NetworkObject botPrefab;
    private static bool firstPlayerJoined = true;

    private void OnPlayerNameChanged(FixedString64Bytes oldValue, FixedString64Bytes newValue)
    {
        playerNameObject.GetComponentInChildren<Text>().text = newValue.Value;
    }

    // Whether the player can aim with the mouse or not
    private bool canAimWithMouse
    {
        get
        {
            return aimMode == AimModes.AimTowardsMouse;
        }
    }

    // Whether the player's X coordinate is locked (Also assign in rigidbody)
    private bool lockXCoordinate
    {
        get
        {
            return movementMode == MovementModes.MoveVertically;
        }
    }
    // Whether the player's Y coordinate is locked (Also assign in rigidbody)
    public bool lockYCoordinate
    {
        get
        {
            return movementMode == MovementModes.MoveHorizontally;
        }
    }

    [ServerRpc]
    private void SpawnServerRpc()
    {
        if (firstPlayerJoined)
        {
            firstPlayerJoined = false;
            Debug.Log("SpawnServerRpc");
            for (int i = 0; i < GameManager.instance.botCount; i++)
            {
                // Debug.Log($"NetworkManager.IsListening: {NetworkManager.IsListening}");
                // Debug.Log($"Spawning bot {i}");
                // Debug.Log(NetworkManager.Singleton);
                // Debug.Log(botPrefab);
                // Debug.Log(NetworkManager.Singleton.SpawnManager);
                NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(botPrefab, NetworkManager.ServerClientId, true, false, false, new Vector2(Random.Range(-10.0f, 10.0f), Random.Range(-10.0f, 10.0f)));
                // NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(GameManager.instance.botPrefab, NetworkManager.ServerClientId, true, false, false);
                // NetworkObject obj = Instantiate(botPrefab);
            }
        }
    }

    [ServerRpc]
    void SetPlayerNameServerRpc(string value)
    {
        Debug.Log("SetPlayerNameServerRpc " + value);

        value = String.Join("", value.Split(' '));
        value = value.Substring(0, Math.Min(value.Length, NetConstants.maxPlayerNameLength));

        value = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkManagerUI>().CheckName(value).ToString();

        playerName.Value = value;
    }

    private void Start()
    {
        // SpawnServerRpc();

        playerNameObject = Instantiate(playerNamePrefab, transform);
        playerName.OnValueChanged = OnPlayerNameChanged;

        if (IsOwner)
        {
            // playerName.Value = GameManager.instance.playerName.Substring(0, Math.Min(32, GameManager.instance.playerName.Length));
            Debug.Log("Start " + GameManager.instance.playerName);
            SetPlayerNameServerRpc(GameManager.instance.playerName);

            if (GameManager.instance.aiControlled == 1)
            {
                GetComponent<EnemyNet>().type = Spawnable.Player;
                GetComponent<ShootingControllerNet>().isPlayerControlled = false;
            }
            else
            {
                SetupInput();
            }
        }

        if (GameManager.instance.aiControlled == 1)
        {
            aiControlled = true;
        }

        playerNameObject.GetComponentInChildren<Text>().text = playerName.Value.Value;
    }

    void Update()
    {
        if (IsOwner && !aiControlled)
        {
            // Collect input and move the player accordingly
            HandleInput();
            // Sends information to an animator component if one is assigned
            SignalAnimator();
        }
    }

    private void SetupInput()
    {
        if (inputManager == null)
        {
            inputManager = InputManager.instance;
        }
        if (inputManager == null)
        {
            Debug.LogWarning("There is no player input manager in the scene, there needs to be one for the Controller to work");
        }
    }

    private void HandleInput()
    {
        // Find the position that the player should look at
        Vector2 lookPosition = GetLookPosition();
        // Get movement input from the inputManager
        Vector3 movementVector = new Vector3(inputManager.horizontalMoveAxis, inputManager.verticalMoveAxis, 0);
        // Move the player
        MovePlayer(movementVector);
        LookAtPoint(lookPosition);
    }

    private void SignalAnimator()
    {
        // Handle Animation
        if (animator != null)
        {

        }
    }

    public Vector2 GetLookPosition()
    {
        Vector2 result = transform.up;
        if (aimMode != AimModes.AimForwards)
        {
            result = new Vector2(inputManager.horizontalLookAxis, inputManager.verticalLookAxis);
        }
        else
        {
            result = transform.up;
        }
        return result;
    }

    private void MovePlayer(Vector3 movement)
    {
        // Set the player's position accordingly

        // Move according to asteroids setting
        if (movementMode == MovementModes.Astroids)
        {

            // If no rigidbody is assigned, assign one
            if (myRigidbody == null)
            {
                myRigidbody = GetComponent<Rigidbody2D>();
            }

            // Move the player using physics
            Vector2 force = transform.up * movement.y * Time.deltaTime * moveSpeed;
            Debug.Log(force);
            myRigidbody.AddForce(force);

            // Rotate the player around the z axis
            Vector3 newRotationEulars = transform.rotation.eulerAngles;
            float zAxisRotation = transform.rotation.eulerAngles.z;
            float newZAxisRotation = zAxisRotation - rotationSpeed * movement.x * Time.deltaTime;
            newRotationEulars = new Vector3(newRotationEulars.x, newRotationEulars.y, newZAxisRotation);
            transform.rotation = Quaternion.Euler(newRotationEulars);

        }
        // Move according to the other settings
        else
        {
            // Don't move in the x if the settings stop us from doing so
            if (lockXCoordinate)
            {
                movement.x = 0;
            }
            // Don't move in the y if the settings stop us from doing so
            if (lockYCoordinate)
            {
                movement.y = 0;
            }
            // Move the player's transform
            transform.position = transform.position + (movement * Time.deltaTime * moveSpeed);
        }
    }

    private void LookAtPoint(Vector3 point)
    {
        if (Time.timeScale > 0)
        {
            // Rotate the player to look at the mouse.
            Vector2 lookDirection = Camera.main.ScreenToWorldPoint(point) - transform.position;

            if (canAimWithMouse)
            {
                transform.up = lookDirection;
            }
            else
            {
                if (myRigidbody != null)
                {
                    myRigidbody.freezeRotation = true;
                }
            }
        }
    }
}
