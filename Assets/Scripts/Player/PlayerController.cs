using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public bool isFacingRight = true;

    private InputSystem inputActions;    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("PlayerController: Missing Rigidbody2D!");
        }
    }

    void OnEnable()
    {
        if (inputActions == null)
        {
            inputActions = new InputSystem();
            inputActions.Game.Move.performed += OnMove;
            inputActions.Game.Move.canceled += OnMove;
        }
        inputActions.Enable();
    }

    void OnDisable()
    {
        if (inputActions != null)
        {
            inputActions.Game.Move.performed -= OnMove;
            inputActions.Game.Move.canceled -= OnMove;
            inputActions.Disable();
        }
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        float moveX = moveInput.x;
        rb.velocity = new Vector2(moveX * moveSpeed, rb.velocity.y);

        if (moveX > 0 && !isFacingRight)
            Flip();
        else if (moveX < 0 && isFacingRight)
            Flip();
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }
}