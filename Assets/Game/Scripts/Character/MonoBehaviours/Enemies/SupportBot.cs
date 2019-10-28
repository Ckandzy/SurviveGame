using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamekit2D;

public class SupportBot : RangeEnemyBase
{
    protected readonly int m_HashLaunchPara = Animator.StringToHash("Launch");
    protected readonly int m_HashLaunch2Para = Animator.StringToHash("Launch2");
    protected override void Start()
    {
        base.Start();
        SceneLinkedSMB<SupportBot>.Initialise(m_Animator, this);
    }

    public override void CheckShoot()
    {
        if (CheckShootingTimer())
        {
            if (CheckShootMode())
            {
                SetShootMode(true);
                m_Animator.SetTrigger(m_HashLaunchPara);
            }
            else
            {
                SetShootMode(false);
                m_Animator.SetTrigger(m_HashLaunch2Para);
            }             
            m_FireTimer = shootRate;
        }
    }

    public void SetShootMode(bool isTrack)
    {
        if (isTrack)
            track = true;
        else
            track = false;
    }
}
