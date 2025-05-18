using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float projectileSpeed = 3.0f;

    private void Start()
    {
        GetComponent<Rigidbody2D>().AddForce(transform.up * projectileSpeed);
    }
}