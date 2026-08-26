using UnityEngine;

public class DynamicAim : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform muzzle;
    [SerializeField] private RectTransform reticle;
    [SerializeField] private Camera playerCamera;

    [Header("Settings")]
    [SerializeField] private float maxDistance = 100f;
    [SerializeField] private LayerMask hitLayers = -1;
    [SerializeField] private float reticleOffset = 0.05f;

    private void Start()
    {
        if (muzzle == null)
        {
            muzzle = transform.Find("Muzzle");
            if (muzzle == null)
                Debug.LogWarning("Muzzle Transform not assigned and not found as child named 'Muzzle'.", this);
        }

        if (reticle == null)
        {
            GameObject crosshairObj = GameObject.FindGameObjectWithTag("Crosshair");
            if (crosshairObj != null)
                reticle = crosshairObj.GetComponent<RectTransform>();
            else
                Debug.LogWarning("Reticle not assigned and not found with tag 'Crosshair'.", this);
        }

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
                Debug.LogError("Player Camera not assigned and Camera.main not found. Disabling script.", this);
        }

        if (playerCamera == null || reticle == null || muzzle == null)
        {
            Debug.LogError("DynamicAim: Missing critical references. Script disabled.", this);
            enabled = false;
        }
    }

    private void Update()
    {
        if (muzzle == null || reticle == null || playerCamera == null)
            return;

        Vector3 rayDirection = muzzle.forward;

        RaycastHit hit;
        Vector3 targetPoint;

        if (Physics.Raycast(muzzle.position, rayDirection, out hit, maxDistance, hitLayers))
        {
            targetPoint = hit.point + hit.normal * reticleOffset;
        }
        else
        {
            targetPoint = muzzle.position + rayDirection * maxDistance;
        }

        Vector3 screenPoint = playerCamera.WorldToScreenPoint(targetPoint);

        if (screenPoint.z > 0)
        {
            reticle.position = screenPoint;
            reticle.gameObject.SetActive(true);
        }
        else
        {
            reticle.gameObject.SetActive(false);
        }
    }
}