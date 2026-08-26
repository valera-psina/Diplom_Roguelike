using UnityEngine;

public class WormShootBehaviour : StateMachineBehaviour
{
    private Transform player;
    private float shootRange = 70f;
    private float exitRange = 72f;
    private float rotationSpeed = 5f;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player == null) return;

        Vector3 direction = (player.position - animator.transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            Quaternion adjusted = targetRotation * Quaternion.Euler(0, 90, 0);
            animator.transform.rotation = Quaternion.Slerp(animator.transform.rotation, adjusted, rotationSpeed * Time.deltaTime);
        }

        float distance = Vector3.Distance(animator.transform.position, player.position);
        if (distance > exitRange)
            animator.SetBool("isShooting", false);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Сбрасываем флаг атаки при выходе из состояния (даже если по окончании анимации)
        animator.SetBool("isShooting", false);
    }
}