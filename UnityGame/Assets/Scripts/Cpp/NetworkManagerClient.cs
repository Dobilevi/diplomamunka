using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;

using Cpp;
using Cpp.Messages;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkManagerClient : MonoBehaviour
{
    public Transform projectileHolder;

    private GameObject player = null;
    private UpdateMessage updateMessage = new UpdateMessage();
    private SpawnProjectileMessage spawnProjectileMessage = new SpawnProjectileMessage();

    private IPEndPoint serverIpEndPoint = null;
    HostBufferWriter bufferWriter = new HostBufferWriter(UInt16.MaxValue);
    NetworkBufferReader bufferReader = new NetworkBufferReader();

    private Task networkTask = null;
    CancellationTokenSource source = new CancellationTokenSource();

    private ulong outPackageId = 0;
    private ulong NextOutPackageId => outPackageId++;

    private ulong packageId = 0;
    private ConcurrentQueue<SpawnMessage> connectQueue = new ConcurrentQueue<SpawnMessage>();
    private ConcurrentQueue<string> connectNameQueue = new ConcurrentQueue<string>();
    private ConcurrentQueue<ulong> disconnectQueue = new ConcurrentQueue<ulong>();
    private ConcurrentQueue<SpawnMessage> spawnEnemyQueue = new ConcurrentQueue<SpawnMessage>();
    private ConcurrentQueue<SpawnMessage> spawnPlayerQueue = new ConcurrentQueue<SpawnMessage>();
    private ConcurrentQueue<string> spawnPlayerNameQueue = new ConcurrentQueue<string>();
    private ConcurrentQueue<SpawnProjectileMessage> spawnProjectileQueue = new ConcurrentQueue<SpawnProjectileMessage>();
    private ConcurrentQueue<DespawnMessage> despawnQueue = new ConcurrentQueue<DespawnMessage>();
    private ConcurrentQueue<UpdateMessage> updateQueue = new ConcurrentQueue<UpdateMessage>();

    private UdpClient udpClient = null;

    Timer updateTimer = null;

    private bool joined = false;
    private bool shouldSendUpdate = false;
    private bool shouldExit = false;

    private float lastReceivedMessageTimestamp = Single.PositiveInfinity;
    public const float timeOut = 5.0f;

    public string playerName;
    private UInt64 clientId = 0;

    public GameObject playerPrefab = null;
    public GameObject botPrefab = null;
    private Dictionary<ulong, GameObject> objectMap = new Dictionary<ulong, GameObject>();

    public GameObject projectilePrefab = null;
    public GameObject rocketPrefab = null;
    public GameObject enemyProjectilePrefab = null;
    private ConcurrentDictionary<ulong, GameObject> projectileMap = new ConcurrentDictionary<ulong, GameObject>();

    public GameObject playerProjectileHitEffect;
    public GameObject playerRocketHitEffect;
    public GameObject enemyProjectileHitEffect;

    public GameObject hitSoundEffect;

    private float start, end;
    private List<float> responseTimes = new List<float>();
    private ulong packagesLost = 0;
    private ulong packagesReceived = 0;
    
    public void Shoot(Spawnable spawnable, Vector3 position)
    {
        spawnProjectileMessage = new SpawnProjectileMessage
        {
            spawnable = spawnable,
            clientId = clientId,
            projectileId = 0,
            x = position.x,
            y = position.y,
            rotation = position.z
        };

        bufferWriter.Reset();
        bufferWriter.WriteUInt64(NextOutPackageId);
        bufferWriter.WriteUInt16((ushort)MessageType.SpawnProjectile);
        bufferWriter.WriteSpawnProjectileMessage(spawnProjectileMessage);

        start = Time.realtimeSinceStartup;
        udpClient.Send(bufferWriter.Buffer, bufferWriter.Size, serverIpEndPoint);
    }

    public Transform GetClosestEnemy(GameObject player)
    {
        if (objectMap.Count == 0)
        {
            return null;
        }

        GameObject closestEnemy = null;
        float closestDist = float.MaxValue;
        foreach (GameObject enemy in objectMap.Values)
        {
            float dist = (player.transform.position - enemy.transform.position).magnitude;
            if (dist < closestDist)
            {
                closestDist = dist;
                closestEnemy = enemy;
            }
        }

        return closestEnemy?.transform;
    }

    [DoesNotReturn]
    private void ReceiveAsync()
    {
        while (true)
        {
            Task<UdpReceiveResult> task = udpClient.ReceiveAsync();

            Task.WaitAll(task);

            if (!serverIpEndPoint.Equals(task.Result.RemoteEndPoint))
            {
                continue;
            }

            lastReceivedMessageTimestamp = Time.realtimeSinceStartup;

            bufferReader.Buffer = task.Result.Buffer;

            try
            {
                ulong newPackageId = bufferReader.ReadUint64();

                if (joined && (newPackageId <= packageId))
                {
                    Debug.Log("Received past package!");
                    continue;
                }
                else
                {
                    packagesReceived++;
                    packagesLost += newPackageId - packageId - 1;
                    packageId = newPackageId;
                }

                switch (bufferReader.ReadMessageType())
                {
                    case MessageType.Connect:
                    {
                        Debug.Log("Connect message");
                        Debug.Log($"SizeOf(SpawnMessage): {Marshal.SizeOf(typeof(SpawnMessage))}");
                        SpawnMessage spawnMessage = bufferReader.ReadSpawnMessage();
                        ushort length = bufferReader.ReadUint16();
                        Debug.Log($"length {length}");
                        string name = bufferReader.ReadString(length);

                        if (!joined)
                        {
                            clientId = spawnMessage.clientId;
                            updateMessage.clientId = clientId;
                            joined = true;
                        }

                        connectNameQueue.Enqueue(name);
                        connectQueue.Enqueue(spawnMessage);

                        break;
                    }
                    case MessageType.Disconnect:
                    {
                        Debug.Log("Disconnect message");
                        disconnectQueue.Enqueue(bufferReader.ReadUint64());
                        break;
                    }
                    case MessageType.SpawnProjectile:
                    {
                        spawnProjectileQueue.Enqueue(bufferReader.ReadSpawnProjectileMessage());
                        break;
                    }
                    case MessageType.Despawn:
                    {
                        despawnQueue.Enqueue(bufferReader.ReadDespawnMessage());
                        break;
                    }
                    case MessageType.Update:
                    {
                        updateQueue.Enqueue(bufferReader.ReadUpdateMessage());
                        break;
                    }
                    case MessageType.InitializeEnemy:
                    case MessageType.SpawnEnemy:
                    {
                        spawnEnemyQueue.Enqueue(bufferReader.ReadSpawnMessage());
                        break;
                    }
                    case MessageType.InitializePlayer:
                    case MessageType.SpawnPlayer:
                    {
                        SpawnMessage spawnMessage = bufferReader.ReadSpawnMessage();
                        ushort length = bufferReader.ReadUint16();

                        spawnPlayerNameQueue.Enqueue(bufferReader.ReadString(length));
                        spawnPlayerQueue.Enqueue(spawnMessage);
                        break;
                    }
                    case MessageType.InitializeProjectile:
                    {
                        spawnProjectileQueue.Enqueue(bufferReader.ReadSpawnProjectileMessage());
                        break;
                    }
                    case MessageType.ErrorServerFull:
                    {
                        if (!joined)
                        {
                            joined = true;
                            shouldExit = true;
                        }

                        break;
                    }
                    default:
                    {
                        break;
                    }
                }
            }
            catch (IndexOutOfRangeException exception)
            {
                continue;
            }
        }
    }

    private void ProcessMessages()
    {
        for (int i = 0; i < connectQueue.Count; i++)
        {
            connectQueue.TryDequeue(out SpawnMessage spawnMessage);
            connectNameQueue.TryDequeue(out string name);

            if (spawnMessage.clientId == clientId)
            {
                GameManager.errorMessage = GameManager.ErrorMessage.Unknown;
                SceneManager.LoadScene("MainMenu");
                return;
            }
            else if (objectMap.ContainsKey(spawnMessage.clientId))
            {
                objectMap.Remove(spawnMessage.clientId, out GameObject obj);
                Destroy(obj);
            }

            objectMap[spawnMessage.clientId] = Instantiate(playerPrefab, new Vector2(spawnMessage.x, spawnMessage.y), Quaternion.Euler(0, 0, spawnMessage.rotation));
            objectMap[spawnMessage.clientId].GetComponent<Health>().teamId = spawnMessage.clientId;
            objectMap[spawnMessage.clientId].GetComponentInChildren<Text>().text = name;
        }

        for (int i = 0; i < spawnEnemyQueue.Count; i++)
        {
            spawnEnemyQueue.TryDequeue(out SpawnMessage spawnMessage);

            if (spawnMessage.clientId == clientId)
            {
                GameManager.errorMessage = GameManager.ErrorMessage.Unknown;
                SceneManager.LoadScene("MainMenu");
                return;
            }
            else if (objectMap.ContainsKey(spawnMessage.clientId))
            {
                objectMap.Remove(spawnMessage.clientId, out GameObject obj);
                Destroy(obj);
            }

            objectMap[spawnMessage.clientId] = Instantiate(botPrefab, new Vector2(spawnMessage.x, spawnMessage.y), Quaternion.Euler(0, 0, spawnMessage.rotation));
        }

        for (int i = 0; i < spawnPlayerQueue.Count; i++)
        {
            spawnPlayerQueue.TryDequeue(out SpawnMessage spawnMessage);
            spawnPlayerNameQueue.TryDequeue(out string playerName);

            if (spawnMessage.clientId == clientId)
            {
                GameManager.errorMessage = GameManager.ErrorMessage.Unknown;
                SceneManager.LoadScene("MainMenu");
                return;
            }
            else if (objectMap.ContainsKey(spawnMessage.clientId))
            {
                objectMap.Remove(spawnMessage.clientId, out GameObject obj);
                Destroy(obj);
            }

            objectMap[spawnMessage.clientId] = Instantiate(playerPrefab, new Vector2(spawnMessage.x, spawnMessage.y), Quaternion.Euler(0, 0, spawnMessage.rotation));

            objectMap[spawnMessage.clientId].GetComponent<Health>().teamId = spawnMessage.clientId;
            objectMap[spawnMessage.clientId].GetComponentInChildren<Text>().text = playerName;
            
        }

        for (int i = 0; i < spawnProjectileQueue.Count; i++)
        {
            spawnProjectileQueue.TryDequeue(out SpawnProjectileMessage spawnMessage);

            switch (spawnMessage.spawnable)
            {
                case Spawnable.Fire:
                case Spawnable.Rocket:
                {
                    // Create the projectile
                    projectileMap[spawnMessage.projectileId] = Instantiate(spawnMessage.spawnable == Spawnable.Fire ? projectilePrefab : rocketPrefab, new Vector2(spawnMessage.x, spawnMessage.y), Quaternion.Euler(0, 0, spawnMessage.rotation), projectileHolder);
                    projectileMap[spawnMessage.projectileId].GetComponent<Damage>().teamId = spawnMessage.clientId;

                    if (clientId != spawnMessage.clientId)
                    {
                        switch (spawnMessage.spawnable)
                        {
                            case Spawnable.Fire:
                            {
                                Instantiate(playerPrefab.GetComponent<ShootingController>().fireEffect);
                                break;
                            }
                            case Spawnable.Rocket:
                            {
                                Instantiate(playerPrefab.GetComponent<ShootingController>().fireRocketEffect);
                                break;
                            }
                            default:
                            {
                                break;
                            }
                        }
                    }

                    if (clientId == spawnMessage.clientId)
                    {
                        end = Time.realtimeSinceStartup;
                        responseTimes.Add((end - start) * 1000);

                        player.GetComponent<ShootingController>().LastFired = Time.timeSinceLevelLoad;
                        if (spawnProjectileMessage.spawnable == Spawnable.Rocket)
                        {
                            player.GetComponent<ShootingController>().RocketCount--;
                        }
                    }
                    break;
                }
                case Spawnable.EnemyFire:
                {
                    // Create the projectile
                    projectileMap[spawnMessage.projectileId] = Instantiate(enemyProjectilePrefab, new Vector2(spawnMessage.x, spawnMessage.y), Quaternion.Euler(0, 0, spawnMessage.rotation), projectileHolder);
                    Instantiate(botPrefab.GetComponentInChildren<ShootingController>().fireEffect);
                    break;
                }
                default:
                {
                    break;
                }
            }
        }

        for (int i = 0; i < updateQueue.Count; i++)
        {
            updateQueue.TryDequeue(out UpdateMessage updateMessage);

            if ((updateMessage.clientId != clientId) && !objectMap.ContainsKey(updateMessage.clientId))
            {
                bufferWriter.Reset();
                bufferWriter.WriteUInt64(NextOutPackageId);
                bufferWriter.WriteUInt16((ushort)MessageType.ErrorObjectDoesNotExist);
                bufferWriter.WriteUInt64(updateMessage.clientId);

                udpClient.Send(bufferWriter.Buffer, bufferWriter.Size, serverIpEndPoint);
                continue;
            }

            switch (updateMessage.updateType)
            {
                case UpdateType.MOVE:
                {
                    if (clientId != updateMessage.clientId)
                    {
                        objectMap[updateMessage.clientId].GetComponent<MultiMovement>().MoveToPoint(new Vector2(updateMessage.x, updateMessage.y), updateMessage.rotation);
                    }
                    break;
                }
                case UpdateType.TELEPORT:
                {
                    if (clientId == updateMessage.clientId)
                    {
                        player.transform.position = new Vector2(updateMessage.x, updateMessage.y);
                        player.GetComponent<Rigidbody2D>().SetRotation(updateMessage.rotation);

                        // Send an ack message
                        bufferWriter.Reset();
                        bufferWriter.WriteUInt64(NextOutPackageId);
                        bufferWriter.WriteUInt16((ushort)MessageType.RespawnAck);
                        bufferWriter.WriteUInt64(clientId);

                        udpClient.Send(bufferWriter.Buffer, bufferWriter.Size, serverIpEndPoint);
                    }
                    else
                    {
                        objectMap[updateMessage.clientId].GetComponent<MultiMovement>().Stop();
                        objectMap[updateMessage.clientId].transform.position = new Vector2(updateMessage.x, updateMessage.y);
                        objectMap[updateMessage.clientId].transform.GetComponent<Rigidbody2D>().MoveRotation(updateMessage.rotation);
                    }
                    break;
                }
                case UpdateType.RESPAWN:
                {
                    if (clientId == updateMessage.clientId)
                    {
                        Instantiate(player.GetComponent<Health>().deathEffect, player.transform.position, player.transform.rotation, projectileHolder);

                        player.transform.position = new Vector2(updateMessage.x, updateMessage.y);
                        player.GetComponent<Rigidbody2D>().MoveRotation(updateMessage.rotation);

                        // Send an ack message
                        bufferWriter.Reset();
                        bufferWriter.WriteUInt64(NextOutPackageId);
                        bufferWriter.WriteUInt16((ushort)MessageType.RespawnAck);
                        bufferWriter.WriteUInt64(clientId);

                        udpClient.Send(bufferWriter.Buffer, bufferWriter.Size, serverIpEndPoint);
                    }
                    else
                    {
                        Instantiate(objectMap[updateMessage.clientId].GetComponent<Health>().deathEffect, objectMap[updateMessage.clientId].transform.position, objectMap[updateMessage.clientId].transform.rotation, projectileHolder);

                        objectMap[updateMessage.clientId].GetComponent<MultiMovement>().Stop();
                        objectMap[updateMessage.clientId].transform.position = new Vector2(updateMessage.x, updateMessage.y);
                        objectMap[updateMessage.clientId].transform.GetComponent<Rigidbody2D>().MoveRotation(updateMessage.rotation);
                    }
                    break;
                }
                default:
                {
                    break;
                }
            }
        }

        for (int i = 0; i < despawnQueue.Count; i++)
        {
            despawnQueue.TryDequeue(out DespawnMessage despawnMessage);
            switch (despawnMessage.spawnable)
            {
                case Spawnable.Fire:
                case Spawnable.Rocket:
                case Spawnable.EnemyFire:
                {
                    projectileMap.Remove(despawnMessage.id, out GameObject objectToDespawn);
                    switch (despawnMessage.spawnable)
                    {
                        case Spawnable.Fire:
                        {
                            Instantiate(playerProjectileHitEffect, objectToDespawn.transform.position, objectToDespawn.transform.rotation, projectileHolder);
                            break;
                        }
                        case Spawnable.Rocket:
                        {
                            Instantiate(playerRocketHitEffect, objectToDespawn.transform.position, objectToDespawn.transform.rotation, projectileHolder);
                            break;
                        }
                        case Spawnable.EnemyFire:
                        {
                            Instantiate(enemyProjectileHitEffect, objectToDespawn.transform.position, objectToDespawn.transform.rotation, projectileHolder);
                            break;
                        }
                    }
                    Instantiate(hitSoundEffect, objectToDespawn.transform.position, objectToDespawn.transform.rotation, projectileHolder);
                    Destroy(objectToDespawn);

                    break;
                }
                case Spawnable.Player:
                case Spawnable.Enemy:
                {
                    objectMap.Remove(despawnMessage.id, out GameObject objectToDespawn);
                    Instantiate(objectToDespawn.GetComponent<Health>().deathEffect, objectToDespawn.transform.position, objectToDespawn.transform.rotation, projectileHolder);
                    Destroy(objectToDespawn);

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
            disconnectQueue.TryDequeue(out ulong clientId);

            if (this.clientId == clientId)
            {
                GameManager.errorMessage = GameManager.ErrorMessage.Disconnected;
                SceneManager.LoadScene("MainMenu");
            }
            else
            {
                objectMap[clientId].SetActive(false);
                Destroy(objectMap[clientId]);
                objectMap.Remove(clientId);
            }
        }
    }

    private void ConnectToServer()
    {
        playerName = String.Join("", GameManager.instance.playerName.Split(' '));
        playerName = playerName.Substring(0, Math.Min(playerName.Length, NetConstants.maxPlayerNameLength));

        bufferWriter.Reset();
        bufferWriter.WriteUInt64(NextOutPackageId);
        bufferWriter.WriteUInt16((ushort)MessageType.Connect);
        bufferWriter.WriteUInt16((ushort)playerName.Length);
        bufferWriter.WriteString(playerName);

        udpClient.Send(bufferWriter.Buffer, bufferWriter.Size, serverIpEndPoint);
    }

    private void SendUpdate()
    {
        updateMessage.x = player.transform.position.x;
        updateMessage.y = player.transform.position.y;
        updateMessage.rotation = player.transform.rotation.eulerAngles.z;

        bufferWriter.Reset();
        bufferWriter.WriteUInt64(NextOutPackageId);
        bufferWriter.WriteUInt16((ushort)MessageType.Update);
        bufferWriter.WriteUpdateMessage(updateMessage);

        udpClient.Send(bufferWriter.Buffer, bufferWriter.Size, serverIpEndPoint);
    }

    private void ShouldSendUpdate(object obj)
    {
        shouldSendUpdate = true;
    }

    private void Start()
    {
        player = GameObject.FindWithTag("Player");
        player.GetComponent<Health>().teamId = clientId;

        serverIpEndPoint = new IPEndPoint(IPAddress.Parse(GameManager.instance.serverAddress), GameManager.instance.serverPort);

        while (true)
        {
            try
            {
                if (NetConstants.clientPort <= NetConstants.maxClientPort)
                {
                    udpClient = new UdpClient(NetConstants.clientPort);
                }
                else
                {
                    GameManager.errorMessage = GameManager.ErrorMessage.Port;
                    SceneManager.LoadScene("MainMenu");
                }
                break;
            }
            catch (SocketException _)
            {
                NetConstants.clientPort++;
            }
        }

        networkTask = Task.Factory.StartNew(ReceiveAsync, source.Token);

        ConnectToServer();

        ushort tries = 0;
        while (!joined)
        {
            if (tries++ > 10)
            {
                // Time out
                GameManager.errorMessage = GameManager.ErrorMessage.Timeout;
                SceneManager.LoadScene("MainMenu");
                return;
            }
            Thread.Sleep(250);
        }

        connectQueue.TryDequeue(out SpawnMessage spawnMessage);
        connectNameQueue.TryDequeue(out string name);

        player.GetComponent<Health>().teamId = clientId;
        player.GetComponentInChildren<Text>().text = name;
        player.transform.position = new Vector2(spawnMessage.x, spawnMessage.y);

        // Start sending updates
        updateTimer = new Timer(ShouldSendUpdate, null, new TimeSpan(0, 0, 0, 0, 0), new TimeSpan(0, 0, 0, 0, NetConstants.updateInterval));
    }

    private void Update()
    {
        if ((Time.realtimeSinceStartup - lastReceivedMessageTimestamp) > timeOut)
        {
            GameManager.errorMessage = GameManager.ErrorMessage.Timeout;
            SceneManager.LoadScene("MainMenu");
        }

        ProcessMessages();

        if (shouldSendUpdate)
        {
           SendUpdate();
           shouldSendUpdate = false;
        }

        if (shouldExit)
        {
            GameManager.errorMessage = GameManager.ErrorMessage.ServerFull;
            SceneManager.LoadScene("MainMenu");
        }
    }

    private void OnDestroy()
    {
        if (joined)
        {
            bufferWriter.Reset();
            bufferWriter.WriteUInt64(NextOutPackageId);
            bufferWriter.WriteUInt16((ushort)MessageType.Disconnect);
            bufferWriter.WriteUInt64(clientId);

            udpClient.Send(bufferWriter.Buffer, bufferWriter.Size, serverIpEndPoint);
        }

        source.Cancel();
        source.Dispose();

        udpClient.Close();
        udpClient.Dispose();

        updateTimer.Dispose();

        StreamWriter fs = new StreamWriter($"response_times_{DateTime.Now.ToString("yyyyMMddHHmmss")}_{Time.realtimeSinceStartup}.txt");
        fs.WriteLine($"packages received: {packagesReceived}");
        fs.WriteLine($"packages lost: {packagesLost}");
        fs.WriteLine($"response times: {responseTimes.Count}");
        fs.WriteLine("");
        foreach (var responseTime in responseTimes)
        {
            fs.WriteLine(responseTime);
        }

        fs.Close();
    }
}
