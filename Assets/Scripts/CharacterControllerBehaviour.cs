using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(CharacterController))]
public class CharacterControllerBehaviour : MonoBehaviour {

    private CharacterController _characterController;
    private Vector3 _velocity = Vector3.zero;
    private Vector3 _movement;
    private bool _jump;
    private bool _isJumping;
    public bool canClimb;
    private bool _climb;
    private bool _stab;
    private Animator _animator;
    private int _horizontalVelocityParameter = Animator.StringToHash("HorizontalVelocity");
    private int _verticalVelocityParameter = Animator.StringToHash("VerticalVelocity");


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

    [SerializeField]
    private GameObject _playerCamera;
    [SerializeField]
    private GameObject _playerPivot;

    private float _cameraMultiplier = 2;

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

    }

    void Update () {
        _movement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        if (Input.GetButtonDown("Jump"))
        {
            _jump = true;
        }
        if (Input.GetButtonDown("Fire3"))
        {
            _climb = true;
        }
        if (Input.GetButtonDown("Fire1"))
        {
            _stab = true;
        }
        MovementAnimations();
        RotateCamera();
    }

    void FixedUpdate()
    {
        ApplyGround();
        ApplyGravity();
        ApplyMovement();
        ApplyGroundDrag();
        LimitMaximumRunningSpeed();
        ApplyJump();
        _characterController.Move(_velocity * Time.deltaTime);
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

    //private void ApplyJump()
    //{
    //    if (_jump && _characterController.isGrounded)
    //    {
    //        _velocity += -Physics.gravity.normalized * Mathf.Sqrt(2 * Physics.gravity.magnitude * _jumpHeight);
    //        _jump = false;
    //        _animator.SetTrigger("Jump");
    //    }
    //}

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

    private void ApplyClimb()
    {
        if (canClimb == true && _climb)
        {
            _movement += new Vector3(0, 100, 0);
            canClimb = false;
        }
    }

    private void ApplyStab()
    {
        if (_stab)
        {
            this.gameObject.GetComponent<BoxCollider>().enabled = true;
        }
    }
    private void MovementAnimations()
    {
        //left thumbstick input
        _animator.SetFloat(_horizontalVelocityParameter, _movement.x);
        _animator.SetFloat(_verticalVelocityParameter, _movement.z);
    }

    public void RotateCamera()
    {
        Vector3 tempRot = transform.localEulerAngles;
        tempRot.y += Input.GetAxis("HorizontalCam") * _cameraMultiplier;
        transform.localEulerAngles = tempRot;

        Vector3 rotationCamPivot = _playerPivot.transform.localEulerAngles;
        rotationCamPivot.x += Input.GetAxis("VerticalCam") * -_cameraMultiplier;

        rotationCamPivot.x = ClampAngle(rotationCamPivot.x, -35, 35);
        _playerPivot.transform.localEulerAngles = rotationCamPivot;
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

}

