using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamekit2D;

public class MonsterMeleeDeathSMB : SceneLinkedSMB<MeleeEnemyBase>
{
    public override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_MonoBehaviour.DisableDamage();
    }

    public override void OnSLStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_MonoBehaviour.gameObject.SetActive(false);
    }
}