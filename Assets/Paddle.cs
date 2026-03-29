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

    private Vector2 _inputDir;       // Current keyboard direction from Input Actions
    private bool _isBallAttached = true;

    // Stores the original Z scale so SetWidth can always scale relative to it
    private float _baseScaleY;

    private Playerinput _inputActions;

    void Awake()
    {
        _inputActions = new Playerinput();
        _baseScaleY   = transform.localScale.y;
    }

    void OnEnable()
    {
        _inputActions.Keyboard.Enable();
        // When a key is held: store direction
        _inputActions.Keyboard.KeyboardInput.performed += ctx => _inputDir = ctx.ReadValue<Vector2>();
        // When key is released: reset to zero
        _inputActions.Keyboard.KeyboardInput.canceled  += ctx => _inputDir = Vector2.zero;
    }

    void OnDisable()
    {
        _inputActions.Keyboard.Disable();
    }

    void Update()
    {
        MoveWithKeyboard();
        MoveWithMouse();
        ClampPosition();

        if (_isBallAttached)
        {
            if (ball == null) return;
            ball.SnapToPaddle(transform.position);

            if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame) | (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame))
            {
                _isBallAttached = false;
                ball.Launch();
            }
        }
    }

    private void MoveWithKeyboard()
    {
        if (Time.timeScale == 0f) return;
        transform.position += new Vector3(_inputDir.x * keyboardSpeed, 0f, 0f);
    }

    private void MoveWithMouse()
    {
        if (Mouse.current == null) return;
        if (Time.timeScale == 0f) return;

        float mouseDelta = Mouse.current.delta.x.ReadValue();
        transform.position += new Vector3(mouseDelta * mouseSensitivity, 0f, 0f);
    }

    // Clamps the paddle so it never goes past the side walls
    private void ClampPosition()
    {
        float halfWidth = transform.localScale.y * 0.5f;
        float clampedX = Mathf.Clamp(transform.position.x, minX + halfWidth, maxX - halfWidth);
        transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);
    }

    public void AttachBall(Ball ballToAttach)
    {
        ball = ballToAttach;
        _isBallAttached = true;
    }

    // Called by the WidePaddle power-up.
    // widthMultiplier = 2 → double width, 1 → back to normal
    public void SetWidth(float widthMultiplier)
    {
        Vector3 s = transform.localScale;
        transform.localScale = new Vector3(s.x, _baseScaleY * widthMultiplier, s.z);
    }
}