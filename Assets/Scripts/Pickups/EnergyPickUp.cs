using UnityEngine;

public class EnergyPickUp : PickUp
{
    protected override bool TryCollect(GameObject collector)
    {
        var energy = collector.GetComponent<Energy>();
        if (energy == null) return false;
        return energy.AddEnergy(amount);
    }
}