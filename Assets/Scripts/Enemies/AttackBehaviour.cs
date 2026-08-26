using UnityEngine;
using UnityEngine.AI;

public class AttackBehaviour : StateMachineBehaviour
{
    Transform player;
    NavMeshAgent agent;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float cancelRange = 5f;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = animator.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            // Не останавливаем агента! Враг продолжает идти к игроку.
            agent.updateRotation = false;   // будем поворачивать вручную для точной наводки
        }
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player == null) return;

        // Плавный поворот к игроку (без компенсации, так как модель не повёрнута)
        Vector3 direction = (player.position - animator.transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            animator.transform.rotation = Quaternion.Slerp(animator.transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        // Если игрок убежал слишком далеко – отменяем атаку
        float distance = Vector3.Distance(animator.transform.position, player.position);
        if (distance > cancelRange)
        {
            animator.SetBool("isAttacking", false);
        }
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