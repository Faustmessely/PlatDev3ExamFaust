using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(CharacterController))]
public class CharacterControllerBehaviour : MonoBehaviour {

    private CharacterController _characterController;
    private Vector3 _velocity = Vector3.zero;
    private Vector3 _movement;
    //jump vars
    private bool _jump;
    private bool _isJumping;
    private bool _isFalling=false;
    //climb vars
    public bool IsClimbing;
    private bool _climb;
    //stab vars
    private bool _stab;
    [SerializeField] private GameObject _stabHitBox;
    //animator vars
    private Animator _animator;
    private int _horizontalVelocityParameter = Animator.StringToHash("HorizontalVelocity");
    private int _verticalVelocityParameter = Animator.StringToHash("VerticalVelocity");
    //gun vars
    public bool HasWeapon = false;
    private float _timer;
    private bool _startTimer;
    private bool _shoot;
    private Ray _bullet;
    //basicMovement vars
    [Header("Locomotion Parameters")]
    [SerializeField]
    private float _mass = 75; // [kg]
    [SerializeField]
    private float _acceleration = 3; // [m/s^2]
    [SerializeField]
    private float _dragOnGround = 0;
    [SerializeField]
    private float _jumpHeight = 5;
    [SerializeField]
    private float _maxRunningSpeed = (30.0f * 1000) / (60 * 60); // [m/s], 30 km/h
    //camera vars
    [SerializeField]
    private GameObject _playerCamera;
    [SerializeField]
    private GameObject _playerPivot;
    [SerializeField]
    private GameObject _mainCamera;
    private float _cameraMultiplier = 2.5f;
    private bool _cameraIsZoomed = true;
    [Header("Dependencies")]
    [SerializeField, Tooltip("What should determine the absolute forward when a player presses forward.")]
    private Transform _absoluteForward;

    void Start () {
        _characterController = GetComponent<CharacterController>();

#if DEBUG
        Assert.IsNotNull(_characterController, "Dependency Error: This component needs a CharachterController to work.");
        Assert.IsNotNull(_absoluteForward, "Dependency Error: Set the Absolute Forward field.");
#endif
        _animator = GetComponent<Animator>();
        _stabHitBox.SetActive(false);
    }

    void Update () {
        _movement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        if (Input.GetButtonDown("Jump"))
        {
            _jump = true;
        }
        if (Input.GetButtonDown("Interact"))
        {
            _climb = true;
        }
        if (Input.GetButtonDown("Fire1"))
        {
            _stab = true;
        }
        if (Input.GetButtonDown("Fire2")&&HasWeapon==true)
        {
            _shoot = true;
        }

        MovementAnimations();
        RotateCamera();
        _animator.SetBool("IsGrounded", _characterController.isGrounded);
        _animator.SetBool("HasWeapon", HasWeapon);
        _animator.SetBool("Stab", _stab);
        _animator.SetBool("Shoot", _shoot);

        if (HasWeapon == true)
        {
            _startTimer = true;
        }

        if (_timer==1000)
        {
            HasWeapon = false;
            _timer = 0;
        }
    }

    void FixedUpdate()
    {
        ApplyGround();
        ApplyGravity();
        if (IsClimbing==false)
        {
            ApplyMovement();
        }
        ApplyGroundDrag();
        LimitMaximumRunningSpeed();
        ApplyJump();
        _characterController.Move(_velocity * Time.deltaTime);
        if (_startTimer==true)
        {
            _timer++;
        }
    }

    private void ApplyGravity()
    {
        if (!_characterController.isGrounded)
        {
            _velocity += Physics.gravity * Time.deltaTime; // g[m/s^2] * t[s]
        }
    }

    private void ApplyGround()
    {
        if (_characterController.isGrounded)
        {
            _velocity -= Vector3.Project(_velocity, Physics.gravity.normalized);
        }
    }

    private void ApplyMovement()
    {
        if (_characterController.isGrounded)
        {
            Vector3 xzAbsoluteForward = Vector3.Scale(_absoluteForward.forward, new Vector3(1, 0, 1));

            Quaternion forwardRotation =
                 Quaternion.LookRotation(xzAbsoluteForward);

            Vector3 relativeMovement = forwardRotation * _movement;

            _velocity += relativeMovement * _acceleration * Time.deltaTime; // F(= m.a) [m/s^2] * t [s]
        }
    }

    private void ApplyGroundDrag()
    {
        if (_characterController.isGrounded)
        {
            _velocity = _velocity * (1 - Time.deltaTime * _dragOnGround);
        }
    }

    private void LimitMaximumRunningSpeed()
    {
        Vector3 yVelocity = Vector3.Scale(_velocity, new Vector3(0, 1, 0));
        Vector3 xzVelocity = Vector3.Scale(_velocity, new Vector3(1, 0, 1));

        Vector3 clampedXzVelocity = Vector3.ClampMagnitude(xzVelocity, _maxRunningSpeed);

        _velocity = yVelocity + clampedXzVelocity;
    }

    private void ApplyJump()
    {
        if (_jump && _characterController.isGrounded)
        {
            _velocity += -Physics.gravity.normalized * Mathf.Sqrt(2 * Physics.gravity.magnitude * _jumpHeight);
            _jump = false;
            _animator.SetTrigger("Jump");
            _isJumping = true;
        }
        else if (_characterController.isGrounded)
        {
            _isJumping = false;
        }
    }

    private void MovementAnimations()
    {
        //left thumbstick input
        _animator.SetFloat(_horizontalVelocityParameter, _movement.x);
        _animator.SetFloat(_verticalVelocityParameter, _movement.z);
        _animator.SetBool("IsClimbing", IsClimbing);
    }
        
    public void RotateCamera()
    {
        if (IsClimbing==false)
        {
            Vector3 tempRot = transform.localEulerAngles;
            tempRot.y += Input.GetAxis("HorizontalCam") * _cameraMultiplier;
            transform.localEulerAngles = tempRot;

            Vector3 rotationCamPivot = _playerPivot.transform.localEulerAngles;
            rotationCamPivot.x += Input.GetAxis("VerticalCam") * -_cameraMultiplier;

            rotationCamPivot.x = ClampAngle(rotationCamPivot.x, -35, 35);
            _playerPivot.transform.localEulerAngles = rotationCamPivot;
        }
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        angle = Mathf.Repeat(angle, 360);
        min = Mathf.Repeat(min, 360);
        max = Mathf.Repeat(max, 360);
        bool inverse = false;
        var tmin = min;
        var tangle = angle;
        if (min > 180)
        {
            inverse = !inverse;
            tmin -= 180;
        }
        if (angle > 180)
        {
            inverse = !inverse;
            tangle -= 180;
        }
        var result = !inverse ? tangle > tmin : tangle < tmin;
        if (!result)
            angle = min;

        inverse = false;
        tangle = angle;
        var tmax = max;
        if (angle > 180)
        {
            inverse = !inverse;
            tangle -= 180;
        }
        if (max > 180)
        {
            inverse = !inverse;
            tmax -= 180;
        }

        result = !inverse ? tangle < tmax : tangle > tmax;
        if (!result)
            angle = max;
        return angle;
    }

    public void FinishClimbing()
    {
        IsClimbing = false;
        transform.position = new Vector3(transform.position.x + (transform.forward.x), transform.position.y + 1.15f, transform.position.z + (transform.forward.z));
    }

    public void Stab()
    {
        _stabHitBox.SetActive(true);
        //GetComponent<BoxCollider>().enabled = true;
    }

    public void StabEnd()
    {
        _stab = false;
        _stabHitBox.SetActive(false);
        //GetComponent<BoxCollider>().enabled = false;
    }

    public void ShootBullet()
    {
        _bullet = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        _shoot = false;
    }
}

