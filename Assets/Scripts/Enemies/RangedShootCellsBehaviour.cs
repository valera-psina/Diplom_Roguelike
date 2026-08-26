using UnityEngine;

public class RangedShootCellsBehaviour : StateMachineBehaviour
{
    private Transform player;
    private float shootRange = 100f;
    private float exitRange = 110f;
    private float rotationSpeed = 5f;
    private bool hasShot = false;

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
            targetRotation *= Quaternion.Euler(0, 90, 0);
            animator.transform.rotation = Quaternion.Slerp(
                animator.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        float distance = Vector3.Distance(animator.transform.position, player.position);
        if (distance > exitRange)
        {
            animator.SetBool("isShooting", false);
            return;
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        hasShot = false;
    }
}