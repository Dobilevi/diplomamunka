using Unity.Netcode;
using UnityEngine;

public class OutOfBoundsNet : NetworkBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsServer)
        {
            if (other.CompareTag("Player"))
            {
                other.GetComponent<HealthNet>().RespawnClientRpc();
            }
        }
    }
}
