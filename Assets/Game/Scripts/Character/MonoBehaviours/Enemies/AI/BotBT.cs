using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamekit2D;
using BTAI;

public class BotBT : MonoBehaviour
#if UNITY_EDITOR
    , BTAI.IBTDebugable
#endif
{
    Animator m_Animator;
    Damageable m_Damageable;
    Root m_Ai = BT.Root();
    SupportBot m_EnemyBehaviour;

    private void OnEnable()
    {
        m_EnemyBehaviour = GetComponent<SupportBot>();
        m_Animator = GetComponent<Animator>();

        m_Ai.OpenBranch(
            //BT.If(() => { return m_EnemyBehaviour.Target != null; }).OpenBranch(
            //    BT.Call(m_EnemyBehaviour.CheckTargetStillVisible),
            //    BT.Call(m_EnemyBehaviour.OrientToTarget),
            //    BT.If(() => { return m_EnemyBehaviour.CheckLaunchMode() == true; }).OpenBranch(
            //        BT.Call(() => { m_EnemyBehaviour.StartLaunch(true); })
            //    ),
            //    BT.If(() => { return m_EnemyBehaviour.CheckLaunchMode() == false; }).OpenBranch(
            //        BT.Call(() => { m_EnemyBehaviour.StartLaunch(false); })
            //    ),
            //    BT.Call(m_EnemyBehaviour.RememberTargetPos)
            //),
            BT.If(() => { return m_EnemyBehaviour.Target == null; }).OpenBranch(
                BT.RandomSequence().OpenBranch(
                    BT.Sequence().OpenBranch(
                       BT.SetBool(m_Animator, "Patrol", true),
                       BT.Call(() => m_EnemyBehaviour.SetFacingData(Random.Range(0f, 1f) > 0.5f ? -1 : 1))
                    ),
                    BT.SetBool(m_Animator, "Patrol", false)
                ),
                BT.Wait(Random.Range(3f, 5f))
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
