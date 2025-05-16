using UnityEngine;

public class HitBehaviour : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);

        Sounds.Instance.PlaySound(1, "Hit");
    }
}