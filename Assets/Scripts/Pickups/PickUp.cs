using Unity.VisualScripting;
using UnityEngine;

[RequireComponent (typeof(Collider))]
public abstract class PickUp : MonoBehaviour
{
    [SerializeField] protected int amount = 10;
    [SerializeField] protected string playerTag = "Player";
    [SerializeField] private AudioClip pickupSound;

    protected abstract bool TryCollect(GameObject collector);

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (TryCollect(other.gameObject))
        {
            if (pickupSound != null)
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            Destroy(gameObject);
        }
    }

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }
}
