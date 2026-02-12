using UnityEngine;

public class PlayerControllerRPG : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float wallBounceForce = 7f;
    [SerializeField] private float wallBounceLockTime = 0.3f;

    [Header("Wall Sliding Settings")]
    [SerializeField] private float wallSlidingSpeed = 2f;
    [SerializeField] private float wallStickTime = 0.1f;

    [Header("Surface Detection")]
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private float wallCheckDistance = 0.1f;
    [SerializeField] private float maxGroundAngle = 60f;
    [SerializeField] private LayerMask tilemapLayer;
    [SerializeField] private LayerMask nonStickyWallLayer;

    [Header("References")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform leftWallCheck;
    [SerializeField] private Transform rightWallCheck;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;

    // Состояния персонажа
    private bool isGrounded;
    private bool isOnWall;
    private bool isOnNonStickyWall;
    private bool isTouchingLeftWall;
    private bool isTouchingRightWall;
    private bool isTouchingLeftNonStickyWall;
    private bool isTouchingRightNonStickyWall;
    private bool isWallSliding;
    private float moveInput;

    // Переменные для отскока
    private float wallBounceTimer = 0f;
    private bool isWallBouncing = false;
    private int bounceFromWall;

    // Переменные для соскальзывания
    private float wallStickTimer = 0f;
    private bool isPressingIntoWall = false;

    // Переменные для анимации прыжка
    private bool isJumping = false;

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        if (groundCheck == null || leftWallCheck == null || rightWallCheck == null)
        {
            Debug.LogError("Не все точки проверки назначены в инспекторе!");
        }
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        CheckWallPressing();

        if (isWallBouncing)
        {
            wallBounceTimer -= Time.deltaTime;
            if (wallBounceTimer <= 0f) isWallBouncing = false;
        }

        // Соскальзывание работает только для обычных стен
        if (isPressingIntoWall && isOnWall && !isGrounded)
        {
            wallStickTimer -= Time.deltaTime;
            if (wallStickTimer <= 0f) isWallSliding = true;
        }
        else
        {
            wallStickTimer = wallStickTime;
            isWallSliding = false;
        }

        // Обработка прыжка - отскок только от обычных стен
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                PerformJump();
                isJumping = true;
            }
            else if (isOnWall && !isWallBouncing)
            {
                PerformWallBounce();
                isJumping = true;
            }
        }

        CheckGround();
        CheckWalls();
        UpdateVisuals();
    }

    void FixedUpdate()
    {
        float finalMoveInput = GetFinalMoveInput();

        if (isWallBouncing)
        {
            ApplyBounceMovement(finalMoveInput);
        }
        else if (isWallSliding)
        {
            ApplyWallSlidingMovement(finalMoveInput);
        }
        else
        {
            rb.linearVelocity = new Vector2(finalMoveInput * moveSpeed, rb.linearVelocity.y);
        }

        // Обработка NonSticky стен - просто блокируем движение сквозь них
        HandleNonStickyWalls();
    }

    void HandleNonStickyWalls()
    {
        if (isTouchingLeftNonStickyWall && rb.linearVelocity.x < 0)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }

        if (isTouchingRightNonStickyWall && rb.linearVelocity.x > 0)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    void CheckWallPressing()
    {
        isPressingIntoWall = (isTouchingLeftWall && moveInput < 0) ||
                            (isTouchingRightWall && moveInput > 0);
    }

    float GetFinalMoveInput()
    {
        float input = moveInput;

        if (isWallBouncing)
        {
            if (bounceFromWall == 1 && input > 0) input = 0;
            else if (bounceFromWall == -1 && input < 0) input = 0;
        }

        return input;
    }

    void ApplyBounceMovement(float moveInput)
    {
        Vector2 currentVelocity = rb.linearVelocity;
        float targetXVelocity = moveInput * moveSpeed;
        float newXVelocity = Mathf.Lerp(currentVelocity.x, targetXVelocity, Time.fixedDeltaTime * 5f);
        rb.linearVelocity = new Vector2(newXVelocity, currentVelocity.y);
    }

    void ApplyWallSlidingMovement(float moveInput)
    {
        float targetXVelocity = moveInput * moveSpeed * 0.3f;
        float newXVelocity = Mathf.Lerp(rb.linearVelocity.x, targetXVelocity, Time.fixedDeltaTime * 5f);
        float newYVelocity = Mathf.Max(rb.linearVelocity.y, -wallSlidingSpeed);
        rb.linearVelocity = new Vector2(newXVelocity, newYVelocity);
    }

    void CheckGround()
    {
        RaycastHit2D groundHit = Physics2D.Raycast(
            groundCheck.position,
            Vector2.down,
            groundCheckDistance,
            tilemapLayer);

        bool groundDetected = false;

        if (groundHit.collider != null)
        {
            float surfaceAngle = Vector2.Angle(groundHit.normal, Vector2.up);
            groundDetected = surfaceAngle <= maxGroundAngle;
        }

        if (isGrounded != groundDetected)
        {
            isGrounded = groundDetected;
            if (isGrounded)
            {
                isWallBouncing = false;
                isWallSliding = false;
                isJumping = false; // Сбрасываем прыжок при приземлении
            }
        }
    }

    void CheckWalls()
    {
        bool leftWallDetected = false;
        bool rightWallDetected = false;

        RaycastHit2D leftHit = Physics2D.Raycast(
            leftWallCheck.position,
            Vector2.left,
            wallCheckDistance,
            tilemapLayer);

        if (leftHit.collider != null)
        {
            float leftAngle = Vector2.Angle(leftHit.normal, Vector2.right);
            leftWallDetected = leftAngle <= maxGroundAngle;
        }

        RaycastHit2D rightHit = Physics2D.Raycast(
            rightWallCheck.position,
            Vector2.right,
            wallCheckDistance,
            tilemapLayer);

        if (rightHit.collider != null)
        {
            float rightAngle = Vector2.Angle(rightHit.normal, Vector2.left);
            rightWallDetected = rightAngle <= maxGroundAngle;
        }

        if (isTouchingLeftWall != leftWallDetected)
            isTouchingLeftWall = leftWallDetected;

        if (isTouchingRightWall != rightWallDetected)
            isTouchingRightWall = rightWallDetected;

        isOnWall = isTouchingLeftWall || isTouchingRightWall;

        bool leftNonStickyDetected = false;
        bool rightNonStickyDetected = false;

        RaycastHit2D leftNonStickyHit = Physics2D.Raycast(
            leftWallCheck.position,
            Vector2.left,
            wallCheckDistance,
            nonStickyWallLayer);

        if (leftNonStickyHit.collider != null)
        {
            float leftAngle = Vector2.Angle(leftNonStickyHit.normal, Vector2.right);
            leftNonStickyDetected = leftAngle <= maxGroundAngle;
        }

        RaycastHit2D rightNonStickyHit = Physics2D.Raycast(
            rightWallCheck.position,
            Vector2.right,
            wallCheckDistance,
            nonStickyWallLayer);

        if (rightNonStickyHit.collider != null)
        {
            float rightAngle = Vector2.Angle(rightNonStickyHit.normal, Vector2.left);
            rightNonStickyDetected = rightAngle <= maxGroundAngle;
        }

        if (isTouchingLeftNonStickyWall != leftNonStickyDetected)
            isTouchingLeftNonStickyWall = leftNonStickyDetected;

        if (isTouchingRightNonStickyWall != rightNonStickyDetected)
            isTouchingRightNonStickyWall = rightNonStickyDetected;

        isOnNonStickyWall = isTouchingLeftNonStickyWall || isTouchingRightNonStickyWall;

        if (!isOnWall)
        {
            isWallSliding = false;
        }
    }

    void PerformJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    void PerformWallBounce()
    {
        Vector2 bounceDirection = isTouchingRightWall ? Vector2.left : Vector2.right;
        bounceFromWall = isTouchingRightWall ? 1 : -1;

        Vector2 bounceVelocity = bounceDirection * wallBounceForce + Vector2.up * jumpForce;
        rb.linearVelocity = bounceVelocity;

        spriteRenderer.flipX = isTouchingRightWall;

        isWallBouncing = true;
        wallBounceTimer = wallBounceLockTime;
        isWallSliding = false;
    }

    void UpdateVisuals()
    {
        if (!isWallBouncing && Mathf.Abs(moveInput) > 0.1f)
        {
            spriteRenderer.flipX = moveInput < 0;
        }

        if (TryGetComponent<Animator>(out var animator))
        {
            // Приоритеты анимаций:
            // 1. Отскок от стены
            // 2. Соскальзывание со стены
            // 3. Прыжок
            // 4. Бег/покой

            animator.SetBool("IsRunning", Mathf.Abs(moveInput) > 0.1f && !isJumping);
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetBool("IsOnWall", isOnWall);
            animator.SetBool("IsWallBouncing", isWallBouncing);
            animator.SetBool("IsWallSliding", isWallSliding);
            animator.SetBool("IsJumping", isJumping);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawRay(groundCheck.position, Vector2.down * groundCheckDistance);

            Gizmos.color = Color.yellow;
            Vector2 leftBound = Quaternion.Euler(0, 0, maxGroundAngle) * Vector2.down;
            Vector2 rightBound = Quaternion.Euler(0, 0, -maxGroundAngle) * Vector2.down;
            Gizmos.DrawRay(groundCheck.position, leftBound * groundCheckDistance);
            Gizmos.DrawRay(groundCheck.position, rightBound * groundCheckDistance);
        }

        if (leftWallCheck != null && rightWallCheck != null)
        {
            // Обычные стены (синий)
            Gizmos.color = isTouchingLeftWall ? Color.blue : Color.gray;
            Gizmos.DrawRay(leftWallCheck.position, Vector2.left * wallCheckDistance);

            Gizmos.color = isTouchingRightWall ? Color.blue : Color.gray;
            Gizmos.DrawRay(rightWallCheck.position, Vector2.right * wallCheckDistance);

            // Стены без прилипания и отскока (красный)
            Gizmos.color = isTouchingLeftNonStickyWall ? Color.red : Color.gray;
            Gizmos.DrawRay(leftWallCheck.position + Vector3.up * 0.1f, Vector2.left * wallCheckDistance);

            Gizmos.color = isTouchingRightNonStickyWall ? Color.red : Color.gray;
            Gizmos.DrawRay(rightWallCheck.position + Vector3.up * 0.1f, Vector2.right * wallCheckDistance);

            if (isWallBouncing)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(transform.position, 0.5f);
            }

            if (isWallSliding)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, 0.6f);
            }

            if (isJumping)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, 0.7f);
            }
        }
    }

    public bool IsGrounded() => isGrounded;
    public bool IsOnWall() => isOnWall;
    public bool IsOnNonStickyWall() => isOnNonStickyWall;
    public bool IsMoving() => Mathf.Abs(moveInput) > 0.1f;
    public bool IsWallBouncing() => isWallBouncing;
    public bool IsWallSliding() => isWallSliding;
    public bool IsJumping() => isJumping;

    public void SetMoveInput(float input) => moveInput = input;
    public void Jump()
    {
        if (isGrounded)
        {
            PerformJump();
            isJumping = true;
        }
        else if (isOnWall && !isWallBouncing)
        {
            PerformWallBounce();
            isJumping = true;
        }
    }
}