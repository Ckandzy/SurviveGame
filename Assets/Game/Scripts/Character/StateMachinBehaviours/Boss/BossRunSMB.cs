﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit2D
{
    public class BossRunSMB : SceneLinkedSMB<BossBehaviour>
    {
        public override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {

        }

        public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            float dist = m_MonoBehaviour.speed * 2;
            if (m_MonoBehaviour.CheckForObstacle(dist * 0.5f))
            {
                m_MonoBehaviour.SetHorizontalSpeed(-dist);
            }
            else
            {
                m_MonoBehaviour.SetHorizontalSpeed(dist);
            }
            m_MonoBehaviour.UpdateFacing();
        }

        public override void OnSLStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_MonoBehaviour.SetMoveVector(Vector2.zero);
        }
    }
}