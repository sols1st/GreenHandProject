using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController1 : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public bool isFacingRight = true;
    public PolygonCollider2D boundaryCollider; // ���� BackgroundBoundsSetter �ϵ� PolygonCollider2D

    private InputSystem inputActions;
    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("PlayerController: Missing Rigidbody2D!");
        }

        if (boundaryCollider == null)
        {
            Debug.LogError("PlayerController: boundaryCollider is not assigned!");
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
        Vector2 desiredVelocity = rb.velocity;
        float moveX = moveInput.x;

        // === �ؼ�������Ƿ���������/���ƶ� ===
        if (moveX > 0 && !CanMoveRight())
        {
            moveX = 0;
        }
        else if (moveX < 0 && !CanMoveLeft())
        {
            moveX = 0;
        }

        desiredVelocity.x = moveX * moveSpeed;

        rb.velocity = desiredVelocity;

        // ��ת�߼�
        if (moveX > 0 && !isFacingRight)
            Flip();
        else if (moveX < 0 && isFacingRight)
            Flip();
    }

    bool CanMoveRight()
    {
        // Ԥ����һ��λ�ã�����һ��㣩
        Vector2 futurePos = (Vector2)transform.position + Vector2.right * 0.1f;
        return IsPointInsideBoundary(futurePos);
    }

    bool CanMoveLeft()
    {
        Vector2 futurePos = (Vector2)transform.position + Vector2.left * 0.1f;
        return IsPointInsideBoundary(futurePos);
    }

    bool IsPointInsideBoundary(Vector2 point)
    {
        if (boundaryCollider == null) return true;
        return boundaryCollider.OverlapPoint(point);
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }
}