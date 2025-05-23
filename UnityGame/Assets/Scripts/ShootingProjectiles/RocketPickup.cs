using UnityEngine;

public class RocketPickup : MonoBehaviour
{
    public GameObject pickUpEvent;

    private void OnTriggerEnter2D(Collider2D other)
    {
        ShootingController controller = other.gameObject.GetComponent<ShootingController>();

        if (controller != null)
        {
            if (controller.isPlayerControlled)
            {
                controller.rocketCount++;
                Destroy(gameObject);
                if (pickUpEvent != null)
                {
                    Instantiate(pickUpEvent);
                }
            }
        }
    }
}
