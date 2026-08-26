using UnityEngine;

public class AnimationEventRelay : MonoBehaviour
{
    private MeleeEnemy parentEnemy;

    private void Awake()
    {
        parentEnemy = GetComponentInParent<MeleeEnemy>();
    }

    // Эти методы вызываются из анимационных событий
    public void PlayFootstep()
    {
        if (parentEnemy != null)
            parentEnemy.PlayFootstep();
    }

    public void PlaySwooshSound()
    {
        if (parentEnemy != null)
            parentEnemy.PlaySwooshSound();
    }

    public void PerformAttack()
    {
        if (parentEnemy != null)
            parentEnemy.PerformAttack();
    }
}