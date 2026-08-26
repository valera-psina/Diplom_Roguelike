using UnityEngine;

public class HealthPickUp : PickUp
{
    protected override bool TryCollect(GameObject collector)
    {
        var health = collector.GetComponent<Health>();
        if (health == null) return false;
        return health.Heal(amount);
    }
}
