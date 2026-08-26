using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponSway : MonoBehaviour
{
    [Header("Position Sway")]
    [SerializeField] private float mouseSwayAmount = 0.05f;
    [SerializeField] private float mouseSmoothTime = 0.1f;
    [SerializeField] private float movementSwayAmount = 0.03f;
    [SerializeField] private float movementSmoothTime = 0.15f;

    [Header("Rotation Sway")]
    [SerializeField] private float rotationSmooth = 5f;
    [SerializeField] private float rotationMultiplier = 0.1f;

    [Header("Ref")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerWeaponsManager weaponsManager;

    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;

    private void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main?.transform;

        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;

        if (playerController == null)
            playerController = FindAnyObjectByType<PlayerController>();
        if (weaponsManager == null)
            weaponsManager = FindAnyObjectByType<PlayerWeaponsManager>();
    }

    private void Update()
    {
        float swayScale = 1f;
        float rotationScale = 1f;

        if (weaponsManager != null && weaponsManager.ActiveWeapon != null)
        {
            swayScale = weaponsManager.ActiveWeapon.AimSwayScale;
            rotationScale = weaponsManager.ActiveWeapon.AimRotationScale;
        }

        Vector3 mousePosSway = CalculateMousePosSway(swayScale);
        Vector3 movementSway = CalculateMovementSway(swayScale);
        Vector3 targetPosition = initialLocalPosition + mousePosSway + movementSway;

        float posSmoothFactor = Time.deltaTime / ((mouseSmoothTime + movementSmoothTime) * 2f) * 2f;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, posSmoothFactor);

        ApplyRotationSway(rotationScale);
    }

    private Vector3 CalculateMousePosSway(float scale)
    {
        Vector2 mouseDelta = Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;
        Vector3 sway = new Vector3(
            -mouseDelta.x * mouseSwayAmount * 0.01f * scale,
            -mouseDelta.y * mouseSwayAmount * 0.01f * scale,
            0f
        );
        return Vector3.ClampMagnitude(sway, mouseSwayAmount * scale);
    }

    private Vector3 CalculateMovementSway(float scale)
    {
        if (playerController == null) return Vector3.zero;
        Vector2 moveInput = playerController.MoveInput;
        if (moveInput.sqrMagnitude < 0.001f) return Vector3.zero;

        Vector3 worldMoveDir = cameraTransform.right * moveInput.x +
                               cameraTransform.forward * moveInput.y;
        worldMoveDir.y = 0f;
        worldMoveDir.Normalize();

        Transform parentTransform = transform.parent;
        Vector3 localMoveDir = parentTransform.InverseTransformDirection(worldMoveDir);

        Vector3 sway = -localMoveDir * (moveInput.magnitude * movementSwayAmount * scale);
        return Vector3.ClampMagnitude(sway, movementSwayAmount * scale);
    }

    private void ApplyRotationSway(float scale)
    {
        Vector2 mouseDelta = Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;

        float mult = rotationMultiplier * scale;
        float mouseX = mouseDelta.x * mult;
        float mouseY = mouseDelta.y * mult;

        Quaternion rotationX = Quaternion.AngleAxis(-mouseY, Vector3.right);
        Quaternion rotationY = Quaternion.AngleAxis(mouseX, Vector3.up);

        Quaternion targetRotation = initialLocalRotation * rotationX * rotationY;

        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRotation,
            rotationSmooth * Time.deltaTime
        );
    }
}