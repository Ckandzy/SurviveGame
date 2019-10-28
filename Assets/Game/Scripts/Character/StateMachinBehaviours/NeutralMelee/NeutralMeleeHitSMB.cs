using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamekit2D;

public class NeutralMeleeHitSMB : SceneLinkedSMB<NeutralMeleeBase>
{
    public override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_MonoBehaviour.SetHorizontalSpeed(0);
    }
}