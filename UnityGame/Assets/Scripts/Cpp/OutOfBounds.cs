using UnityEngine;

public class OutOfBounds : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            other.GetComponent<Health>().Respawn();
        }
    }
}
