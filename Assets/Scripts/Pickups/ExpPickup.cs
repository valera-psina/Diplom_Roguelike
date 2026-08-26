using UnityEngine;

public class ExpPickup : PickUp
{
    protected override bool TryCollect(GameObject collector)
    {
        var exp = collector.GetComponent<Level>();
        if (exp == null) return false;
        return exp.AddExperience(amount);
    }
}
