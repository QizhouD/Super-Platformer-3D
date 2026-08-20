using System.Collections.Generic;
using Cinemachine;
using KBCore.Refs;
using UnityEngine;
using Utilities;

namespace Platformer {
    public class PlayerController : ValidatedMonoBehaviour {
        [Header("References")]
        [SerializeField, Self] Rigidbody rb;
        [SerializeField, Self] GroundChecker groundChecker;
        [SerializeField, Self] Animator animator;
        [SerializeField, Anywhere] CinemachineFreeLook freeLookVCam;
        [SerializeField, Anywhere] InputReader input;
        public InputReader InputReader => input;
        public bool AllowDash => allowDash;
        public bool AllowDoubleJump => allowDoubleJump;
        public float MoveSpeed => moveSpeed;
        public float JumpForce => jumpForce;
        public float JumpDuration => jumpDuration;
        public float GravityMultiplier => gravityMultiplier;
        public float DashForce => dashForce;
        public float DashDuration => dashDuration;
        public event System.Action Jumped;
        public event System.Action Dashed;
        
        [Header("Movement Settings")]
        [SerializeField] float moveSpeed = 6f;
        [SerializeField] float rotationSpeed = 15f;
        [SerializeField] float smoothTime = 0.2f;
        
        [Header("Jump Settings")]
        [SerializeField] float jumpForce = 10f;
        [SerializeField] float jumpDuration = 0.5f;
        [SerializeField] float jumpCooldown = 0f;
        [SerializeField] float gravityMultiplier = 3f;
        //double jump
        [SerializeField] bool allowDoubleJump = false;
        bool hasDoubleJump;

        
        [Header("Dash Settings")]
        [SerializeField] float dashForce = 10f;
        [SerializeField] float dashDuration = 1f;
        [SerializeField] float dashCooldown = 2f;
        [SerializeField] bool allowDash = false;
        
        [Header("Attack Settings")]
        [SerializeField] float attackCooldown = 0.5f;
        [SerializeField] float attackDistance = 1f;
        [SerializeField] int attackDamage = 10;

        const float ZeroF = 0f;
        const float JumpBuffer = 0.12f;
        const float MinimumJumpHold = 0.1f;
        const float StepHeight = 0.45f;
        
        Transform mainCam;
        CapsuleCollider bodyCollider;
        PhysicMaterial noFriction;
        
        float currentSpeed;
        float velocity;
        float jumpVelocity;
        float dashVelocity = 1f;
        float jumpBufferUntil;
        float jumpStartedAt;

        Vector3 movement;

        List<Timer> timers;
        CountdownTimer jumpTimer;
        CountdownTimer jumpCooldownTimer;
        CountdownTimer dashTimer;
        CountdownTimer dashCooldownTimer;
        CountdownTimer attackTimer;
        
        StateMachine stateMachine;
        
        // Animator parameters
        static readonly int Speed = Animator.StringToHash("Speed");

        void Awake() {
            mainCam = Camera.main.transform;
            freeLookVCam.Follow = transform;
            freeLookVCam.LookAt = transform;
            // Invoke event when observed transform is teleported, adjusting freeLookVCam's position accordingly
            freeLookVCam.OnTargetObjectWarped(transform, transform.position - freeLookVCam.transform.position - Vector3.forward);
            
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            bodyCollider = GetComponent<CapsuleCollider>();
            ApplyNoFriction();
            
            SetupTimers();
            SetupStateMachine();
        }

        void SetupStateMachine() {
            // State Machine
            stateMachine = new StateMachine();

            // Declare states
            var locomotionState = new LocomotionState(this, animator);
            var jumpState = new JumpState(this, animator);
            var dashState = new DashState(this, animator);
            var attackState = new AttackState(this, animator);

            // Define transitions
            At(locomotionState, jumpState, new FuncPredicate(() => jumpTimer.IsRunning));
            At(locomotionState, dashState, new FuncPredicate(() => dashTimer.IsRunning));
            At(locomotionState, attackState, new FuncPredicate(() => attackTimer.IsRunning));
            At(attackState, locomotionState, new FuncPredicate(() => !attackTimer.IsRunning));
            Any(locomotionState, new FuncPredicate(ReturnToLocomotionState));

            // Set initial state
            stateMachine.SetState(locomotionState);
        }

        bool ReturnToLocomotionState() {
            var shouldReturn = groundChecker.IsGrounded
                               && !attackTimer.IsRunning
                               && !jumpTimer.IsRunning
                               && !dashTimer.IsRunning
                               && rb.velocity.y <= 0.6f;

            if (shouldReturn) hasDoubleJump = true;
            return shouldReturn;
        }


        void SetupTimers() {
            // Setup timers
            jumpTimer = new CountdownTimer(jumpDuration);
            jumpCooldownTimer = new CountdownTimer(jumpCooldown);

            jumpTimer.OnTimerStart += () => {
                jumpVelocity = jumpForce;
                Jumped?.Invoke();
            };
            jumpTimer.OnTimerStop += () => jumpCooldownTimer.Start();

            dashTimer = new CountdownTimer(dashDuration);
            dashCooldownTimer = new CountdownTimer(dashCooldown);

            dashTimer.OnTimerStart += () => {
                dashVelocity = dashForce;
                Dashed?.Invoke();
            };
            dashTimer.OnTimerStop += () => {
                dashVelocity = 1f;
                dashCooldownTimer.Start();
            };

            attackTimer = new CountdownTimer(attackCooldown);

            timers = new(5) {jumpTimer, jumpCooldownTimer, dashTimer, dashCooldownTimer, attackTimer};
        }

        void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
        void Any(IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);

        void Start() => input.EnablePlayerActions();

        void OnEnable() {
            input.Jump += OnJump;
            input.Dash += OnDash;
            input.Attack += OnAttack;
        }
        
        void OnDisable() {
            input.Jump -= OnJump;
            input.Dash -= OnDash;
            input.Attack -= OnAttack;
        }
        
        void OnAttack() {
            if (!attackTimer.IsRunning) {
                attackTimer.Start();
            }
        }

        public void Attack() {
            Vector3 attackPos = transform.position + transform.forward;
            Collider[] hitEnemies = Physics.OverlapSphere(attackPos, attackDistance);
            
            foreach (var enemy in hitEnemies) {
                Debug.Log(enemy.name);
                if (enemy.CompareTag("Enemy")) {
                    enemy.GetComponent<Health>().TakeDamage(attackDamage);
                }
            }
        }

        void OnJump(bool performed) {
            if (performed) {
                jumpBufferUntil = Time.time + JumpBuffer;
                TryConsumeJumpBuffer();
            } else if (jumpTimer.IsRunning && Time.time - jumpStartedAt >= MinimumJumpHold) {
                jumpTimer.Stop();
            }
        }
        
        void OnDash(bool performed) {
            if (!allowDash) return;
            if (performed && !dashTimer.IsRunning && !dashCooldownTimer.IsRunning) {
                dashTimer.Start();
            } else if (!performed && dashTimer.IsRunning) {
                dashTimer.Stop();
            }
        }

        void Update() {
            movement = new Vector3(input.Direction.x, 0f, input.Direction.y);
            TryConsumeJumpBuffer();
            stateMachine.Update();

            HandleTimers();
            UpdateAnimator();
        }

        void FixedUpdate() {
            stateMachine.FixedUpdate();
        }

        void UpdateAnimator() {
            animator.SetFloat(Speed, currentSpeed);
        }

        void HandleTimers() {
            foreach (var timer in timers) {
                timer.Tick(Time.deltaTime);
            }
        }

        public void HandleJump() {
            // If not jumping and grounded, keep jump velocity at 0
            if (!jumpTimer.IsRunning && groundChecker.IsGrounded) {
                jumpVelocity = ZeroF;
                return;
            }
            
            if (!jumpTimer.IsRunning) {
                // Gravity takes over
                jumpVelocity += Physics.gravity.y * gravityMultiplier * Time.fixedDeltaTime;
            }
            
            // Apply velocity
            rb.velocity = new Vector3(rb.velocity.x, jumpVelocity, rb.velocity.z);
        }

        public void HandleMovement() {
            var adjustedDirection = Quaternion.AngleAxis(mainCam.eulerAngles.y, Vector3.up) * movement;
            adjustedDirection.y = 0f;

            if (adjustedDirection.sqrMagnitude > 0.0001f) {
                HandleRotation(adjustedDirection);
                HandleHorizontalMovement(adjustedDirection.normalized);
                SmoothSpeed(adjustedDirection.magnitude);
            } else {
                SmoothSpeed(ZeroF);
                rb.velocity = new Vector3(ZeroF, rb.velocity.y, ZeroF);
            }
        }

        void HandleHorizontalMovement(Vector3 direction) {
            var desiredSpeed = ResolveMoveSpeed();
            var desired = direction * desiredSpeed;
            if (groundChecker.IsGrounded && !jumpTimer.IsRunning)
                desired = SlideAndStep(desired);

            rb.velocity = new Vector3(desired.x, rb.velocity.y, desired.z);
        }

        float ResolveMoveSpeed() {
            var speed = moveSpeed * dashVelocity;
            if (moveSpeed > 40f) speed *= Time.fixedDeltaTime;
            return speed;
        }

        Vector3 SlideAndStep(Vector3 desired) {
            if (bodyCollider == null) return desired;

            var radius = Mathf.Max(0.05f, bodyCollider.radius * 0.9f);
            var worldCenter = transform.TransformPoint(bodyCollider.center);
            var half = Mathf.Max(0f, bodyCollider.height * 0.5f - bodyCollider.radius);
            var bottom = worldCenter + Vector3.down * half;
            var top = worldCenter + Vector3.up * half;
            var travel = desired.magnitude * Time.fixedDeltaTime + 0.08f;
            var heading = desired.normalized;
            var mask = ~(1 << gameObject.layer);

            if (Physics.CapsuleCast(
                    bottom + Vector3.up * 0.05f,
                    top,
                    radius,
                    heading,
                    out var hit,
                    travel,
                    mask,
                    QueryTriggerInteraction.Ignore)) {
                if (hit.rigidbody == rb) return desired;

                if (TryStepUp(bottom, top, radius, heading, travel))
                    return desired;

                var slide = Vector3.ProjectOnPlane(desired, hit.normal);
                slide.y = 0f;
                if (slide.sqrMagnitude < 0.01f && groundChecker.IsGrounded)
                    rb.position += Vector3.up * 0.06f;
                return slide;
            }

            return desired;
        }

        bool TryStepUp(Vector3 bottom, Vector3 top, float radius, Vector3 heading, float travel) {
            var raisedBottom = bottom + Vector3.up * StepHeight;
            var raisedTop = top + Vector3.up * StepHeight;
            if (Physics.CapsuleCast(
                    raisedBottom,
                    raisedTop,
                    radius,
                    heading,
                    travel,
                    ~(1 << gameObject.layer),
                    QueryTriggerInteraction.Ignore))
                return false;

            rb.position += Vector3.up * StepHeight;
            return true;
        }

        void HandleRotation(Vector3 adjustedDirection) {
            var targetRotation = Quaternion.LookRotation(adjustedDirection);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime);
        }

        void TryConsumeJumpBuffer() {
            if (Time.time > jumpBufferUntil) return;
            if (jumpTimer.IsRunning || jumpCooldownTimer.IsRunning) return;

            if (groundChecker.CanJump) {
                jumpTimer.Start();
                jumpStartedAt = Time.time;
                jumpBufferUntil = 0f;
                return;
            }

            if (allowDoubleJump && hasDoubleJump) {
                jumpTimer.Start();
                jumpStartedAt = Time.time;
                jumpBufferUntil = 0f;
                hasDoubleJump = false;
            }
        }

        void ApplyNoFriction() {
            noFriction = new PhysicMaterial("PlayerNoFriction") {
                dynamicFriction = 0f,
                staticFriction = 0f,
                bounciness = 0f,
                frictionCombine = PhysicMaterialCombine.Minimum,
                bounceCombine = PhysicMaterialCombine.Minimum
            };
            if (bodyCollider != null) bodyCollider.material = noFriction;
        }

        void OnDestroy() {
            if (noFriction != null) Destroy(noFriction);
        }

        void SmoothSpeed(float value) {
            currentSpeed = Mathf.SmoothDamp(currentSpeed, value, ref velocity, smoothTime);
        }
        
        public void EnableDoubleJump()
        {
            allowDoubleJump = true;
            UINotification.ShowNotification("Double Jump Ability Unlocked!");
            Debug.Log("doubleJump unlock！");
        }
        
        public void EnableDash()
        {
            allowDash = true;
            UINotification.ShowNotification("Dash Ability Unlocked!");
            Debug.Log("Dash ability unlocked！");
        }

        public void SetTraversalAbilities(bool doubleJumpEnabled, bool dashEnabled) {
            allowDoubleJump = doubleJumpEnabled;
            allowDash = dashEnabled;
            if (!allowDoubleJump) hasDoubleJump = false;
        }

    }
}
