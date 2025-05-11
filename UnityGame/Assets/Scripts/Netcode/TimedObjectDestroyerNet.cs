using Unity.Netcode;
using UnityEngine;

public class TimedObjectDestroyerNet : NetworkBehaviour
{
    public float lifetime = 5.0f;

    private float timeAlive = 0.0f;

    void Update()
    {
        if (IsServer && (timeAlive > lifetime))
        {
            GetComponent<NetworkObject>().Despawn();
        }
        else
        {
            timeAlive += Time.deltaTime;
        }
    }
}
