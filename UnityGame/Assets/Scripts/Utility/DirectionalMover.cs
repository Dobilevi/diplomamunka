using UnityEngine;

public class DirectionalMover : MonoBehaviour
{
    public Vector3 direction = Vector3.down;
    public float speed = 5.0f;

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        transform.position = transform.position + direction.normalized * speed * Time.deltaTime;
    }
}
