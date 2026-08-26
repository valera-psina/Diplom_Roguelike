using UnityEngine;

public class JumpEnemyIdleBehaviour : StateMachineBehaviour
{
    private Transform player;
    private JumpEnemy enemy;
    private float jumpRange;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        enemy = animator.GetComponent<JumpEnemy>();
        jumpRange = enemy != null ? enemy.JumpRange : 8f;
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player == null || enemy == null) return;

        float distance = Vector3.Distance(animator.transform.position, player.position);
        if (distance <= jumpRange && enemy.CanAttack)
        {
            animator.SetBool("IsAttack", true);
        }
    }
}