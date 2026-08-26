using UnityEngine;

public class WormIdleBehaviour : StateMachineBehaviour
{
    private Transform player;
    [SerializeField] private float shootRange = 20f;
    [SerializeField] private float minIdleTime = 1f;
    [SerializeField] private float maxIdleTime = 3f;

    private float enterTime;
    private float delay;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        animator.SetBool("isShooting", false);

        // Случайная задержка перед возможностью атаковать
        delay = Random.Range(minIdleTime, maxIdleTime);
        enterTime = Time.time;
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player == null) return;

        float distance = Vector3.Distance(animator.transform.position, player.position);

        // Если игрок в зоне и прошло время задержки – начинаем атаку
        if (distance < shootRange && Time.time >= enterTime + delay)
        {
            animator.SetBool("isShooting", true);
        }
    }
}