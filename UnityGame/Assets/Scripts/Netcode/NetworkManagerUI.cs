using System;
using System.Collections.Concurrent;
using System.IO;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class NetworkManagerUI : MonoBehaviour
{
    public NetworkObject botPrefab;

    public ConcurrentDictionary<FixedString64Bytes, ushort> takenPlayerNameMap = new ConcurrentDictionary<FixedString64Bytes, ushort>();

    public FixedString64Bytes CheckName(FixedString64Bytes playerName)
    {
        if (takenPlayerNameMap.ContainsKey(playerName))
        {
            takenPlayerNameMap[playerName]++;
            playerName += $" ({takenPlayerNameMap[playerName]})";
        }
        else
        {
            takenPlayerNameMap[playerName] = 1;
        }

        return playerName;
    }

    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback = ConnectionApprovalCheck;
        NetworkManager.Singleton.OnServerStarted += SpawnEnemies;
        NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Port = (ushort)GameManager.instance.port;
        NetworkManager.Singleton.StartHost();
    }

    public void StartServer()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback = ConnectionApprovalCheck;
        NetworkManager.Singleton.OnServerStarted += SpawnEnemies;
        NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Port = (ushort)GameManager.instance.port;
        NetworkManager.Singleton.StartServer();
    }

    public void StartClient()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback = null;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnDisconnect;
        NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address = GameManager.instance.serverAddress;
        NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Port = (ushort)GameManager.instance.serverPort;
        NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Port = (ushort)GameManager.instance.serverPort;
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(GameManager.instance.serverAddress, (ushort)GameManager.instance.serverPort);
        NetworkManager.Singleton.StartClient();
    }

    private void SpawnEnemies()
    {
        for (int i = 0; i < GameManager.instance.botCount; i++)
        {
            NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(botPrefab, NetworkManager.ServerClientId, true, false, true, new Vector2(Random.Range(-10.0f, 10.0f), Random.Range(-10.0f, 10.0f)));
        }
    }

    private void ConnectionApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        Debug.Log("Connection Approval");
        
        if (NetworkManager.Singleton.ConnectedClientsIds.Count >= GameManager.instance.maxPlayers)
        {
            response.Reason = "Server is full!";
            response.Approved = false;
            response.CreatePlayerObject = false;
        }
        else
        {
            response.Approved = true;
            response.CreatePlayerObject = true;

            response.Position = new Vector2(Random.Range(-10.0f, 10.0f), Random.Range(-10.0f, 10.0f));
            response.Rotation = Quaternion.identity;
        }

        response.Pending = false;
    }

    private void OnDisconnect(ulong clientId)
    {
        GameManager.errorMessage = GameManager.ErrorMessage.Disconnected;
        SceneManager.LoadScene("MainMenu");
    }
}
