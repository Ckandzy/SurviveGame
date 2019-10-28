using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamekit2D;
using BTAI;

public class NeutralMeleeLivingBT : MonoBehaviour
#if UNITY_EDITOR
    , BTAI.IBTDebugable
#endif
{
    [Header("Setting")]
    //活跃度越高，越倾向于巡逻，而不是静止
    [Range(0, 1)]
    public float activityDegree = 0f;

    Animator m_Animator;
    Damageable m_Damageable;
    Root m_Ai = BT.Root();
    NeutralMeleeBase m_NeutralMeleeBase;

    private void OnEnable()
    {
        m_NeutralMeleeBase = GetComponent<NeutralMeleeBase>();
        m_Animator = GetComponent<Animator>();

        m_Ai.OpenBranch(   
            BT.If(() => { return m_Animator.GetCurrentAnimatorStateInfo(0).shortNameHash == Animator.StringToHash("Idle") && m_NeutralMeleeBase.Target == null; }).OpenBranch(
                BT.If(()=> { return Random.Range(0f, 1f) < activityDegree; }).OpenBranch(
                    BT.Sequence().OpenBranch(
                       BT.SetBool(m_Animator, "Patrol", true),
                       BT.Call(() => m_NeutralMeleeBase.SetFacingData(Random.Range(0f, 1f) > 0.5f ? -1 : 1))
                    )
                ),
                BT.Wait(Random.Range(3f, 5f))
            ),
            BT.If(() => { return m_Animator.GetCurrentAnimatorStateInfo(0).shortNameHash == Animator.StringToHash("Patrol") && m_NeutralMeleeBase.Target == null; }).OpenBranch(
                BT.If(() => { return Random.Range(0f, 1f) < activityDegree; }).OpenBranch(
                    BT.Sequence().OpenBranch(
                       BT.SetBool(m_Animator, "Patrol", false),
                       BT.Call(() => m_NeutralMeleeBase.SetFacingData(Random.Range(0f, 1f) > 0.5f ? -1 : 1))
                    )
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
