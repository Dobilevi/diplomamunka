using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Pipes;
using UnityEngine;

using Cpp;
using Cpp.Messages;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class NetworkManagerServer : MonoBehaviour
{
    public Transform projectileHolder;

    public int serverUpdateInterval = 50; // ms
    public int clientUpdateInterval = 50;

    public float fireRate = 1.0f;
    public float rocketFireRate = 1.0f;

    public GameObject botPrefab = null;
    public GameObject playerPrefab = null;

    private ulong nextProjectileId = 0;
    public GameObject projectilePrefab = null;
    public GameObject rocketPrefab = null;
    private ConcurrentDictionary<ulong, GameObject> projectileMap = new ConcurrentDictionary<ulong, GameObject>();

    private ConcurrentDictionary<ulong, GameObject> botMap = new ConcurrentDictionary<ulong, GameObject>();

    private ConcurrentDictionary<ulong, bool> respawningMap = new ConcurrentDictionary<ulong, bool>();

    private ulong nextClientId = 0;
    public int maxPlayerCount = 10;
    private ushort playerCount = 0;
    private ConcurrentDictionary<ulong, GameObject> playerMap = new ConcurrentDictionary<ulong, GameObject>();
    private ConcurrentDictionary<ulong, Vector2> playerRespawnPointMap = new ConcurrentDictionary<ulong, Vector2>();
    private ConcurrentDictionary<ulong, float> fireCooldownMap = new ConcurrentDictionary<ulong, float>();
    private ConcurrentDictionary<ulong, ushort> rocketCountMap = new ConcurrentDictionary<ulong, ushort>();
    private ConcurrentDictionary<string, ushort> takenPlayerNameMap = new ConcurrentDictionary<string, ushort>();
    private ConcurrentDictionary<ulong, string> playerNameMap = new ConcurrentDictionary<ulong, string>();

    private ConcurrentQueue<ulong> connectQueue = new ConcurrentQueue<ulong>();
    private ConcurrentQueue<string> connectPlayerNameQueue = new ConcurrentQueue<string>();
    private ConcurrentQueue<UpdateMessage> updateQueue = new ConcurrentQueue<UpdateMessage>();
    private ConcurrentQueue<SpawnProjectileMessage> spawnQueue = new ConcurrentQueue<SpawnProjectileMessage>();
    private ConcurrentQueue<ulong> disconnectQueue = new ConcurrentQueue<ulong>();
    private ConcurrentQueue<ulong> resendRecipientQueue = new ConcurrentQueue<ulong>();
    private ConcurrentQueue<ulong> resendShipQueue = new ConcurrentQueue<ulong>();

    private Task receiveTask = null;
    CancellationTokenSource source = new CancellationTokenSource();
    CancellationTokenSource sendSource = new CancellationTokenSource();

    private bool shouldSendUpdate = false;

#if UNITY_STANDALONE_WIN
    private NamedPipeClientStream pipeClient = null;
    private NamedPipeServerStream pipeServer = null;
#else
    private FileStream pipeClient = null;
    private FileStream pipeServer = null;
#endif

    private NamedPipeStreamWriter namedPipeStreamWriter = null;
    private NamedPipeStreamReader namedPipeStreamReader = null;

    private Timer updateTimer = null;

    private bool shouldExit = false;

    public ulong NextProjectileId => ++nextProjectileId;

    public ulong NextClientId => ++nextClientId;

    public void Shoot(GameObject projectile, Spawnable spawnable, ulong projectileId)
    {
        projectileMap[projectileId] = projectile;

        SpawnProjectileMessage spawnProjectileMessage = new SpawnProjectileMessage
        {
            spawnable = spawnable,
            clientId = 0,
            projectileId = projectileId,
            x = projectile.transform.position.x,
            y = projectile.transform.position.y,
            rotation = projectile.transform.eulerAngles.z
        };

        namedPipeStreamWriter.WriteMessageType(MessageType.SpawnProjectile);
        namedPipeStreamWriter.WriteSpawnProjectileMessage(spawnProjectileMessage);
    }

    public void Respawn(ulong clientId, Vector2 respawnPosition)
    {
        playerMap[clientId].GetComponent<MultiMovement>().Stop();

        // Block Update messages
        respawningMap[clientId] = true;

        namedPipeStreamWriter.WriteMessageType(MessageType.Update);
        namedPipeStreamWriter.WriteUpdateMessage(new UpdateMessage(UpdateType.RESPAWN, clientId, respawnPosition.x, respawnPosition.y, 0));
    }

    public void Despawn(ulong clientId)
    {
        namedPipeStreamWriter.WriteMessageType(MessageType.Despawn);
        namedPipeStreamWriter.WriteDespawnMessage(new DespawnMessage(Spawnable.Enemy, clientId));

        botMap.TryRemove(clientId, out GameObject objectToDestroy);
        Destroy(objectToDestroy);
    }

    public void DeleteProjectile(ulong projectileId)
    {
        projectileMap.TryRemove(projectileId, out _);

        namedPipeStreamWriter.WriteMessageType(MessageType.Despawn);
        namedPipeStreamWriter.WriteDespawnMessage(new DespawnMessage(Spawnable.Fire, projectileId));
    }

    public Transform GetClosestPlayer(GameObject bot)
    {
        if (playerMap.Count == 0)
        {
            return null;
        }

        GameObject closestPlayer = null;
        float closestDist = float.MaxValue;
        foreach (GameObject player in playerMap.Values)
        {
            float dist = (bot.transform.position - player.transform.position).magnitude;
            if (dist < closestDist)
            {
                closestDist = dist;
                closestPlayer = player;
            }
        }

        return closestPlayer?.transform;
    }

    [DoesNotReturn]
    private void ReceiveUpdatesAsync()
    {
        while (true)
        {
            ulong clientId;
            MessageType messageType = namedPipeStreamReader.ReadMessageType();
            UpdateMessage updateMessage = new UpdateMessage();
            SpawnProjectileMessage spawnProjectileMessage = new SpawnProjectileMessage();

            switch (messageType)
            {
                case MessageType.Connect:
                {
                    clientId = NextClientId;
                    ushort length = namedPipeStreamReader.ReadUint16();
                    string playerName = namedPipeStreamReader.ReadString(length);

                    connectPlayerNameQueue.Enqueue(playerName);
                    connectQueue.Enqueue(clientId);

                    break;
                }
                case MessageType.Disconnect:
                {
                    clientId = namedPipeStreamReader.ReadUint64();

                    disconnectQueue.Enqueue(clientId);

                    break;
                }
                case MessageType.Update:
                {
                    namedPipeStreamReader.ReadUpdateMessage(ref updateMessage);

                    if (!respawningMap[updateMessage.clientId])
                    {
                        updateQueue.Enqueue(updateMessage);
                    }

                    break;
                }
                case MessageType.SpawnProjectile:
                {
                    namedPipeStreamReader.ReadSpawnProjectileMessage(ref spawnProjectileMessage);

                    spawnQueue.Enqueue(spawnProjectileMessage);

                    break;
                }
                case MessageType.RespawnAck:
                {
                    clientId = namedPipeStreamReader.ReadUint64();

                    respawningMap[clientId] = false;

                    break;
                }
                case MessageType.ErrorObjectDoesNotExist:
                {
                    resendRecipientQueue.Enqueue(namedPipeStreamReader.ReadUint64());
                    resendShipQueue.Enqueue(namedPipeStreamReader.ReadUint64());

                    break;
                }
                case MessageType.Exit:
                {
                    shouldExit = true;

                    break;
                }
                default:
                {
                    break;
                }
            }
        }
    }

    private void SendUpdates()
    {
        foreach (KeyValuePair<ulong, GameObject> bot in botMap)
        {
            namedPipeStreamWriter.WriteMessageType(MessageType.Update);
            namedPipeStreamWriter.WriteUpdateMessage(new UpdateMessage(UpdateType.MOVE, bot.Key, bot.Value.transform.position.x, bot.Value.transform.position.y, bot.Value.transform.rotation.eulerAngles.z));
        }

        foreach (KeyValuePair<ulong, GameObject> player in playerMap)
        {
            namedPipeStreamWriter.WriteMessageType(MessageType.Update);
            namedPipeStreamWriter.WriteUpdateMessage(new UpdateMessage(UpdateType.MOVE, player.Key, player.Value.transform.position.x, player.Value.transform.position.y, player.Value.transform.rotation.eulerAngles.z));
        }
    }

    private void ShouldSendUpdate(object obj)
    {
        shouldSendUpdate = true;
    }

    void Awake()
    {
        try
        {
#if UNITY_EDITOR_WIN
            Process.Start(@"..\CppServer\build\Debug\CppServer.exe");
#elif UNITY_STANDALONE_WIN
            Process.Start(@"..\..\..\CppServer\build\Debug\CppServer.exe");
#elif UNITY_EDITOR_LINUX
            Process.Start("../CppServer/build/CppServer");
#elif UNITY_STANDALONE_LINUX
            Process.Start("../../../CppServer/build/CppServer");
#endif
        }
        catch (Exception e)
        {
            SceneManager.LoadScene("MainMenu");
            return;
        }

#if UNITY_STANDALONE_WIN
        pipeServer = new NamedPipeServerStream("CppPipe", PipeDirection.In);
        pipeServer.WaitForConnection();
        pipeClient = new NamedPipeClientStream(".", "CsharpPipe", PipeDirection.Out);
        pipeClient.Connect();
#else
        pipeServer = new FileStream("/tmp/CppPipe", FileMode.Open);
        pipeClient = new FileStream("/tmp/CsharpPipe", FileMode.Open);
#endif

        namedPipeStreamWriter = new NamedPipeStreamWriter(pipeClient);
        namedPipeStreamReader = new NamedPipeStreamReader(pipeServer);

        // Send port number to server
        namedPipeStreamWriter.WriteUint16((ushort)GameManager.instance.port);
    }

    void Start()
    {
        maxPlayerCount = GameManager.instance.maxPlayers;

        // Start reading incoming messages
        receiveTask = Task.Factory.StartNew(ReceiveUpdatesAsync, source.Token);

        // Start
        updateTimer = new Timer(ShouldSendUpdate, null, new TimeSpan(0, 0, 0, 0, 0), new TimeSpan(0, 0, 0, 0, serverUpdateInterval));

        for (int i = 0; i < GameManager.instance.botCount; i++)
        {
            ulong clientId = NextClientId;
            botMap[clientId] = Instantiate(botPrefab, new Vector2(Random.Range(-10.0f, 10.0f), Random.Range(-10.0f, 10.0f)), Quaternion.identity);
            botMap[clientId].GetComponent<Health>().clientId = clientId;
        }
    }

    private void ProcessMessages()
    {
        ulong clientId;
        UpdateMessage updateMessage = new UpdateMessage();
        SpawnMessage spawnMessage = new SpawnMessage();
        SpawnProjectileMessage spawnProjectileMessage = new SpawnProjectileMessage();

        for (int i = 0; i < resendShipQueue.Count; i++)
        {
            resendShipQueue.TryDequeue(out ulong clientIdOf);
            resendRecipientQueue.TryDequeue(out ulong clientIdTo);

            if (playerMap.ContainsKey(clientIdOf))
            {
                GameObject playerOf = playerMap[clientIdOf];
                
                namedPipeStreamWriter.WriteMessageType(MessageType.InitializePlayer);
                namedPipeStreamWriter.WriteUint64(clientIdTo);

                spawnMessage = new SpawnMessage
                {
                    clientId = clientIdOf,
                    x = playerOf.transform.position.x,
                    y = playerOf.transform.position.y,
                    rotation = playerOf.transform.rotation.eulerAngles.z
                };

                namedPipeStreamWriter.WriteSpawnMessage(spawnMessage);

                continue;
            }

            if (botMap.ContainsKey(clientIdOf))
            {
                GameObject playerOf = botMap[clientIdOf];
                
                namedPipeStreamWriter.WriteMessageType(MessageType.InitializeEnemy);
                namedPipeStreamWriter.WriteUint64(clientIdTo);

                spawnMessage = new SpawnMessage
                {
                    clientId = clientIdOf,
                    x = playerOf.transform.position.x,
                    y = playerOf.transform.position.y,
                    rotation = playerOf.transform.rotation.eulerAngles.z
                };

                namedPipeStreamWriter.WriteSpawnMessage(spawnMessage);

                continue;
            }
        }

        for (int i = 0; i < connectQueue.Count; i++)
        {
            connectPlayerNameQueue.TryDequeue(out string playerName);
            connectQueue.TryDequeue(out clientId);

            if (playerCount >= maxPlayerCount)
            {
                namedPipeStreamWriter.WriteMessageType(MessageType.ErrorServerFull);
                namedPipeStreamWriter.WriteUint64(clientId);
                continue;
            }

            playerCount++;

            playerRespawnPointMap[clientId] = new Vector2(Random.Range(-10.0f, 10.0f), Random.Range(-10.0f, 10.0f));

            playerMap[clientId] = Instantiate(playerPrefab, playerRespawnPointMap[clientId],  Quaternion.identity);

            playerMap[clientId].GetComponent<Health>().teamId = clientId;
            playerMap[clientId].GetComponent<Health>().clientId = clientId;
            respawningMap[clientId] = false;
            playerMap[clientId].transform.position = playerRespawnPointMap[clientId];
            playerMap[clientId].GetComponent<Health>().SetRespawnPoint(playerRespawnPointMap[clientId]);

            playerName = String.Join("", playerName.Split(' '));
            if (takenPlayerNameMap.ContainsKey(playerName))
            {
                takenPlayerNameMap[playerName]++;
                playerName += $" ({takenPlayerNameMap[playerName]})";
            }
            else
            {
                takenPlayerNameMap[playerName] = 1;
            }
            playerNameMap[clientId] = playerName;
            playerMap[clientId].GetComponentInChildren<Text>().text = playerName;

            fireCooldownMap[clientId] = fireCooldownMap[clientId] = Time.timeSinceLevelLoad;
            rocketCountMap[clientId] = 3;

            namedPipeStreamWriter.WriteMessageType(MessageType.Connect);

            spawnMessage = new SpawnMessage
            {
                clientId = clientId,
                x = playerRespawnPointMap[clientId].x,
                y = playerRespawnPointMap[clientId].y,
                rotation = 0
            };

            namedPipeStreamWriter.WriteSpawnMessage(spawnMessage);
            namedPipeStreamWriter.WriteUint16((ushort)playerName.Length);
            namedPipeStreamWriter.WriteString(playerName);

            foreach (var botPair in botMap)
            {
                namedPipeStreamWriter.WriteMessageType(MessageType.InitializeEnemy);
                namedPipeStreamWriter.WriteUint64(clientId);

                spawnMessage = new SpawnMessage
                {
                    clientId = botPair.Key,
                    x = botPair.Value.transform.position.x,
                    y = botPair.Value.transform.position.y,
                    rotation = botPair.Value.transform.rotation.eulerAngles.z
                };

                namedPipeStreamWriter.WriteSpawnMessage(spawnMessage);
            }

            foreach (var playerPair in playerMap)
            {
                if (clientId == playerPair.Key)
                {
                    continue;
                }

                namedPipeStreamWriter.WriteMessageType(MessageType.InitializePlayer);
                namedPipeStreamWriter.WriteUint64(clientId);

                spawnMessage = new SpawnMessage
                {
                    clientId = playerPair.Key,
                    x = playerPair.Value.transform.position.x,
                    y = playerPair.Value.transform.position.y,
                    rotation = playerPair.Value.transform.rotation.eulerAngles.z
                };

                namedPipeStreamWriter.WriteSpawnMessage(spawnMessage);
                namedPipeStreamWriter.WriteUint16((ushort)playerNameMap[playerPair.Key].Length);
                namedPipeStreamWriter.WriteString(playerNameMap[playerPair.Key]);
            }

            foreach (var projectile in projectileMap)
            {
                namedPipeStreamWriter.WriteMessageType(MessageType.InitializeProjectile);
                namedPipeStreamWriter.WriteUint64(clientId);

                Damage damage = projectile.Value.GetComponent<Damage>();
                spawnProjectileMessage = new SpawnProjectileMessage
                {
                    spawnable = damage.type,
                    clientId = damage.teamId,
                    projectileId = projectile.Key,
                    x = projectile.Value.transform.position.x,
                    y = projectile.Value.transform.position.y,
                    rotation = projectile.Value.transform.rotation.eulerAngles.z
                };

                namedPipeStreamWriter.WriteSpawnProjectileMessage(spawnProjectileMessage);
            }
        }

        GameObject player;
        for (int i = 0; i < updateQueue.Count; i++)
        {
            updateQueue.TryDequeue(out updateMessage);

            player = playerMap[updateMessage.clientId];

            switch (updateMessage.updateType)
            {
                case UpdateType.MOVE:
                {
                    player.GetComponent<MultiMovement>().MoveToPoint(new Vector2(updateMessage.x, updateMessage.y), updateMessage.rotation);
                    break;
                }
                case UpdateType.TELEPORT:
                {
                    player.GetComponent<MultiMovement>().Stop();
                    player.transform.position = new Vector2(updateMessage.x, updateMessage.y);
                    player.GetComponent<Rigidbody2D>().MoveRotation(updateMessage.rotation);
                    break;
                }
                default:
                {
                    break;
                }
            }
        }

        for (int i = 0; i < spawnQueue.Count; i++)
        {
            spawnQueue.TryDequeue(out spawnProjectileMessage);

            spawnProjectileMessage.projectileId = NextProjectileId;
            switch (spawnProjectileMessage.spawnable)
            {
                case Spawnable.Fire:
                {
                    if ((Time.timeSinceLevelLoad - fireCooldownMap[spawnProjectileMessage.clientId]) > fireRate)
                    {
                        namedPipeStreamWriter.WriteMessageType(MessageType.SpawnProjectile);
                        namedPipeStreamWriter.WriteSpawnProjectileMessage(spawnProjectileMessage);

                        GameObject obj = Instantiate(projectilePrefab, new Vector2(spawnProjectileMessage.x, spawnProjectileMessage.y), Quaternion.Euler(0, 0, spawnProjectileMessage.rotation), projectileHolder);
                        obj.GetComponent<Damage>().teamId = spawnProjectileMessage.clientId;
                        obj.GetComponent<Damage>().projectileId = spawnProjectileMessage.projectileId;
                        obj.GetComponent<Damage>().isMultiplayerServer = true;

                        fireCooldownMap[spawnProjectileMessage.clientId] = Time.timeSinceLevelLoad;
                    }
                    break;
                }
                case Spawnable.Rocket:
                {
                    if ((rocketCountMap[spawnProjectileMessage.clientId] > 0) && (Time.timeSinceLevelLoad - fireCooldownMap[spawnProjectileMessage.clientId]) > rocketFireRate)
                    {
                        namedPipeStreamWriter.WriteMessageType(MessageType.SpawnProjectile);
                        namedPipeStreamWriter.WriteSpawnProjectileMessage(spawnProjectileMessage);

                        GameObject gameObject = Instantiate(rocketPrefab, new Vector2(spawnProjectileMessage.x, spawnProjectileMessage.y),  Quaternion.Euler(0, 0, spawnProjectileMessage.rotation), projectileHolder);
                        gameObject.GetComponent<Damage>().teamId = spawnProjectileMessage.clientId;
                        gameObject.GetComponent<Damage>().projectileId = spawnProjectileMessage.projectileId;
                        gameObject.GetComponent<Damage>().isMultiplayerServer = true;

                        rocketCountMap[spawnProjectileMessage.clientId]--;
                        fireCooldownMap[spawnProjectileMessage.clientId] = Time.timeSinceLevelLoad;
                    }
                    break;
                }
                default:
                {
                    break;
                }
            }
        }

        for (int i = 0; i < disconnectQueue.Count; i++)
        {
            disconnectQueue.TryDequeue(out clientId);

            if (playerMap.TryRemove(clientId, out GameObject obj))
            {
                playerCount--;
                respawningMap.TryRemove(clientId, out _);
                playerRespawnPointMap.TryRemove(clientId, out _);
                playerNameMap.TryRemove(clientId, out _);

                Destroy(obj);
                namedPipeStreamWriter.WriteMessageType(MessageType.Disconnect);
                namedPipeStreamWriter.WriteUint64(clientId);
            }
        }
    }

    void Update()
    {
        ProcessMessages();

        if (shouldSendUpdate)
        {
            SendUpdates();
            shouldSendUpdate = false;
        }

        if (shouldExit)
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    void OnDestroy()
    {
        // Disconnecting still connected players
        foreach (var playerId in playerMap.Keys)
        {
            namedPipeStreamWriter.WriteMessageType(MessageType.Disconnect);
            namedPipeStreamWriter.WriteUint64(playerId);
        }

        namedPipeStreamWriter.WriteMessageType(MessageType.Exit);

        source.Cancel();
        sendSource.Cancel();
        source.Dispose();
        sendSource.Dispose();

        pipeClient.Close();
        pipeServer.Close();

        updateTimer.Dispose();
    }
}
