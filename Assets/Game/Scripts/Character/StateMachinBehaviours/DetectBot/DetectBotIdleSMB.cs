using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamekit2D;

public class DetectBotIdleSMB : SceneLinkedSMB<RangeEnemyBase>
{
    public override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_MonoBehaviour.SetMoveVector(Vector2.zero);
        //m_MonoBehaviour.engineAudio.PlayRandomSound();
    }

    public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_MonoBehaviour.ScanForPlayer();
    }

    public override void OnSLStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //m_MonoBehaviour.engineAudio.Stop();
    }
}