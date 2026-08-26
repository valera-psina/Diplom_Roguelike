using UnityEngine;

public class ShellCollisionHandler : MonoBehaviour
{
    [SerializeField] private int newLayer = 9;
    private bool hasCollided = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (hasCollided) return;

        SetLayerRecursively(gameObject, newLayer);
        hasCollided = true;
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}