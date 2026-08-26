using System;
using System.Collections;
using System.Threading;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Player Controller")]
    [SerializeField] private float _moveSpeed = 10f;
    [SerializeField] private float _gravity = -9.81f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float airControl = 1f;
    [SerializeField] private float dashDuration = 1f;
    [SerializeField] private float dashSpeed = 25f;
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private CinemachineCamera _cinemachineCamera;

    [Header("Camera Bobbing")]
    [SerializeField] private float bobbingSpeed = 14f;
    [SerializeField] private float bobbingAmount = 0.05f;
    [SerializeField] private float bobbingResetSpeed = 5f;

    private Vector2 _move;
    private float _verticalVelocity;
    private Vector3 _horizontalVelocity;
    private float _currentMoveSpeed;
    private float _bobTimer;
    private Vector3 _cameraOriginalPosition;
    private float baseMaxSpeed;

    public Vector2 MoveInput => _move;

    private void Awake()
    {
        _currentMoveSpeed = _moveSpeed;
        baseMaxSpeed = _currentMoveSpeed;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (_cinemachineCamera != null)
            _cameraOriginalPosition = _cinemachineCamera.transform.localPosition;
    }

    public void OnMove(InputValue val)
    {
        _move = val.Get<Vector2>();
    }

    public void OnJump()
    {
        if (_characterController.isGrounded)
            _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * _gravity);
    }

    public void OnDash()
    {
        StartCoroutine(Dash());
    }

    private void Update()
    {
        if (_characterController.isGrounded && _verticalVelocity < 0)
            _verticalVelocity = -2f;
        else
            _verticalVelocity += _gravity * Time.deltaTime;

        Vector3 desiredMovement = (GetForward() * _move.y + GetRight() * _move.x) * _currentMoveSpeed;

        if (_characterController.isGrounded)
            _horizontalVelocity = desiredMovement;
        else
            _horizontalVelocity = Vector3.Lerp(_horizontalVelocity, desiredMovement, airControl * Time.deltaTime);

        Vector3 movement = _horizontalVelocity;
        movement.y = _verticalVelocity;

        _characterController.Move(movement * Time.deltaTime);

        HandleCameraBobbing();
    }

    private IEnumerator Dash()
    {
        yield return new WaitForSeconds(dashDuration);
    }

    private Vector3 GetForward()
    {
        Vector3 forward = _cinemachineCamera.transform.forward;
        forward.y = 0;
        return forward.normalized;
    }

    private Vector3 GetRight()
    {
        Vector3 right = _cinemachineCamera.transform.right;
        right.y = 0;
        return right.normalized;
    }

    private void HandleCameraBobbing()
    {
        float waveslice = 0f;
        float horizontal = _move.x;
        float vertical = _move.y;

        Vector3 cameraPosition = _cameraOriginalPosition;

        if (_characterController.isGrounded && (Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f))
        {
            _bobTimer += Time.deltaTime * bobbingSpeed;

            waveslice = Mathf.Sin(_bobTimer);

            float translateChange = waveslice * bobbingAmount;

            cameraPosition.y += translateChange;
            cameraPosition.x += Mathf.Cos(_bobTimer * 0.5f) * bobbingAmount * 0.5f;
        }
        else
        {
            _bobTimer = 0f;
            cameraPosition = Vector3.Lerp(_cinemachineCamera.transform.localPosition,
                                         _cameraOriginalPosition,
                                         Time.deltaTime * bobbingResetSpeed);
        }

        _cinemachineCamera.transform.localPosition = cameraPosition;
    }

    public void SetMoveSpeedMultiplier(float mult)
    {
        _currentMoveSpeed = baseMaxSpeed * mult;
    }
}