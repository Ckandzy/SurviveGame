using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamekit2D;

public class DetectBot : RangeEnemyBase
{
    [Header("DetectBot")]
    public float RangeAngle = 60f;
    public int bulletNum = 3;
    public override void DoShoot()
    {
        Vector2 shootPosition = shootingOrigin.transform.localPosition;
        RaycastHit2D[] m_HitBuffer = new RaycastHit2D[1];
        Projectile.ProjectData[] projectData = new Projectile.ProjectData[bulletNum];
        Vector2 vector = m_SpriteForward;
        for (int i = 0; i < projectData.Length; i++)
        {
            projectData[i] = new Projectile.ProjectData
            {
                direction = projectData.Length > 1 ? vector.Rotate(RangeAngle * 0.5f).Rotate(-i * RangeAngle / (bulletNum - 1)) : m_SpriteForward,
                gravity = Vector2.zero,
                shootOrigin = shootingOrigin.transform.position,
                shootSpeed = launchSpeed
            };
            if (track)
            {
                projectData[i].Track = true;
                projectData[i].Target = m_Target;
                projectData[i].trackSensitivity = trackSensitivity;
            }
            else
            {
                projectData[i].Track = false;
                projectData[i].Target = null;
                if (locate)
                {
                    projectData[i].direction = (Target.transform.position - shootingOrigin.position).normalized;
                }
            }
            ProjectileObject obj = m_ProjectilePool.Pop(projectData[i]);
        }
        shootingAudio.PlayRandomSound();
    }
}
