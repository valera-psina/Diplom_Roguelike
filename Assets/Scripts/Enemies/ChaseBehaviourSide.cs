using UnityEngine;
using UnityEngine.AI;

public class ChaseBehaviourSide : StateMachineBehaviour
{
    private NavMeshAgent agent;
    private Transform player;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float anticipationRange = 3.5f;
    [SerializeField] private float chaseRange = 50f;
    private bool isTriggeringAttack;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        agent = animator.GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        isTriggeringAttack = false;

        if (agent != null)
        {
            agent.updateRotation = false;
            agent.isStopped = false;
        }
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (agent == null || player == null) return;

        float distance = Vector3.Distance(animator.transform.position, player.position);

        if (animator.GetBool("isAttacking")) return;

        if (distance < anticipationRange && !isTriggeringAttack)
        {
            isTriggeringAttack = true;
            animator.SetBool("isAttacking", true);
            return;
        }

        if (distance > chaseRange)
        {
            animator.SetBool("isChasing", false);
            agent.isStopped = true;
            return;
        }

        agent.isStopped = false;
        agent.SetDestination(player.position);

        if (agent.velocity.sqrMagnitude > 0.1f)
        {
            Vector3 moveDirection = agent.velocity.normalized;
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            Quaternion adjustedRotation = targetRotation * Quaternion.Euler(0, 90, 0);
            animator.transform.rotation = Quaternion.Slerp(animator.transform.rotation, adjustedRotation, Time.deltaTime * 10f);
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        isTriggeringAttack = false;
    }
}