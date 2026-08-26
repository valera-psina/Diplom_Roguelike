using UnityEngine;
using UnityEngine.AI;

public class AttackBehaviourSide : StateMachineBehaviour
{
    Transform player;
    NavMeshAgent agent;
    [SerializeField] private float cancelRange = 5f;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = animator.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.updateRotation = false;
        }
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player == null) return;

        Vector3 direction = (player.position - animator.transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            Quaternion adjustedRotation = targetRotation * Quaternion.Euler(0, 90, 0);
            animator.transform.rotation = Quaternion.Slerp(animator.transform.rotation, adjustedRotation, Time.deltaTime * 10f);
        }

        float distance = Vector3.Distance(animator.transform.position, player.position);
        if (distance > cancelRange)
            animator.SetBool("isAttacking", false);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (agent != null)
        {
            agent.isStopped = false;
            agent.updateRotation = true;
        }
    }
}