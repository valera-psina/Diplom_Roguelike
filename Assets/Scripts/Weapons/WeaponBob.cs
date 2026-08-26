using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponBob : MonoBehaviour
{
    [SerializeField] private float bobSpeed = 8f;
    [SerializeField] private float bobAmount = 0.02f;
    [SerializeField] private float smooth = 10f;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerWeaponsManager weaponsManager;

    private Vector3 startLocalPos;
    private float timer;

    private void Start()
    {
        startLocalPos = transform.localPosition;
        if (weaponsManager == null)
            weaponsManager = FindAnyObjectByType<PlayerWeaponsManager>();
    }

    private void Update()
    {
        if (playerController == null) return;

        float bobScale = 1f;
        if (weaponsManager != null && weaponsManager.ActiveWeapon != null)
            bobScale = weaponsManager.ActiveWeapon.AimBobScale;

        Vector2 moveInput = playerController.MoveInput;
        float inputMag = moveInput.magnitude;

        if (inputMag > 0.1f)
        {
            timer += Time.deltaTime * bobSpeed;
            float wave = Mathf.Sin(timer) * inputMag * bobAmount * bobScale;
            transform.localPosition = startLocalPos + new Vector3(0f, wave, 0f);
        }
        else
        {
            timer = 0f;
            transform.localPosition = Vector3.Lerp(transform.localPosition, startLocalPos, Time.deltaTime * smooth);
        }
    }
}