using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class PlayerController : Actor
{
    [Header("Player")]
    [Tooltip("Move speed of the character in m/s")]
    public float RunSpeed = 5.335f;
    [Tooltip("Sprint speed of the character in m/s")]
    public float SprintSpeed = 7.335f;
    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;
    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 10.0f;

    [Space(10)]
    [Tooltip("The height the player can jump")]
    public float JumpHeight = 1.2f;
    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float Gravity = -15.0f;

    [Space(10)]
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    public float JumpTimeout = 0.50f;
    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float FallTimeout = 0.15f;

    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool IsGrounded = true;
    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;
    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.28f;
    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;

    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;
    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 70.0f;
    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -30.0f;
    public GameObject followCamera;
    public GameObject aimCamera;

    [Header("Sensitivity")]
    public float LookSensitivity = 1f;
    public float AimSensitivity = .6f;
    public bool IsAiming = false;

    [Header("Bullets")]
    public Transform BulletParent;
    public Transform BulletSpawnPoint;
    public GameObject BulletPrefab;
    public float BulletMissDespawnDistance = 25;

    // cinemachine
    private float cinemachineTargetYaw;
    private float cinemachineTargetPitch;

    // player
    private float speed;
    private float targetRotation = 0.0f;
    private float rotationVelocity;
    public float verticalVelocity;
    private float terminalVelocity = 53.0f;

    // timeout deltatime
    private float jumpTimeoutDelta;
    private float fallTimeoutDelta;

    private PlayerInput playerInput;
    private CharacterController controller;
    private GameObject mainCamera;

    private const float cameraRotationThreshold = 0.01f;

    // store controls
    private InputAction lookAction;
    private InputAction moveAction;
    private InputAction sprintAction;
    private InputAction jumpAction;
    private InputAction aimAction;
    private InputAction shootAction;


#if !UNITY_IOS || !UNITY_ANDROID
    [Header("Mouse Cursor Settings")]
    public bool cursorLocked = true;
    public bool cursorInputForLook = true;
#endif

    private void Awake()
    {
        // get a reference to our main camera
        if (mainCamera == null)
        {
            mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
    }

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        InitializeActions();
        jumpTimeoutDelta = JumpTimeout;
        fallTimeoutDelta = FallTimeout;
    }

    private void Update()
    {
        HandleAiming();
        AimCheck();
        JumpAndGravity();
        GroundedCheck();
        Move();
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    private void OnDrawGizmosSelected()
    {
        var transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        var transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);
        Gizmos.color = IsGrounded ? transparentGreen : transparentRed;

        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawSphere(
            center: new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
            radius: GroundedRadius);
    }

    private void OnDisable()
    {
        shootAction.performed -= ShootGun;
    }

    private void InitializeActions()
    {
        lookAction = playerInput.actions["Look"];
        moveAction = playerInput.actions["Move"];
        sprintAction = playerInput.actions["Sprint"];
        jumpAction = playerInput.actions["Jump"];
        aimAction = playerInput.actions["Aim"];
        shootAction = playerInput.actions["Shoot"];
        shootAction.performed += ShootGun;
    }

    private void ShootGun(CallbackContext context)
    {
        RaycastHit hit;

        GameObject bullet = Instantiate(
            original: BulletPrefab,
            position: BulletSpawnPoint.position,
            rotation: mainCamera.transform.rotation,
            parent: BulletParent);

        var projectile = bullet.GetComponent<Projectile>();

        if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out hit, Mathf.Infinity))
        {
            projectile.Target = hit.point;
            projectile.IsHit = true;
        }
        else
        {
            var cameraTransform = mainCamera.transform;
            projectile.Target = cameraTransform.position + cameraTransform.forward * BulletMissDespawnDistance;
            projectile.IsHit = false;
        }
    }

    private void AimCheck()
    {
        IsAiming = aimAction.IsPressed();
    }

    private void JumpAndGravity()
    {
        if (IsGrounded)
        {
            // reset the fall timeout timer
            fallTimeoutDelta = FallTimeout;

            // stop our velocity dropping infinitely when grounded
            if (verticalVelocity < 0.0f)
            {
                verticalVelocity = -2f;
            }

            // Jump
            if (jumpAction.IsPressed() && jumpTimeoutDelta <= 0.0f)
            {
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
            }

            // jump timeout
            if (jumpTimeoutDelta >= 0.0f)
            {
                jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            // reset the jump timeout timer
            jumpTimeoutDelta = JumpTimeout;

            // fall timeout
            if (fallTimeoutDelta >= 0.0f)
            {
                fallTimeoutDelta -= Time.deltaTime;
            }

            // if we are not grounded, do not jump
            jumpAction.Reset();
        }

        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (verticalVelocity < terminalVelocity)
        {
            verticalVelocity += Gravity * Time.deltaTime;
        }
    }

    private void GroundedCheck()
    {
        // set sphere position, with offset
        var spherePosition = new Vector3(
            x: transform.position.x,
            y: transform.position.y - GroundedOffset,
            z: transform.position.z);

        IsGrounded = Physics.CheckSphere(
            position: spherePosition,
            radius: GroundedRadius,
            layerMask: GroundLayers,
            queryTriggerInteraction: QueryTriggerInteraction.Ignore);
    }

    private void Move()
    {
        // set target speed based on move speed, sprint speed and if sprint is pressed
        float targetSpeed = sprintAction.IsPressed() ? SprintSpeed : RunSpeed;

        // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

        // note: Vector2's == operator uses approximation so is not floating point error prone,
        // and is cheaper than magnitude if there is no input, set the target speed to 0
        var move = moveAction.ReadValue<Vector2>();

        if (move == Vector2.zero)
        {
            targetSpeed = 0.0f;
        }

        // a reference to the players current horizontal velocity
        float currentHorizontalSpeed = new Vector3(controller.velocity.x, 0.0f, controller.velocity.z).magnitude;
        float speedOffset = 0.1f;
        float inputMagnitude = 1f;

        // accelerate or decelerate to target speed
        if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            // creates curved result rather than a linear one giving a more organic speed change
            // note T in Lerp is clamped, so we don't need to clamp our speed
            speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

            // round speed to 3 decimal places
            speed = Mathf.Round(speed * 1000f) / 1000f;
        }
        else
        {
            speed = targetSpeed;
        }

        // normalise input direction
        Vector3 inputDirection = new Vector3(move.x, 0.0f, move.y).normalized;

        // note: Vector2's != operator uses approximation so is not floating point error prone, and is
        // cheaper than magnitude if there is a move input rotate player when the player is moving
        if (move != Vector2.zero)
        {
            targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z)
                * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;

            float rotation = Mathf.SmoothDampAngle(
                current: transform.eulerAngles.y,
                target: targetRotation,
                currentVelocity: ref rotationVelocity,
                smoothTime: RotationSmoothTime);

            // rotate to face input direction relative to camera position
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        if (IsAiming)
        {
            // face the way the camera is facing
            transform.rotation = Quaternion.Euler(0.0f, mainCamera.transform.eulerAngles.y, 0.0f);
        }

        // move the player
        Vector3 targetDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;
        targetDirection = targetDirection.normalized * (speed * Time.deltaTime);
        Vector3 velocity = new Vector3(0.0f, verticalVelocity, 0.0f) * Time.deltaTime;
        controller.Move(targetDirection + velocity);
    }

    private void CameraRotation()
    {
        var look = lookAction.ReadValue<Vector2>();

        // if there is an input and camera position is not fixed
        if (look.sqrMagnitude >= cameraRotationThreshold)
        {
            //Don't multiply mouse input by Time.deltaTime;
            float sensitivity = IsAiming ? AimSensitivity : LookSensitivity;
            float deltaTimeMultiplier = 1f * sensitivity;
            cinemachineTargetYaw += look.x * deltaTimeMultiplier;
            cinemachineTargetPitch += look.y * deltaTimeMultiplier;
        }

        // clamp our rotations so our values are limited 360 degrees
        cinemachineTargetYaw = ClampAngle(cinemachineTargetYaw, float.MinValue, float.MaxValue);
        cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, BottomClamp, TopClamp);

        // Cinemachine will follow this target
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(
            x: cinemachineTargetPitch,
            y: cinemachineTargetYaw,
            z: 0.0f);
    }

    private void HandleAiming()
    {
        if (aimAction.IsPressed())
        {
            aimCamera.SetActive(true);
            followCamera.SetActive(false);
        }
        else
        {
            followCamera.SetActive(true);
            aimCamera.SetActive(false);
        }
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f)
        {
            lfAngle += 360f;
        }

        if (lfAngle > 360f)
        {
            lfAngle -= 360f;
        }

        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }


#if !UNITY_IOS || !UNITY_ANDROID
    private void OnApplicationFocus(bool hasFocus)
    {
        SetCursorState(cursorLocked);
    }

    private void SetCursorState(bool newState)
    {
        Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
    }
#endif
}
