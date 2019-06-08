using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamekit2D;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(CharacterController2D))]
[RequireComponent(typeof(Collider2D))]
public class RangeEnemyBase : MonoBehaviour
{
    static Collider2D[] s_ColliderCache = new Collider2D[16];

    public Vector3 moveVector { get { return m_MoveVector; } }
    public Transform Target { get { return m_Target; } }

    [Tooltip("If the sprite face left on the spritesheet, enable this. Otherwise, leave disabled")]
    public bool spriteFaceLeft = false;
    public bool shootOriginLeft = false;

    [Header("Movement")]
    public float speed;
    public float gravity = 10.0f;

    [Header("References")]
    [Tooltip("If the enemy will be using ranged attack, set a prefab of the projectile it should use")]
    public Projectile projectilePrefab;

    [Header("SightType Cone")]
    [Tooltip("The angle of the forward of the view cone. 0 is forward of the sprite, 90 is up, 180 behind etc.")]
    [Range(0.0f, 360.0f)]
    public float viewDirection = 0.0f;
    [Range(0.0f, 360.0f)]
    public float viewFov;
    public float viewDistance;
    [Tooltip("Time in seconds without the target in the view cone before the target is considered lost from sight")]
    public float timeBeforeTargetLost = 3.0f;

    [Header("Range Attack Data")]
    [Tooltip("From where the projectile are spawned")]
    public Transform shootingOrigin;
    [Tooltip("Which layer will be checked for track")]
    public LayerMask trackLayerMask;
    public float launchSpeed = 20f;
    public bool destroyWhenOutOfView = true;
    [Tooltip("Whether the bullet always tracks the target")]
    public bool track = false;
    public float trackTime = 5f;
    [Tooltip("Whether the bullet is locating the target at starting")]
    public bool locate = false;
    public float trackSensitivity = 0.1f;
    [Tooltip("Starting shoot angle when \"locate\" is false")]
    public float shootAngle = 45.0f;
    //public float shootForce = 100.0f;
    public float shootRate = 2.0f;

    [Header("Audio")]
    public RandomAudioPlayer shootingAudio;
    //public RandomAudioPlayer meleeAttackAudio;
    public RandomAudioPlayer dieAudio;
    public RandomAudioPlayer footStepAudio;
    public ContinuousAudioPlayer engineAudio;
    public ContinuousAudioPlayer moveAudio;

    [Header("Misc")]
    [Tooltip("Time in seconds during which the enemy flicker after being hit")]
    public float flickeringDuration;

    [Header("Debug")]
    protected SpriteRenderer m_SpriteRenderer;
    protected CharacterController2D m_CharacterController2D;
    protected Collider2D m_Collider;
    protected Animator m_Animator;

    protected Vector3 m_MoveVector;
    [SerializeField]
    protected Transform m_Target;
    protected Vector3 m_TargetShootPosition;
    protected float m_TimeSinceLastTargetView;

    [SerializeField]
    protected float m_FireTimer = 0.0f;

    //as we flip the sprite instead of rotating/scaling the object, this give the forward vector according to the sprite orientation
    /*protected*/
    public Vector2 m_SpriteForward;
    protected Bounds m_LocalBounds;
    protected Vector3 m_LocalDamagerPosition;

    protected RaycastHit2D[] m_RaycastHitCache = new RaycastHit2D[8];
    protected ContactFilter2D m_Filter;

    protected Coroutine m_FlickeringCoroutine = null;
    protected Color m_OriginalColor;

    //protected BulletPool m_BulletPool;
    protected ProjectilePool m_ProjectilePool;
    protected BoxCollider2D boxCollider2D;

    protected ContactFilter2D m_ContactFilter;

    protected bool m_Dead = false;

    protected readonly int m_HashAlertPara = Animator.StringToHash("Alert");
    protected readonly int m_HashPatrolPara = Animator.StringToHash("Patrol");
    protected readonly int m_HashShootPara = Animator.StringToHash("Shoot");
    protected readonly int m_HashMeleeAttackPara = Animator.StringToHash("MeleeAttack");
    protected readonly int m_HashHitPara = Animator.StringToHash("Hit");
    protected readonly int m_HashDeathPara = Animator.StringToHash("Death");
    protected readonly int m_HashGroundedPara = Animator.StringToHash("Grounded");

    protected virtual void Awake()
    {
        m_CharacterController2D = GetComponent<CharacterController2D>();
        m_Collider = GetComponent<Collider2D>();
        m_Animator = GetComponent<Animator>();
        m_SpriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider2D = GetComponent<BoxCollider2D>();

        m_OriginalColor = m_SpriteRenderer.color;

        m_ContactFilter.layerMask = trackLayerMask;
        m_ContactFilter.useLayerMask = true;
        m_ContactFilter.useTriggers = false;

        if (projectilePrefab != null)
            m_ProjectilePool = ProjectilePool.GetObjectPool(projectilePrefab.gameObject, 8);

        m_SpriteForward = spriteFaceLeft ? Vector2.left : Vector2.right;
        if (m_SpriteRenderer.flipX) m_SpriteForward = -m_SpriteForward;
    }

    private void OnEnable()
    {
        m_Dead = false;
        m_Collider.enabled = true;
    }

    protected virtual void Start()
    {
        SceneLinkedSMB<RangeEnemyBase>.Initialise(m_Animator, this);

        m_LocalBounds = new Bounds();
        int count = m_CharacterController2D.Rigidbody2D.GetAttachedColliders(s_ColliderCache);
        for (int i = 0; i < count; ++i)
        {
            m_LocalBounds.Encapsulate(transform.InverseTransformBounds(s_ColliderCache[i].bounds));
        }
        m_Filter = new ContactFilter2D();
        m_Filter.layerMask = m_CharacterController2D.groundedLayerMask;
        m_Filter.useLayerMask = true;
        m_Filter.useTriggers = false;
    }

    void FixedUpdate()
    {
        if (m_Dead)
            return;

        m_MoveVector.y = Mathf.Max(m_MoveVector.y - gravity * Time.deltaTime, -gravity);

        m_CharacterController2D.Move(m_MoveVector * Time.deltaTime);

        m_CharacterController2D.CheckCapsuleEndCollisions();

        UpdateTimers();

        m_Animator.SetBool(m_HashGroundedPara, m_CharacterController2D.IsGrounded);

    }

    void UpdateTimers()
    {
        if (m_TimeSinceLastTargetView > 0.0f && m_Target == null)
            m_TimeSinceLastTargetView -= Time.deltaTime;

        if (m_FireTimer > 0.0f)
            m_FireTimer -= Time.deltaTime;
    }

    public void SetHorizontalSpeed(float horizontalSpeed)
    {
        m_MoveVector.x = horizontalSpeed * m_SpriteForward.x;
    }

    public bool CheckForObstacle(float forwardDistance)
    {
        //we circle cast with a size slightly small than the collider height. That avoid to collide with very small bump on the ground
        if (Physics2D.CircleCast(m_Collider.bounds.center, m_Collider.bounds.extents.y - 0.2f, m_SpriteForward, forwardDistance, m_Filter.layerMask.value))
        {
            return true;
        }

        Vector3 castingPosition = (Vector2)(transform.position + m_LocalBounds.center) + m_SpriteForward * m_LocalBounds.extents.x;
        Debug.DrawLine(castingPosition, castingPosition + Vector3.down * (m_LocalBounds.extents.y + 0.2f));

        if (!Physics2D.CircleCast(castingPosition, 0.1f, Vector2.down, m_LocalBounds.extents.y + 0.2f, m_CharacterController2D.groundedLayerMask.value))
        {
            return true;
        }

        return false;
    }

    public void SetFacingData(int facing)
    {
        Vector3 vector = shootingOrigin.transform.localPosition;
        float absX = Mathf.Abs(vector.x);
        if (facing == -1)
        {
            m_SpriteRenderer.flipX = !spriteFaceLeft;
            m_SpriteForward = Vector2.left;
            vector.x = shootOriginLeft ^ spriteFaceLeft ? absX : -absX;
            shootingOrigin.transform.localPosition = vector;
        }
        else if (facing == 1)
        {
            m_SpriteRenderer.flipX = spriteFaceLeft;
            m_SpriteForward = Vector2.right;
            vector.x = shootOriginLeft ^ spriteFaceLeft ? -absX : absX;
            shootingOrigin.transform.localPosition = vector;
        }
    }

    public void SetMoveVector(Vector2 newMoveVector)
    {
        m_MoveVector = newMoveVector;
    }

    public void UpdateFacing()
    {
        bool faceLeft = m_MoveVector.x < 0f;
        bool faceRight = m_MoveVector.x > 0f;

        if (faceLeft)
        {
            SetFacingData(-1);
        }
        else if (faceRight)
        {
            SetFacingData(1);
        }
    }

    public void ScanForPlayer()
    {
        if (DetectTarget())
        {
            m_Target = PlayerCharacter.PlayerInstance.transform;
            m_TimeSinceLastTargetView = timeBeforeTargetLost;

            m_Animator.SetBool(m_HashAlertPara, true);
            m_Animator.SetBool(m_HashPatrolPara, false);
        }
        else
        {
            m_Animator.SetBool(m_HashAlertPara, false);
        }
    }

    public void OrientToTarget()
    {
        if (m_Target == null)
            return;

        Vector3 toTarget = m_Target.position - transform.position;

        //Debug.Log(toTarget + " , " + m_SpriteForward + " , " + Vector2.Dot(toTarget, m_SpriteForward));
        if (Vector2.Dot(toTarget, m_SpriteForward) < 0)
        {
            SetFacingData(Mathf.RoundToInt(-m_SpriteForward.x));
        }
    }

    public void CheckTargetStillVisible()
    {
        if (m_Target == null || !DetectTarget(false))
        {
            m_Animator.SetBool(m_HashAlertPara, false);
            m_Target = null;
            return;
        }

        if (m_TimeSinceLastTargetView <= 0.0f)
        {
            ForgetTarget();
        }
    }

    public bool DetectTarget(bool checkPlayerInput = true)
    {
        if (checkPlayerInput)
        {
            //If the player don't have control, they can't react, so do not pursue them
            if (!PlayerInput.Instance.HaveControl)
                return false;
        }

        Vector3 dir = PlayerCharacter.PlayerInstance.transform.position - transform.position;

        if (dir.sqrMagnitude > viewDistance * viewDistance)
        {
            return false;
        }
        //注：当viewDirection为0时, -viewDirection和viewDirection仍然有效, 在计算机有符号整数和浮点数中, +0和-0是两种不同的表示。
        Vector3 testForward = Quaternion.Euler(0, 0, spriteFaceLeft ? Mathf.Sign(m_SpriteForward.x) * -viewDirection : Mathf.Sign(m_SpriteForward.x) * viewDirection) * m_SpriteForward;

        float angle = Vector3.Angle(testForward, dir);

        if (angle > viewFov * 0.5f)
        {
            return false;
        }

        return true;
    }

    public void ForgetTarget()
    {
        m_Animator.SetBool(m_HashAlertPara, false);
        m_Target = null;
    }

    //This is used in case where there is a delay between deciding to shoot and shoot (e.g. Spitter that have an animation before spitting)
    //so we shoot where the target was when the animation started, not where it is when the actual shooting happen
    public void RememberTargetPos()
    {
        if (m_Target == null)
            return;

        m_TargetShootPosition = m_Target.transform.position;
    }

    //This is call each update if the enemy is in a attack/shooting state, but the timer will early exit if too early to shoot.
    protected bool CheckShootingTimer()
    {
        if (m_FireTimer > 0.0f)
        {
            return false;
        }

        if (m_Target == null)
        {//we lost the target, shouldn't shoot
            return false;
        }
        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>true for track</returns>
    public bool CheckShootMode()
    {
        Vector2 shootPosition = shootingOrigin.transform.position;
        RaycastHit2D[] m_HitBuffer = new RaycastHit2D[1];
        bool flag = Physics2D.Raycast(shootingOrigin.position, m_SpriteForward, m_ContactFilter, m_HitBuffer, viewDistance) > 0 && m_Target != null;
        return !flag;
    }

    public virtual void CheckShoot()
    {
        if (CheckShootingTimer())
        {
            m_Animator.SetTrigger(m_HashShootPara);
            m_FireTimer = shootRate;
        }
    }

    #region Call By Event
    public virtual void DoShoot()
    {
        Vector2 shootPosition = shootingOrigin.transform.localPosition;
        RaycastHit2D[] m_HitBuffer = new RaycastHit2D[1];
        Projectile.ProjectData projectData = new Projectile.ProjectData()
        {
            direction = m_SpriteForward,
            gravity = Vector2.zero,
            shootOrigin = shootingOrigin.position,
            shootSpeed = launchSpeed,
            destroyWhenOutOfView = destroyWhenOutOfView
        };
        if (track)
        {
            projectData.Track = true;
            projectData.direction = new Vector2(m_SpriteForward.x * Mathf.Cos(Mathf.Deg2Rad * shootAngle), Mathf.Sin(Mathf.Deg2Rad * shootAngle));
            projectData.Target = m_Target;
            projectData.trackSensitivity = trackSensitivity;
            projectData.timeBeforeAutodestruct = trackTime;
        }
        else
        {
            projectData.Track = false;
            projectData.Target = null;
            if (locate)
            {
                projectData.direction = (Target.transform.position - shootingOrigin.position).normalized;
            }
        }
        ProjectileObject obj = m_ProjectilePool.Pop(projectData);
        shootingAudio.PlayRandomSound();
    }

    public void Die(Damager damager, Damageable damageable)
    {
        //Vector2 throwVector = new Vector2(0, 2.0f);
        //Vector2 damagerToThis = damager.transform.position - transform.position;

        //throwVector.x = Mathf.Sign(damagerToThis.x) * -4.0f;
        //SetMoveVector(throwVector);

        m_Animator.SetTrigger(m_HashDeathPara);

        dieAudio.PlayRandomSound();

        m_Dead = true;
        //Debug.Log(moveVector);
        //m_Collider.enabled = false;
        gameObject.layer = 20;
        CameraShaker.Shake(0.15f, 0.3f);
    }

    public void Hit(Damager damager, Damageable damageable)
    {
        if (damageable.CurrentHealth <= 0)
            return;

        m_Animator.SetTrigger(m_HashHitPara);

        //Vector2 throwVector = new Vector2(0, 3.0f);
        //Vector2 damagerToThis = damager.transform.position - transform.position;

        //throwVector.x = Mathf.Sign(damagerToThis.x) * -2.0f;
        //m_MoveVector = throwVector;

        if (m_FlickeringCoroutine != null)
        {
            StopCoroutine(m_FlickeringCoroutine);
            m_SpriteRenderer.color = m_OriginalColor;
        }

        m_FlickeringCoroutine = StartCoroutine(Flicker(damageable));
        CameraShaker.Shake(0.15f, 0.3f);
    }
    #endregion

    protected IEnumerator Flicker(Damageable damageable)
    {
        float timer = 0f;
        float sinceLastChange = 0.0f;

        Color transparent = m_OriginalColor;
        transparent.a = 0.2f;
        int state = 1;

        m_SpriteRenderer.color = transparent;

        while (timer < damageable.invulnerabilityDuration)
        {
            yield return null;
            timer += Time.deltaTime;
            sinceLastChange += Time.deltaTime;
            if (sinceLastChange > flickeringDuration)
            {
                sinceLastChange -= flickeringDuration;
                state = 1 - state;
                m_SpriteRenderer.color = state == 1 ? transparent : m_OriginalColor;
            }
        }

        m_SpriteRenderer.color = m_OriginalColor;
    }

    public void PlayFootStep()
    {
        footStepAudio.PlayRandomSound();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        //draw the cone of view
        Vector3 forward = spriteFaceLeft ? Vector2.left : Vector2.right;
        forward = Quaternion.Euler(0, 0, spriteFaceLeft ? -viewDirection : viewDirection) * forward;

        if (GetComponent<SpriteRenderer>().flipX) forward.x = -forward.x;

        Vector3 endpoint = transform.position + (Quaternion.Euler(0, 0, viewFov * 0.5f) * forward);

        Handles.color = new Color(0, 1.0f, 0, 0.2f);
        Handles.DrawSolidArc(transform.position, -Vector3.forward, (endpoint - transform.position).normalized, viewFov, viewDistance);
        Handles.color = new Color(1.0f, 0, 0, 0.1f);
    }
#endif
}