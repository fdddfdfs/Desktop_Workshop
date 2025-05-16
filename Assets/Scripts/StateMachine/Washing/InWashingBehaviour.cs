using System;
using UnityEngine;

public class InWashingBehaviour : StateMachineBehaviour
{
    public event Action OnExit;
    
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
        
        OnExit?.Invoke();
    }
}