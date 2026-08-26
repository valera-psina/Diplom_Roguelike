using UnityEngine;

public class JumpEnemyAttackBehaviour : StateMachineBehaviour
{
    private JumpEnemy enemy;
    private Transform player;
    private Transform enemyTransform;
    private Vector3 currentTarget;
    private float moveStartTime;
    private float moveEndTime;
    private float trackEndTime;
    private float stateElapsed;
    private bool targetLocked;
    private int lastCycle = -1;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        enemy = animator.GetComponent<JumpEnemy>();
        enemyTransform = animator.transform;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        lastCycle = -1;
        stateElapsed = 0f;
        targetLocked = false;
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (enemyTransform == null || enemy == null) return;

        float animDuration = stateInfo.length;
        int currentCycle = Mathf.FloorToInt(stateInfo.normalizedTime);

        // Новый цикл анимации — перезапускаем таймеры и цель
        if (currentCycle != lastCycle)
        {
            lastCycle = currentCycle;
            stateElapsed = 0f;
            targetLocked = false;

            moveStartTime = animDuration * enemy.MoveStartNorm;
            moveEndTime = animDuration * enemy.MoveEndNorm;
            trackEndTime = animDuration * enemy.TrackUntilNorm;

            enemy.GenerateNewJumpOffset();
            currentTarget = enemy.GetJumpTarget(player != null ? player.position : enemyTransform.position);
        }

        stateElapsed += Time.deltaTime;

        // Всегда поворачиваемся к игроку
        if (player != null)
        {
            Vector3 dir = (player.position - enemyTransform.position).normalized;
            dir.y = 0;
            if (dir != Vector3.zero)
                enemyTransform.rotation = Quaternion.Slerp(
                    enemyTransform.rotation,
                    Quaternion.LookRotation(dir),
                    Time.deltaTime * 15f
                );
        }

        // Фаза рывка
        if (stateElapsed >= moveStartTime && stateElapsed <= moveEndTime)
        {
            // Обновляем цель, пока не вышли за trackEndTime
            if (!targetLocked && stateElapsed < trackEndTime && player != null)
            {
                currentTarget = enemy.GetJumpTarget(player.position);
            }
            else
            {
                targetLocked = true;
            }

            // Двигаемся к текущей цели с постоянной скоростью
            enemyTransform.position = Vector3.MoveTowards(
                enemyTransform.position,
                currentTarget,
                enemy.JumpSpeed * Time.deltaTime
            );
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("IsAttack", false);
    }
}