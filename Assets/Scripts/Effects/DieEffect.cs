using UnityEngine;

public class DieEffect : MonoBehaviour
{
    [SerializeField] private GameObject dieEffect;
    [SerializeField] private Transform pos;

    private Health health;

    private void Start()
    {
        health = GetComponent<Health>();
        if (health != null)
        {
            health.OnDie += OnDieEffect;
        }
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.OnDie -= OnDieEffect;
        }
    }

    public void OnDieEffect(GameObject killer)
    {
        if (dieEffect != null)
        {
            GameObject effect = Instantiate(dieEffect, pos.position, pos.rotation);
            Debug.Log("Эффект!!!");
            Destroy(effect, 3f);
        }
    }
}