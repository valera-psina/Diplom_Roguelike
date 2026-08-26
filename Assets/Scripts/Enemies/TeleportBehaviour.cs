using UnityEngine;

public class TeleportBehaviour : StateMachineBehaviour
{
    private RangedCellsEnemy enemy;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (enemy == null)
            enemy = animator.GetComponent<RangedCellsEnemy>();

        if (enemy != null)
            animator.SetBool("isShooting", false);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }
}