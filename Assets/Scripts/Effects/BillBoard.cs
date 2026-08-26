using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;

    private void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null)
            targetCamera = FindFirstObjectByType<Camera>();
    }

    private void LateUpdate()
    {
        if (targetCamera == null) return;

        transform.LookAt(transform.position + targetCamera.transform.rotation * Vector3.forward,
                         targetCamera.transform.rotation * Vector3.up);

    }
}