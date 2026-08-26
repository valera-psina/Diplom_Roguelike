using UnityEngine;

public class PoltergeistSMBehaviour : StateMachineBehaviour
{
    [SerializeField] private string detectedParam = "IsPlayerDetected";
    [SerializeField] private string attackRangeParam = "IsInAttackRange";
    [SerializeField] private string noticedParam = "HasNoticed";

    private int detectedHash;
    private int attackRangeHash;
    private int noticedHash;
    private PoltergeistController controller;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        detectedHash = Animator.StringToHash(detectedParam);
        attackRangeHash = Animator.StringToHash(attackRangeParam);
        noticedHash = Animator.StringToHash(noticedParam);

        if (controller == null)
            controller = animator.GetComponent<PoltergeistController>();

        if (controller == null) return;

        if (stateInfo.IsName("Idle"))
        {
            controller.HasNoticed = false;
            animator.SetBool(noticedHash, false);
        }
        else if (stateInfo.IsName("NoticePlayer"))
        {
            controller.HasNoticed = true;
            animator.SetBool(noticedHash, true);
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (controller == null) return;

        animator.SetBool(detectedHash, controller.IsPlayerDetected);
        animator.SetBool(attackRangeHash, controller.IsInAttackRange);

        animator.SetBool(noticedHash, controller.HasNoticed);
    }
}