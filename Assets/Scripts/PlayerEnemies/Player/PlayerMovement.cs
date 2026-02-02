using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public PlayerMovementStats MovementStats;
    [SerializeField] private Collider2D _feetCollider;
    [SerializeField] private Collider2D _bodyCollider;

    private KnockbackReceiver knockbackReceiver;
    private Rigidbody2D _rb;

    // Spawn variable
    private Vector3 _playerSpawnPosition;
    private Quaternion _playerSpawnRotation;

    // Movement variables
    private Vector2 _moveVelocity;
    private bool _isFacingRight;
    public bool IsFacingRight => _isFacingRight;

    // Collision check variables
    private RaycastHit2D _groundHit;
    private RaycastHit2D _headHit;
    private bool _isGrounded;
    private bool _bumpedHead;

    // Jump Variables
    public float VerticalVelocity { get; private set; }
    private bool _isJumping;
    private bool _isFastFalling;
    private bool _isFalling;
    private float _fastFallTime;
    private float _fastFallReleaseSpeed;
    private int _numberOfJumpUsed;

    // Apex Variables
    private float _apexPoint;
    private float _timePastApexThreshold;
    private bool _isPastApexThreshold;

    // Jump Buffer Variables
    private float _jumpBufferTimer;
    private bool _jumpReleasedDuringBuffer;

    // Coyote Time Variables
    private float _coyoteTimer;

    // Runtime modifiers / stacks
    private int _invertHorizontalStack = 0;

    public float GravityMultiplier { get; private set; } = 1f;
    public float MaxFallSpeedMultiplier { get; private set; } = 1f;
    public float SpeedMultiplier { get; private set; } = 1f;
    public bool InvertHorizontalInput => (_invertHorizontalStack % 2) != 0;

    // Dash runtime
    private bool _isDashing;
    private float _dashTimeLeft;
    private float _dashCooldownLeft;
    private Vector2 _dashDir;
    private bool _dashPrevHeld;

    // Respawn Position
    [SerializeField] private Transform _respawnPos;

    private void Awake()
    {
        _isFacingRight = true;
        _rb = GetComponent<Rigidbody2D>();
        knockbackReceiver = GetComponent<KnockbackReceiver>();

        _playerSpawnPosition = _respawnPos.position;
        _playerSpawnRotation = transform.rotation;
    }

    private void Update()
    {
        CountTimer();
        JumpChecks();

        UpdateDashTimers();
        HandleDashInput();
    }

    private void FixedUpdate()
    {
        CollisionChecks();

        if (knockbackReceiver != null && knockbackReceiver.IsKnockedBack)
        {
            return; // El knockback controla la velocity
        }

        // Si estamos en Dash, controlamos la velocidad directamente
        if (_isDashing)
        {
            UpdateDash();
            return;
        }

        Jump();

        Vector2 moveInput = InputManager.Movement;
        if (InvertHorizontalInput) moveInput.x *= -1f;

        if (_isGrounded)
            Move(MovementStats.GroundAcceleration, MovementStats.GroundDeceleration, moveInput);
        else
            Move(MovementStats.AirAceleration, MovementStats.AirDeceleration, moveInput);
    }

    private void OnDrawGizmos()
    {
        if (MovementStats.ShowWalkJumpArc)
            DrawJumpArc(MovementStats.MaxWalkSpeed, Color.white);
    }

    #region Movement
    private void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
        if (moveInput != Vector2.zero)
        {
            TurnCheck(moveInput);

            // ✅ YA NO HAY RUN usando DashIsPressed
            float baseSpeed = MovementStats.MaxWalkSpeed;
            float effectiveSpeed = baseSpeed * SpeedMultiplier;

            Vector2 targetVelocity = new Vector2(moveInput.x, 0f) * effectiveSpeed;

            _moveVelocity = Vector2.Lerp(_moveVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            _rb.linearVelocity = new Vector2(_moveVelocity.x, _rb.linearVelocity.y);
        }
        else
        {
            _moveVelocity = Vector2.Lerp(_moveVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
            _rb.linearVelocity = new Vector2(_moveVelocity.x, _rb.linearVelocity.y);
        }
    }

    private void TurnCheck(Vector2 moveInput)
    {
        if (_isFacingRight && moveInput.x < 0) Turn(false);
        else if (!_isFacingRight && moveInput.x > 0) Turn(true);
    }

    private void Turn(bool turnRight)
    {
        if (turnRight)
        {
            _isFacingRight = true;
            transform.Rotate(0f, 180f, 0f);
        }
        else
        {
            _isFacingRight = false;
            transform.Rotate(0f, -180f, 0f);
        }
    }
    #endregion

    #region Dash
    private void UpdateDashTimers()
    {
        if (_dashCooldownLeft > 0f)
            _dashCooldownLeft -= Time.deltaTime;
    }

    private void HandleDashInput()
    {
        if (MovementStats == null || !MovementStats.DashEnabled) return;

        // Rising edge usando DashIsPressed (tap)
        bool dashHeld = InputManager.DashIsPressed;
        bool dashPressedThisFrame = dashHeld && !_dashPrevHeld;
        _dashPrevHeld = dashHeld;

        if (!dashPressedThisFrame) return;

        TryStartDash();
    }

    private void TryStartDash()
    {
        if (_isDashing) return;
        if (_dashCooldownLeft > 0f) return;
        if (!MovementStats.DashAllowInAir && !_isGrounded) return;

        Vector2 input = InputManager.Movement;
        if (InvertHorizontalInput) input.x *= -1f;

        // Dirección fija al inicio
        if (MovementStats.DashOnlyHorizontal)
        {
            if (Mathf.Abs(input.x) > 0.01f)
                _dashDir = input.x > 0 ? Vector2.right : Vector2.left;
            else
                _dashDir = _isFacingRight ? Vector2.right : Vector2.left;
        }
        else
        {
            // Dash libre (si lo necesitas en el futuro)
            if (input.sqrMagnitude > 0.01f)
                _dashDir = input.normalized;
            else
                _dashDir = _isFacingRight ? Vector2.right : Vector2.left;
        }

        _isDashing = true;
        _dashTimeLeft = MovementStats.DashDuration;
        _dashCooldownLeft = MovementStats.DashCooldown;

        if (MovementStats.DashCancelVerticalVelocity)
        {
            VerticalVelocity = 0f;
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0f);
        }
    }

    private void UpdateDash()
    {
        _dashTimeLeft -= Time.fixedDeltaTime;

        float dashSpeed = MovementStats.DashSpeed * SpeedMultiplier;

        // Mantengo la Y actual; si cancelaste al iniciar, será 0
        _rb.linearVelocity = new Vector2(_dashDir.x * dashSpeed, _rb.linearVelocity.y);

        if (_dashTimeLeft <= 0f)
        {
            _isDashing = false;
            // Opcional: al terminar, podrías suavizar la X si quieres “end lag”
            // _moveVelocity = new Vector2(_rb.linearVelocity.x, 0f);
        }
    }
    #endregion

    #region Jump
    private void JumpChecks()
    {
        // When we press the jump button.
        if (InputManager.JumpWasPressed)
        {
            _jumpBufferTimer = MovementStats.JumpBufferTime;
            _jumpReleasedDuringBuffer = false;
        }

        // When we release the jump button.
        if (InputManager.JumpWasReleased)
        {
            if (_jumpBufferTimer > 0f)
                _jumpReleasedDuringBuffer = true;

            if (_isJumping && VerticalVelocity > 0f)
            {
                if (_isPastApexThreshold)
                {
                    _isPastApexThreshold = false;
                    _isFastFalling = true;
                    _fastFallTime = MovementStats.TimeForUpwardsCancel;
                    VerticalVelocity = 0f;
                }
                else
                {
                    _isFastFalling = true;
                    _fastFallReleaseSpeed = VerticalVelocity;
                }
            }
        }

        // Initiate Jump with Jump buffering and coyote time.
        if (_jumpBufferTimer > 0f && !_isJumping && (_isGrounded || _coyoteTimer > 0))
        {
            InitiateJump(1);

            if (_jumpReleasedDuringBuffer)
            {
                _isFastFalling = true;
                _fastFallReleaseSpeed = VerticalVelocity;
            }
        }
        // Double Jump
        else if (_jumpBufferTimer > 0 && _isJumping && _numberOfJumpUsed < MovementStats.NumberOfJumpsAllowed)
        {
            _isFastFalling = false;
            InitiateJump(1);
        }
        // Air jump after Coyote Time lapsed
        else if (_jumpBufferTimer > 0 && _isFalling && _numberOfJumpUsed < MovementStats.NumberOfJumpsAllowed - 1)
        {
            InitiateJump(2);
            _isFastFalling = true;
        }

        // Landed
        if ((_isJumping || _isFalling) && _isGrounded && VerticalVelocity <= 0f)
        {
            _isJumping = false;
            _isFalling = false;
            _isFastFalling = false;
            _fastFallTime = 0f;
            _isPastApexThreshold = false;
            _numberOfJumpUsed = 0;

            VerticalVelocity = Physics2D.gravity.y;
        }
    }

    private void InitiateJump(int numberOfJumpsUsed)
    {
        if (!_isJumping)
            _isJumping = true;

        _jumpBufferTimer = 0f;
        _numberOfJumpUsed += numberOfJumpsUsed;
        VerticalVelocity = MovementStats.InitialJumpVelocity;
    }

    private void Jump()
    {
        float effectiveGravity = MovementStats.Gravity * GravityMultiplier;
        float effectiveMaxFallSpeed = MovementStats.MaxFallSpeed * MaxFallSpeedMultiplier;

        if (_isJumping)
        {
            if (_bumpedHead)
                _isFastFalling = true;

            if (VerticalVelocity >= 0f)
            {
                _apexPoint = Mathf.InverseLerp(MovementStats.InitialJumpVelocity, 0f, VerticalVelocity);

                if (_apexPoint > MovementStats.ApexThreshold)
                {
                    if (!_isPastApexThreshold)
                    {
                        _isPastApexThreshold = true;
                        _timePastApexThreshold = 0f;
                    }

                    if (_isPastApexThreshold)
                    {
                        _timePastApexThreshold += Time.fixedDeltaTime;
                        if (_timePastApexThreshold < MovementStats.ApexHangTime) VerticalVelocity = 0f;
                        else VerticalVelocity = -0.01f;
                    }
                }
                else
                {
                    VerticalVelocity += effectiveGravity * Time.fixedDeltaTime;
                    if (_isPastApexThreshold) _isPastApexThreshold = false;
                }
            }
            else if (!_isFastFalling)
            {
                VerticalVelocity += effectiveGravity * MovementStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if (VerticalVelocity < 0f)
            {
                if (!_isFalling) _isFalling = true;
            }
        }

        if (_isFastFalling)
        {
            if (_fastFallTime >= MovementStats.TimeForUpwardsCancel)
            {
                VerticalVelocity += effectiveGravity * MovementStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else
            {
                VerticalVelocity = Mathf.Lerp(_fastFallReleaseSpeed, 0f, (_fastFallTime / MovementStats.TimeForUpwardsCancel));
            }

            _fastFallTime += Time.fixedDeltaTime;
        }

        if (!_isGrounded && !_isJumping)
        {
            if (!_isFalling) _isFalling = true;
            VerticalVelocity += effectiveGravity * Time.fixedDeltaTime;
        }

        VerticalVelocity = Mathf.Clamp(VerticalVelocity, -effectiveMaxFallSpeed, 50f);
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, VerticalVelocity);
    }

    private void DrawJumpArc(float moveSpeed, Color gizmoColor)
    {
        float effectiveGravity = MovementStats.Gravity * GravityMultiplier;

        Vector2 startPosition = new Vector2(_feetCollider.bounds.center.x, _feetCollider.bounds.min.y);
        Vector2 previousPosition = startPosition;

        float speed = MovementStats.DrawRight ? moveSpeed : -moveSpeed;
        Vector2 velocity = new Vector2(speed, MovementStats.InitialJumpVelocity);

        Gizmos.color = gizmoColor;

        float timeStep = 2 * MovementStats.TimeTillJumpApex / MovementStats.ArcResolution;

        for (int i = 0; i < MovementStats.VisualizationSteps; i++)
        {
            float simulationTime = i * timeStep;
            Vector2 displacement;
            Vector2 drawPoint;

            if (simulationTime < MovementStats.TimeTillJumpApex)
            {
                displacement = velocity * simulationTime + 0.5f * new Vector2(0, effectiveGravity) * simulationTime * simulationTime;
            }
            else if (simulationTime < MovementStats.TimeTillJumpApex + MovementStats.ApexHangTime)
            {
                float apexTime = simulationTime - MovementStats.TimeTillJumpApex;
                displacement = velocity * MovementStats.TimeTillJumpApex + 0.5f * new Vector2(0, effectiveGravity) * MovementStats.TimeTillJumpApex * MovementStats.TimeTillJumpApex;
                displacement += new Vector2(speed, 0) * apexTime;
            }
            else
            {
                float descendTime = simulationTime - (MovementStats.TimeTillJumpApex + MovementStats.ApexHangTime);
                displacement = velocity * MovementStats.TimeTillJumpApex + 0.5f * new Vector2(0, effectiveGravity) * MovementStats.TimeTillJumpApex * MovementStats.TimeTillJumpApex;
                displacement += new Vector2(speed, 0) * MovementStats.ApexHangTime;
                displacement += new Vector2(speed, 0) * descendTime + 0.5f * new Vector2(0, effectiveGravity) * descendTime * descendTime;
            }

            drawPoint = startPosition + displacement;

            if (MovementStats.StopOnCollision)
            {
                RaycastHit2D hit = Physics2D.Raycast(previousPosition, drawPoint - previousPosition,
                    Vector2.Distance(previousPosition, drawPoint), MovementStats.GroundLayer);

                if (hit.collider != null)
                {
                    Gizmos.DrawLine(previousPosition, hit.point);
                    break;
                }
            }

            Gizmos.DrawLine(previousPosition, drawPoint);
            previousPosition = drawPoint;
        }
    }
    #endregion

    #region Spawn
    public void Respawn()
    {
        _rb.linearVelocity = Vector2.zero;
        VerticalVelocity = 0f;

        _isJumping = false;
        _isFacingRight = true;
        _isFalling = false;
        _isFastFalling = false;
        _fastFallTime = 0f;
        _isPastApexThreshold = false;
        _numberOfJumpUsed = 0;
        _coyoteTimer = 0f;
        _jumpBufferTimer = 0f;

        // Dash reset (importante al reiniciar nivel)
        _isDashing = false;
        _dashTimeLeft = 0f;
        _dashCooldownLeft = 0f;
        _dashPrevHeld = false;

        transform.position = _playerSpawnPosition;
        transform.rotation = _playerSpawnRotation;
    }

    public void ResetInputModifiers()
    {
        _invertHorizontalStack = 0;
    }
    #endregion

    #region RunTimeModifiers
    public void MultiplyGravity(float multiplier) => GravityMultiplier *= multiplier;
    public void MultiplyMaxFallSpeed(float multiplier) => MaxFallSpeedMultiplier *= multiplier;
    public void MultiplySpeed(float multiplier) => SpeedMultiplier *= multiplier;

    public void PushInvertHorizontal() => _invertHorizontalStack++;
    public void PopInvertHorizontal()
    {
        _invertHorizontalStack--;
        if (_invertHorizontalStack < 0) _invertHorizontalStack = 0;
    }

    public void ResetMultipliers()
    {
        GravityMultiplier = 1f;
        MaxFallSpeedMultiplier = 1f;
        SpeedMultiplier = 1f;
        ResetInputModifiers();
        Debug.Log("PlayerMovement: Multiplicadores reseteados a valores por defecto");
    }
    #endregion

    #region Collision Checks
    private void IsGrounded()
    {
        Vector2 boxCastOrigin = new Vector2(_feetCollider.bounds.center.x, _feetCollider.bounds.min.y);
        Vector2 boxCastSize = new Vector2(_feetCollider.bounds.size.x, MovementStats.GroundDetectionRayLenght);

        _groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down,
            MovementStats.GroundDetectionRayLenght, MovementStats.GroundLayer);

        _isGrounded = _groundHit.collider != null;

        if (MovementStats.DebugShowIsGrounderBox)
        {
            Color rayColor = _isGrounded ? Color.green : Color.red;
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y),
                Vector2.down * MovementStats.GroundDetectionRayLenght, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y),
                Vector2.down * MovementStats.GroundDetectionRayLenght, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y - MovementStats.GroundDetectionRayLenght),
                Vector2.right * boxCastSize.x, rayColor);
        }
    }

    private void BumpedHead()
    {
        Vector2 boxCastOrigin = new Vector2(_feetCollider.bounds.center.x, _bodyCollider.bounds.max.y);
        Vector2 boxCastSize = new Vector2(_feetCollider.bounds.size.x * MovementStats.HeadWidth, MovementStats.HeadDetectionRayLenght);

        _headHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up,
            MovementStats.HeadDetectionRayLenght, MovementStats.GroundLayer);

        _bumpedHead = _headHit.collider != null;

        if (MovementStats.DebugShowHeadBumpBox)
        {
            Color rayColor = _bumpedHead ? Color.green : Color.red;
            Debug.DrawRay(new Vector2(boxCastOrigin.x - (boxCastSize.x / 2), boxCastOrigin.y),
                Vector2.up * MovementStats.HeadDetectionRayLenght, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + (boxCastSize.x / 2), boxCastOrigin.y),
                Vector2.up * MovementStats.HeadDetectionRayLenght, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - (boxCastSize.x / 2), boxCastOrigin.y + MovementStats.HeadDetectionRayLenght),
                Vector2.right * boxCastSize.x * MovementStats.HeadWidth, rayColor);
        }
    }

    private void CollisionChecks()
    {
        IsGrounded();
        BumpedHead();
    }
    #endregion

    #region Timers
    private void CountTimer()
    {
        _jumpBufferTimer -= Time.deltaTime;

        if (!_isGrounded) _coyoteTimer -= Time.deltaTime;
        else _coyoteTimer = MovementStats.JumpCoyoteTime;
    }
    #endregion
}
