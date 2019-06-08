using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamekit2D;

public class MonsterRangeAlertSMB : SceneLinkedSMB<RangeEnemyBase>
{
    public override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_MonoBehaviour.SetMoveVector(Vector2.zero);
    }

    public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_MonoBehaviour.CheckTargetStillVisible();
        m_MonoBehaviour.OrientToTarget();
        m_MonoBehaviour.CheckShoot();
        //m_MonoBehaviour.RememberTargetPos();
    }

    public override void OnSLStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }
}