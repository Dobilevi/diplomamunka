using Unity.Netcode;
using UnityEngine;

public class ProjectileNet : NetworkBehaviour
{
    public float projectileSpeed = 3.0f;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (IsOwner)
        {
            GetComponent<Rigidbody2D>().AddForce(transform.up * projectileSpeed);
        }
    }
}
