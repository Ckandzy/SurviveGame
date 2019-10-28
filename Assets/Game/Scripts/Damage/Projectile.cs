using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamekit2D;

[RequireComponent(typeof(Rigidbody2D))]
//[RequireComponent(typeof(TakeDamager))]
public class Projectile : MonoBehaviour
{
    [System.Serializable]
    public class ProjectData
    {
        public Vector3 shootOrigin;
        public Vector2 gravity;
        public float shootSpeed;
        public Vector2 direction;
        public bool Track;
        public Transform Target;
        /// <summary>
        /// 灵敏度, 导弹追踪强度
        /// </summary>
        public float trackSensitivity;
        public bool destroyWhenOutOfView = true;
        public float timeBeforeAutodestruct = -1.0f;
    }
    #region Trajectory Mode
    [Header("Trajectory")]
    public ProjectData projectData;

    public Vector2 moveVector;
    #endregion

    protected Rigidbody2D m_Rigidbody2D;
    protected float m_Timer;   

    [HideInInspector]
    public ProjectileObject projectilePoolObject;
    [HideInInspector]
    public Camera mainCamera;

    public void ReturnToPool()
    {
        projectilePoolObject.ReturnToPool();
    }
    private void OnEnable()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        moveVector = projectData.shootSpeed * projectData.direction.normalized;
        mainCamera = Camera.main;
        m_Timer = 0f;
        //transform.rotation = Quaternion.FromToRotation(Vector3.right, projectData.direction);

        //Debug.Log(Vector3.SignedAngle(Vector3.right, projectData.direction, Vector3.back));
        transform.Rotate(0, 0, -Vector3.SignedAngle(Vector3.right, projectData.direction, Vector3.back));
    }

    void FixedUpdate()
    {
        m_Rigidbody2D.MovePosition(m_Rigidbody2D.position + moveVector * Time.deltaTime);
        if (projectData.Track)
        {
            Vector3 dir = Vector3.RotateTowards(
                moveVector.normalized, 
                (projectData.Target.position - transform.position).normalized, 
                projectData.trackSensitivity, 
                0f
             );
            
            moveVector = projectData.shootSpeed * dir;
            //Unity源代码:
            //public Vector3 right { get { return rotation * Vector3.right; } set { rotation = Quaternion.FromToRotation(Vector3.right, value); } }
            
            //重点[错误写法] :transform.rotation = Quaternion.FromToRotation(transform.right, dir);
            //正确写法[1] :transform.right = dir;
            //正确写法[2] :
            transform.rotation = Quaternion.FromToRotation(Vector3.right, dir);
        }

        if (projectData.destroyWhenOutOfView)
        {
            Vector3 screenPoint = mainCamera.WorldToViewportPoint(transform.position);
            bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 &&
                            screenPoint.x < 1 && screenPoint.y > 0 &&
                            screenPoint.y < 1;
            if (!onScreen)
                projectilePoolObject.ReturnToPool();
        }

        if (projectData.timeBeforeAutodestruct > 0)
        {
            m_Timer += Time.deltaTime;
            if (m_Timer > projectData.timeBeforeAutodestruct)
            {
                projectilePoolObject.ReturnToPool();
            }
        }
    }

    public void OnHitDamageable(Damager origin, Damageable damageable)
    {
        //FindSurface(origin.LastHit);
    }

    public void OnHitNonDamageable(Damager origin)
    {
        //FindSurface(origin.LastHit);
    }
}
