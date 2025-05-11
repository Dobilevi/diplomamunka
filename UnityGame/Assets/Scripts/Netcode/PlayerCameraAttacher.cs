using Unity.Netcode;
using UnityEngine;

public class PlayerCameraAttacher : MonoBehaviour
{
    void Start()
    {
        if (GetComponent<NetworkObject>().IsOwner)
        {
            GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
            camera.GetComponent<CameraController>().target = gameObject.transform;
        }
    }
}
