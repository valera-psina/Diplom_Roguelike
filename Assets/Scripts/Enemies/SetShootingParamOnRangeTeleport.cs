using UnityEngine;

public class SetShootingParamOnRangeTeleport : StateMachineBehaviour
{
    [SerializeField] private string parameterName = "isShooting";
    private int paramHash;
    private RangedCellsEnemy shooter;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        paramHash = Animator.StringToHash(parameterName);
        shooter = animator.GetComponent<RangedCellsEnemy>();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (shooter == null) return;
        animator.SetBool(paramHash, shooter.CanAttack);
    }
}