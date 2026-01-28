using UnityEngine;
// using static GameManager;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public PlayerMovementStats MovementStats;
    [SerializeField] private Collider2D _feetCollider;
    [SerializeField] private Collider2D _bodyCollider;

    private Rigidbody2D _rb;

    // Spawn variable
    private Vector3 _playerSpawnPosition;
    private Quaternion _playerSpawnRotation;

    //Movement variables
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

    // Runtime Multipiers
    private int _invertHorizontalStack = 0;

    public float GravityMultiplier { get; private set; } = 1f;
    public float MaxFallSpeedMultiplier { get; private set; } = 1f;
    public float SpeedMultiplier { get; private set; } = 1;
    public bool InvertHorizontalInput => (_invertHorizontalStack % 2) != 0;


    // Respawn Position
    [SerializeField] private Transform _respawnPos;

    private void Awake()
    {
        _isFacingRight = true;

        _rb = GetComponent<Rigidbody2D>();

        _playerSpawnPosition = _respawnPos.position;
        _playerSpawnRotation = transform.rotation;
    }

    private void Update()
    {
        CountTimer();
        JumpChecks();
    }

    private void FixedUpdate()
    {
        CollisionChecks();
        Jump();

        Vector2 moveInput = InputManager.Movement;

        if (InvertHorizontalInput)
            moveInput.x *= -1f;

        if (_isGrounded)
            Move(MovementStats.GroundAcceleration, MovementStats.GroundDeceleration, moveInput);
        else
            Move(MovementStats.AirAceleration, MovementStats.AirDeceleration, moveInput);
    }

    private void OnDrawGizmos()
    {
        if (MovementStats.ShowWalkJumpArc)
        {
            DrawJumpArc(MovementStats.MaxWalkSpeed, Color.white);
        }
        if (MovementStats.ShowRunJumpArc)
        {
            DrawJumpArc(MovementStats.MaxRunSpeed, Color.red);
        }
    }


    #region Movement
    private void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
        if (moveInput != Vector2.zero)
        {
            TurnCheck(moveInput);

            float baseSpeed = InputManager.DashIsPressed ? MovementStats.MaxRunSpeed : MovementStats.MaxWalkSpeed;
            float effectiveSpeed = baseSpeed * SpeedMultiplier;

            Vector2 targetVelocity = Vector2.zero;
            
            targetVelocity = new Vector2(moveInput.x, 0f) * effectiveSpeed;

            _moveVelocity = Vector2.Lerp(_moveVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            _rb.linearVelocity = new Vector2(_moveVelocity.x, _rb.linearVelocity.y);
        } else if (moveInput == Vector2.zero)
        {
            _moveVelocity = Vector2.Lerp(_moveVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
            _rb.linearVelocity = new Vector2(_moveVelocity.x, _rb.linearVelocity.y);
        }
    }

    private void TurnCheck (Vector2 moveInput)
    {
        if (_isFacingRight && moveInput.x < 0)
        {
            Turn(false);
        } else if (!_isFacingRight && moveInput.x > 0)
        {
            Turn(true);
        }
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
            transform.Rotate(0f,-180f,0f);
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
            {
                _jumpReleasedDuringBuffer = true;
            }

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

        // Air jump after Coyote Time laspsed
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
        {
            _isJumping = true;
        }

        _jumpBufferTimer = 0f;
        _numberOfJumpUsed += numberOfJumpsUsed;
        VerticalVelocity = MovementStats.InitialJumpVelocity;
    }

    private void Jump()
    {
        float effectiveGravity = MovementStats.Gravity * GravityMultiplier;
        float effectiveMaxFallSpeed = MovementStats.MaxFallSpeed * MaxFallSpeedMultiplier;

        // Apply Gravity while jumping
        if (_isJumping)
        {
            // Check for head Bump
            if (_bumpedHead)
            {
                _isFastFalling = true;
            }

            // Gravity on Ascending
            if (VerticalVelocity >= 0f)
            {
                // Apex Controls
                _apexPoint = Mathf.InverseLerp(MovementStats.InitialJumpVelocity, 0f, VerticalVelocity);

                if (_apexPoint > MovementStats.ApexThreshold)
                {
                    if (!_isPastApexThreshold)
                    {
                        _isPastApexThreshold = true;
                        _timePastApexThreshold = 0f;
                    }

                    if(_isPastApexThreshold)
                    {
                        _timePastApexThreshold += Time.fixedDeltaTime;
                        if(_timePastApexThreshold < MovementStats.ApexHangTime)
                        {
                            VerticalVelocity = 0f;
                        }
                        else
                        {
                            VerticalVelocity = -0.01f;
                        }
                    }
                }

                // Gravity on Ascending but not past Apex Threshold
                else
                {
                    VerticalVelocity += effectiveGravity * Time.fixedDeltaTime;
                    if (_isPastApexThreshold)
                    {
                        _isPastApexThreshold = false;
                    }
                }
            }

            // Gravity on Descending
            else if (!_isFastFalling)
            {
                VerticalVelocity += effectiveGravity * MovementStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }

            else if(VerticalVelocity < 0f)
            {
                if (!_isFalling)
                {
                    _isFalling = true;
                }
            }
        }

        // Jump cut
        if (_isFastFalling)
        {
            if(_fastFallTime >= MovementStats.TimeForUpwardsCancel)
            {
                VerticalVelocity += effectiveGravity * MovementStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            } 
            else if(_fastFallTime < MovementStats.TimeForUpwardsCancel)
            {
                VerticalVelocity = Mathf.Lerp(_fastFallReleaseSpeed, 0f, (_fastFallTime / MovementStats.TimeForUpwardsCancel));
            }

            _fastFallTime += Time.fixedDeltaTime;
        }

        // Normal Gravity while falling
        if (!_isGrounded && !_isJumping)
        {
            if (!_isFalling)
            {
                _isFalling = true;
            }

            VerticalVelocity += effectiveGravity * Time.fixedDeltaTime;
        }

        // Clamp Fall Speed
        VerticalVelocity = Mathf.Clamp(VerticalVelocity, -effectiveMaxFallSpeed, 50f);

        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, VerticalVelocity);
    }

    private void DrawJumpArc(float moveSpeed, Color gizmoColor)
    {
        float effectiveGravity = MovementStats.Gravity * GravityMultiplier;
        float effectiveMaxFallSpeed = MovementStats.MaxFallSpeed * MaxFallSpeedMultiplier;

        Vector2 startPosition = new Vector2(_feetCollider.bounds.center.x, _feetCollider.bounds.min.y);
        Vector2 previousPosition = startPosition;
        float speed = 0f;

        if (MovementStats.DrawRight)
        {
            speed = moveSpeed;
        }
        else
        {
            speed = -moveSpeed;
        }
        Vector2 velocity = new Vector2(speed, MovementStats.InitialJumpVelocity);

        Gizmos.color = gizmoColor;

        float timeStep = 2 * MovementStats.TimeTillJumpApex / MovementStats.ArcResolution; // Time stop for the simulation
        //float totalTime = (2 * MovementStats.TimeTillJumpApex) + MovementStats.ApexHangTime

        for (int i = 0; i<MovementStats.VisualizationSteps; i++)
        {
            float simulationTime = i * timeStep;
            Vector2 displacement;
            Vector2 drawPoint;

            if(simulationTime < MovementStats.TimeTillJumpApex) // Ascending
            {
                displacement = velocity * simulationTime + 0.5f * new Vector2(0, effectiveGravity) * simulationTime * simulationTime;
            } 
            else if(simulationTime < MovementStats.TimeTillJumpApex + MovementStats.ApexHangTime) // Apex hang Time
            {
                float apexTime = simulationTime - MovementStats.TimeTillJumpApex;
                displacement = velocity * MovementStats.TimeTillJumpApex + 0.5f * new Vector2(0, effectiveGravity) * MovementStats.TimeTillJumpApex * MovementStats.TimeTillJumpApex;
                displacement += new Vector2(speed, 0) * apexTime; // No vertical movement during hang time
            }
            else // Descending
            {
                float descendTime = simulationTime - (MovementStats.TimeTillJumpApex + MovementStats.ApexHangTime);
                displacement = velocity * MovementStats.TimeTillJumpApex + 0.5f * new Vector2(0, effectiveGravity) * MovementStats.TimeTillJumpApex * MovementStats.TimeTillJumpApex;
                displacement += new Vector2(speed, 0) * MovementStats.ApexHangTime; // Horizontal movement during hang time
                displacement += new Vector2(speed, 0) * descendTime + 0.5f * new Vector2(0, effectiveGravity) * descendTime * descendTime;
            }

            drawPoint = startPosition + displacement;

            if (MovementStats.StopOnCollision)
            {
                RaycastHit2D hit = Physics2D.Raycast(previousPosition, drawPoint - previousPosition, Vector2.Distance(previousPosition, drawPoint), MovementStats.GroundLayer);
                if (hit.collider != null)
                {
                    // If a hit is detected, stop drawing the arc at the hit point
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
    // Method to respawn player at the inicial position
    public void Respawn()
    {
        // reset velocities
        _rb.linearVelocity = Vector2.zero;
        VerticalVelocity = 0f;

        // Reset jumping/falling variables
        _isJumping = false;
        _isFacingRight = true;
        _isFalling = false;
        _isFastFalling = false;
        _fastFallTime = 0f;
        _isPastApexThreshold = false;
        _numberOfJumpUsed = 0;
        _coyoteTimer = 0f;
        _jumpBufferTimer = 0f;

        // Place Player at the spawn position
        transform.position = _playerSpawnPosition;
        transform.rotation = _playerSpawnRotation;
    }
    public void ResetInputModifiers()
    {
        _invertHorizontalStack = 0;
    }

    #endregion

    #region RunTimeModifiers
    public void MultiplyGravity(float multiplier)
    {
        GravityMultiplier *= multiplier;
    }

    public void MultiplyMaxFallSpeed(float multiplier)
    {
        MaxFallSpeedMultiplier *= multiplier;
    }
    public void MultiplySpeed(float multiplier)
    {
        SpeedMultiplier *= multiplier;
    }
    public void PushInvertHorizontal()
    {
        _invertHorizontalStack++;
    }
    public void PopInvertHorizontal()
    {
        _invertHorizontalStack--;
        if (_invertHorizontalStack < 0) _invertHorizontalStack = 0;
    }

    /// Resetea todos los multiplicadores de runtime a sus valores por defecto
    public void ResetMultipliers()
    {
        GravityMultiplier = 1f;
        MaxFallSpeedMultiplier = 1f;
        SpeedMultiplier = 1f;
        ResetInputModifiers(); // También resetea el input invertido

        Debug.Log("PlayerMovement: Multiplicadores reseteados a valores por defecto");
    }


    #endregion

    #region Collision Checks

    private void IsGrounded()
    {
        Vector2 boxCastOrigin = new Vector2(_feetCollider.bounds.center.x, _feetCollider.bounds.min.y);
        Vector2 boxCastSize = new Vector2(_feetCollider.bounds.size.x, MovementStats.GroundDetectionRayLenght);

        _groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, MovementStats.GroundDetectionRayLenght, MovementStats.GroundLayer);
        if (_groundHit.collider != null)
        {
            _isGrounded = true;
        }
        else
        {
            _isGrounded = false;
        }

        #region Debug Visualization
        if (MovementStats.DebugShowIsGrounderBox)
        {
            Color rayColor;
            if (_isGrounded)
            {
                rayColor = Color.green;
            }
            else
            {
                rayColor = Color.red;
            }

            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * MovementStats.GroundDetectionRayLenght, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * MovementStats.GroundDetectionRayLenght, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y - MovementStats.GroundDetectionRayLenght), Vector2.right * boxCastSize.x, rayColor);
        }
        #endregion
    }

    private void BumpedHead()
    {
        Vector2 boxCastOrigin = new Vector2(_feetCollider.bounds.center.x, _bodyCollider.bounds.max.y);
        Vector2 boxCastSize = new Vector2(_feetCollider.bounds.size.x * MovementStats.HeadWidth, MovementStats.HeadDetectionRayLenght);

        _headHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up, MovementStats.HeadDetectionRayLenght, MovementStats.GroundLayer);
        if(_headHit.collider != null)
        {
            _bumpedHead = true;
        }
        else
        {
            _bumpedHead = false;
        }

        #region Debug Visualization

        if (MovementStats.DebugShowHeadBumpBox)
        {
            float headWidth = MovementStats.HeadWidth;

            Color rayColor;
            if (_bumpedHead)
            {
                rayColor = Color.green;
            }
            else { rayColor = Color.red; }

            Debug.DrawRay(new Vector2(boxCastOrigin.x - (boxCastSize.x / 2), boxCastOrigin.y), Vector2.up * MovementStats.HeadDetectionRayLenght, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + (boxCastSize.x / 2), boxCastOrigin.y), Vector2.up *MovementStats.HeadDetectionRayLenght, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - (boxCastSize.x / 2), boxCastOrigin.y + MovementStats.HeadDetectionRayLenght), Vector2.right * boxCastSize.x *headWidth, rayColor);
        }
    }
    #endregion

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

        if (!_isGrounded)
        {
            _coyoteTimer -= Time.deltaTime;
        }
        else
        {
            _coyoteTimer = MovementStats.JumpCoyoteTime;
        }
    }

    #endregion
}
