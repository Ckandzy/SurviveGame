using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamekit2D;

public class NeutralMeleeAttackSMB : SceneLinkedSMB<NeutralMeleeBase>
{
    public override void OnSLStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnSLStateExit(animator, stateInfo, layerIndex);

        m_MonoBehaviour.SetHorizontalSpeed(0);
    }
}