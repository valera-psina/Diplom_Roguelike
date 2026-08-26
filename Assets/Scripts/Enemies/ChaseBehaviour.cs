using UnityEngine;
using UnityEngine.AI;

public class ChaseBehaviour : StateMachineBehaviour
{
    private NavMeshAgent agent;
    private Transform player;
    [SerializeField] private float attackRange = 2f;               // стандартная дистанция атаки
    [SerializeField] private float anticipationRange = 3.5f;       // начало анимации атаки заранее (attackRange + запас)
    [SerializeField] private float chaseRange = 50f;
    private bool isTriggeringAttack;
 
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        agent = animator.GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        isTriggeringAttack = false;

        if (agent != null)
        {
            agent.updateRotation = true;
            agent.isStopped = false;
        }
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (agent == null || player == null) return;

        float distance = Vector3.Distance(animator.transform.position, player.position);

        // Если уже атакует – не вмешиваемся (но это состояние Chase, сюда не должны попадать с isAttacking=true,
        // но оставим проверку для безопасности)
        if (animator.GetBool("isAttacking")) return;

        // Начинаем атаку заранее, как только вошли в anticipationRange
        if (distance < anticipationRange && !isTriggeringAttack)
        {
            isTriggeringAttack = true;
            animator.SetBool("isAttacking", true);
            // Агента НЕ останавливаем! Он продолжит идти к игроку.
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
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        isTriggeringAttack = false;
    }
}