using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    // The camera being controlled by this script
    [HideInInspector] private Camera playerCamera = null;

    public Transform target = null;

    public enum CameraStyles { Locked, Overhead, Free };

    public CameraStyles cameraMovementStyle = CameraStyles.Locked;

    [Range(0, 0.75f)] public float freeCameraMouseTracking = 0.5f;

    public float maxDistanceFromTarget = 5.0f;

    public float cameraZCoordinate = -10.0f;

    //The input manager that reads in input
    private InputManager inputManager;

    void Start()
    {
        playerCamera = GetComponent<Camera>();
        if (inputManager == null)
        {
            inputManager = InputManager.instance;
        }
        if (inputManager == null)
        {
            Debug.LogWarning("The Camera Controller can not find an Input Manager in the scene");
        }
    }

    void Update()
    {
        SetCameraPosition();
    }

    private void SetCameraPosition()
    {
        if (target != null)
        {
            Vector3 targetPosition = GetTargetPosition();
            Vector3 mousePosition = GetPlayerMousePosition();
            Vector3 desiredCameraPosition = ComputeCameraPosition(targetPosition, mousePosition);

            transform.position = desiredCameraPosition;
        }      
    }

    public Vector3 GetTargetPosition()
    {
        if (target != null)
        {
            return target.position;
        }
        return transform.position;
    }

    public Vector3 GetPlayerMousePosition()
    {
        if (cameraMovementStyle == CameraStyles.Locked)
        {
            return Vector3.zero;
        }
        return playerCamera.ScreenToWorldPoint(new Vector2(inputManager.horizontalLookAxis, inputManager.verticalLookAxis));
    }

    public Vector3 ComputeCameraPosition(Vector3 targetPosition, Vector3 mousePosition)
    {
        Vector3 result = Vector3.zero;
        switch (cameraMovementStyle)
        {
            case CameraStyles.Locked:
                result = transform.position;
                break;
            case CameraStyles.Overhead:
                result = targetPosition;
                break;
            case CameraStyles.Free:
                Vector3 desiredPosition = Vector3.Lerp(targetPosition, mousePosition, freeCameraMouseTracking);
                Vector3 difference = desiredPosition - targetPosition;
                difference = Vector3.ClampMagnitude(difference, maxDistanceFromTarget);
                result = targetPosition + difference;
                break;
        }
        result.z = cameraZCoordinate;
        return result;
    }
}
