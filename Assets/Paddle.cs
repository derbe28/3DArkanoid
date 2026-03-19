using UnityEngine;
using UnityEngine.InputSystem;

public class Paddle : MonoBehaviour
{
    [Header("Movement")]
    public float keyboardSpeed = 0.1f;
    public float mouseSensitivity = 0.05f;

    [Header("Boundaries")]
    public float minX = -4f;    // Left wall limit
    public float maxX = 4f;     // Right wall limit

    [Header("Ball")]
    public Ball ball;
    
    private Vector2 inputDir;   // Current keyboard direction from Input Actions
    private bool isBallAttached = true;

    private Playerinput inputActions;

    void Awake()
    {
        inputActions = new Playerinput();
    }

    void OnEnable()
    {
        inputActions.Keyboard.Enable();
        // When a key is held: store direction
        inputActions.Keyboard.KeyboardInput.performed += ctx => inputDir = ctx.ReadValue<Vector2>();
        // When key is released: reset to zero
        inputActions.Keyboard.KeyboardInput.canceled  += ctx => inputDir = Vector2.zero;
    }

    void OnDisable()
    {
        inputActions.Keyboard.Disable();
    }

    void Update()
    {
        MoveWithKeyboard();
        MoveWithMouse();
        ClampPosition();

        if (isBallAttached)
        {
            ball.SnapToPaddle(transform.position);

            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                isBallAttached = false;
                ball.Launch();
            }
        }
    }

    private void MoveWithKeyboard()
    {
        transform.position += new Vector3(inputDir.x * keyboardSpeed, 0f, 0f);
    }

    private void MoveWithMouse()
    {
        if (Mouse.current == null) return;

        float mouseDelta = Mouse.current.delta.x.ReadValue();
        transform.position += new Vector3(mouseDelta * mouseSensitivity, 0f, 0f);
    }
    
    // Clamps the paddle so it never goes past the side walls.
    private void ClampPosition()
    {
        float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
        transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);
    }

    public void AttachBall(Ball ballToAttach)
    {
        ball = ballToAttach;
        isBallAttached = true;
    }
}
