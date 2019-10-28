using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamekit2D;

public class NeutralMeleePatrolSMB : SceneLinkedSMB<NeutralMeleeBase>
{
    public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //We do this explicitly here instead of in the enemy class, that allow to handle obstacle differently according to state
        // (e.g. look at the ChomperRunToTargetSMB that stop the pursuit if there is an obstacle) 
        float dist = m_MonoBehaviour.speed;
        if (m_MonoBehaviour.CheckForObstacle(dist * 0.5f))
        {
            m_MonoBehaviour.SetHorizontalSpeed(-dist);
        }
        else
        {
            m_MonoBehaviour.SetHorizontalSpeed(dist);
        }
        m_MonoBehaviour.UpdateFacing();
        //m_MonoBehaviour.ScanForPlayer();
    }
}