using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    // A global reference for the input manager that outher scripts can access to read the input
    public static InputManager instance;

    private void Awake()
    {
        ResetValuesToDefault();
        // Set up the instance of this
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void ResetValuesToDefault()
    {
        horizontalMoveAxis = default;
        verticalMoveAxis = default;

        horizontalLookAxis = default;
        verticalLookAxis = default;

        firePressed = default;
        fireHeld = default;
        fireRocketPressed = default;

        pausePressed = default;
    }

    public float horizontalMoveAxis;
    public float verticalMoveAxis;
    public void ReadMovementInput(InputAction.CallbackContext context)
    {
        Vector2 inputVector = context.ReadValue<Vector2>();
        horizontalMoveAxis = inputVector.x;
        verticalMoveAxis = inputVector.y;
    }

    public float horizontalLookAxis;
    public float verticalLookAxis;

    public void ReadMousePositionInput(InputAction.CallbackContext context)
    {
        Vector2 inputVector = context.ReadValue<Vector2>();
        if (Mathf.Abs(inputVector.x) > 1 && Mathf.Abs(inputVector.y) > 1)
        {
            horizontalLookAxis = inputVector.x;
            verticalLookAxis = inputVector.y;
        }
    }

    public bool firePressed;
    public bool fireHeld;
    public bool fireRocketPressed;

    public void ReadFireInput(InputAction.CallbackContext context)
    {
        firePressed = !context.canceled;
        fireHeld = !context.canceled;
        StartCoroutine("ResetFireStart");
    }

    public void ReadFireRocketInput(InputAction.CallbackContext context)
    {
        fireRocketPressed = !context.canceled;
        StartCoroutine("ResetFireRocketStart");
    }

    private IEnumerator ResetFireStart()
    {
        yield return new WaitForEndOfFrame();
        firePressed = false;
    }

    private IEnumerator ResetFireRocketStart()
    {
        yield return new WaitForEndOfFrame();
        fireRocketPressed = false;
    }

    public bool pausePressed;
    public void ReadPauseInput(InputAction.CallbackContext context)
    {
        pausePressed = !context.canceled;
        StartCoroutine(ResetPausePressed());
    }

    IEnumerator ResetPausePressed()
    {
        yield return new WaitForEndOfFrame();
        pausePressed = false;
    }
}
