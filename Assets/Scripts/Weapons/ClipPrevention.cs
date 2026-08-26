using UnityEngine;

public class ClipPrevention : MonoBehaviour
{
    [SerializeField] private GameObject clipProjector;
    [SerializeField] private float checkDistance;
    [SerializeField] private Vector3 newDirection;
    [SerializeField] private LayerMask collisionLayers = -1;
    [SerializeField] private float smoothSpeed = 5f;
    private float currentLerpPos;
    private RaycastHit hit;

    private void Start()
    {
        if (clipProjector == null)
        {
            GameObject clipProjectorObj = GameObject.FindGameObjectWithTag("ClipProjector");
            if (clipProjectorObj != null)
                clipProjector = clipProjectorObj;
        }
    }

    private void Update()
    {
        float targetLerpPos = 0f;
        if (Physics.Raycast(clipProjector.transform.position, clipProjector.transform.forward, out hit, checkDistance, collisionLayers))
            targetLerpPos = 1 - (hit.distance / checkDistance);
        targetLerpPos = Mathf.Clamp01(targetLerpPos);
        currentLerpPos = Mathf.Lerp(currentLerpPos, targetLerpPos, smoothSpeed * Time.deltaTime);
        transform.localRotation =
            Quaternion.Lerp(
                Quaternion.Euler(Vector3.zero),
                Quaternion.Euler(newDirection),
                currentLerpPos
                );
    }
}
