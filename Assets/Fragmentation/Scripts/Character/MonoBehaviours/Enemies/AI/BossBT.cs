using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamekit2D;
using BTAI;

public class BossBT : MonoBehaviour
#if UNITY_EDITOR
    , BTAI.IBTDebugable
#endif
{
    Animator m_Animator;
    Damageable m_Damageable;
    Root m_Ai = BT.Root();
    BossBehaviour m_BossBehaviour;

    private void OnEnable()
    {
        m_BossBehaviour = GetComponent<BossBehaviour>();
        m_Animator = GetComponent<Animator>();

        m_Ai.OpenBranch(
            BT.RandomSequence().OpenBranch(
                BT.Root().OpenBranch(
                    BT.Trigger(m_Animator, "Walk 1"),
                    BT.Wait(0.2f),
                    BT.WaitForAnimatorState(m_Animator, "Idle 1")
                )
            )
        );
    }

    private void Update()
    {
        m_Ai.Tick();
    }
#if UNITY_EDITOR
    public BTAI.Root GetAIRoot()
    {
        return m_Ai;
    }
#endif
}
