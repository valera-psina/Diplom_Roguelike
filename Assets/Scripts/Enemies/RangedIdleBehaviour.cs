using UnityEngine;

public class RangedIdleBehaviour : StateMachineBehaviour
{
    private Transform player;
    [SerializeField] private float shootRange = 100f;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator.SetBool("isShooting", false);
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player == null) return;

        float distance = Vector3.Distance(animator.transform.position, player.position);
        if (distance < shootRange)
        {
            animator.SetBool("isShooting", true);
        }
    }
}