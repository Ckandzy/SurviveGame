using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BTAI;
using Gamekit2D;
using Fragmentation.Skill;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(CharacterController2D))]
[RequireComponent(typeof(Collider2D))]
public class BossBehaviour : MonoBehaviour
#if UNITY_EDITOR
    , BTAI.IBTDebugable
#endif
{
    static Collider2D[] s_ColliderCache = new Collider2D[16];

    public Vector3 moveVector { get { return m_MoveVector; } }
    public Transform target;

    [Tooltip("If the sprite face left on the spritesheet, enable this. Otherwise, leave disabled")]
    public bool spriteFaceLeft = false;
    //public bool shootOriginPos = false;

    [Header("Movement")]
    public float speed;
    public float gravity = 10.0f;

    [Header("References")]
    [Tooltip("If the enemy will be using ranged attack, set a prefab of the projectile it should use")]
    public Projectile[] projectilePrefab;

    [Header("SightType Cone")]
    [Tooltip("The angle of the forward of the view cone. 0 is forward of the sprite, 90 is up, 180 behind etc.")]
    [Range(0.0f, 360.0f)]
    public float viewDirection = 0.0f;
    [Range(0.0f, 360.0f)]
    public float viewFov;
    public float viewDistance;

    [Header("Range Attack Data")]
    [Tooltip("From where the projectile are spawned")]
    public Transform rangeAttackOriginGroup;
    public Transform shootOrigin;
    public Transform launchOrigin;
    [EnemyMeleeRangeCheck]
    public float missileRange = 3.0f;
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

    public bool shootStrafe = false;
    public float shootStrafeDuration = 0.0f;
    public int shootStrafeTimes = 0;
    protected float m_ShootStrafeTimer = 0.0f;

    public bool launchStrafe = false;
    public float launchStrafeDuration = 0.0f;
    public int launchStrafeTimes = 0;
    protected float m_LaunchStrafeTimer = 0.0f;

    [Header("Melee Attack Data")]
    [EnemyMeleeRangeCheck]
    public float meleeRange = 3.0f;
    public Damager meleeDamager;
    public Damager contactDamager;
    [Tooltip("if true, the enemy will jump/dash forward when it melee attack")]
    public bool attackDash;
    [Tooltip("The force used by the dash")]
    public Vector2 attackForce;

    [Header("Audio")]
    public RandomAudioPlayer strafeAudio;
    public RandomAudioPlayer launchAudio;
    public RandomAudioPlayer dieAudio;
    public RandomAudioPlayer footStepAudio;
    public RandomAudioPlayer meleeAttack1Audio;
    public RandomAudioPlayer meleeAttack2_1Audio;
    public RandomAudioPlayer meleeAttack2_2Audio;

    [Header("Misc")]
    [Tooltip("Time in seconds during which the enemy flicker after being hit")]
    public float flickeringDuration;

    [Header("AI")]
    public Fragmentation.Skill.SkillBase meleeAttack = new Fragmentation.Skill.SkillBase();
    public Fragmentation.Skill.SkillBase rangeAttack = new Fragmentation.Skill.SkillBase();
    public Fragmentation.Skill.SkillBase chase = new Fragmentation.Skill.SkillBase();
    /// <summary>
    /// 1状态下扫射次数
    /// </summary>
    public int stage1StrafeTimes = 2;
    /// <summary>
    /// 2状态下连击次数
    /// </summary>
    public int stage2MeleeTimes = 2;
    /// <summary>
    /// 扫射间隔
    /// </summary>
    public float strafeInterval = 1f;
    protected bool tick = false;

    [Header("Debug")]
    public bool enableSwitch = false;
    protected int end;
    public bool isTired = false;

    public BossAudio bossAudio;

    protected SpriteRenderer m_SpriteRenderer;
    protected CharacterController2D m_CharacterController2D;
    protected Collider2D m_Collider;
    protected Animator m_Animator;

    protected Vector3 m_MoveVector;
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
    protected ProjectilePool[] m_ProjectilePool;
    protected BoxCollider2D boxCollider2D;

    protected ContactFilter2D m_ContactFilter;

    protected bool m_Dead = false;
    protected bool m_Switch = false;
    protected bool m_Active = false;

    protected readonly int m_HashHitPara = Animator.StringToHash("Hit");
    protected readonly int m_HashDeathPara = Animator.StringToHash("Death");
    protected readonly int m_HashChasePara = Animator.StringToHash("Chase");
    protected readonly int m_HashGroundedPara = Animator.StringToHash("Grounded");
    protected readonly int m_HashWalk1Para = Animator.StringToHash("Walk 1");
    protected readonly int m_HashMeleeAttack1Para = Animator.StringToHash("MeleeAttack 1");
    protected readonly int m_HashRangeAttack1Para = Animator.StringToHash("RangeAttack 1");
    protected readonly int m_HashWalk2Para = Animator.StringToHash("Walk 2");
    protected readonly int m_HashMeleeAttack2Para = Animator.StringToHash("MeleeAttack 2");
    protected readonly int m_HashRangeAttack2Para = Animator.StringToHash("RangeAttack 2");
    protected readonly int m_HashSwitchPara = Animator.StringToHash("Switch");

    Root m_Ai = BT.Root();

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

        m_ProjectilePool = new ProjectilePool[projectilePrefab.Length];
        for (int i = 0; i < projectilePrefab.Length; i++)
        {
            m_ProjectilePool[i] = ProjectilePool.GetObjectPool(projectilePrefab[i].gameObject, 8);
        }

        m_SpriteForward = spriteFaceLeft ? Vector2.left : Vector2.right;
        if (m_SpriteRenderer.flipX) m_SpriteForward = -m_SpriteForward;
    }

    private void OnEnable()
    {
        m_Dead = false;
        m_Collider.enabled = true;

        m_Ai.OpenBranch(
            BT.Call(() =>
            {
                DetectTarget(false);
            }),
            BT.If(() => { return !isTired; }).OpenBranch(
                BT.Call(CheckChase),
                BT.RandomSequence(new int[] { 2, 6, 1 }).OpenBranch(
                    BT.Root().OpenBranch(
                        BT.SetBool(m_Animator, "Walk 1", true),
                        BT.Wait(Random.Range(1f, 2f)),
                        BT.SetBool(m_Animator, "Walk 1", false),
                        BT.WaitForAnimatorState(m_Animator, "Idle 1")
                    ),
                    BT.Root().OpenBranch(
                        BT.Call(CheckMeleeAttack)
                    ),
                    BT.Repeat(1).OpenBranch(
                        BT.Call(CheckRangeAttack),
                        BT.Wait(shootStrafeDuration)
                    )
                )
            ),
            BT.If(() => { return isTired; }).OpenBranch(
            )
        );

        tick = true;
    }

    protected virtual void Start()
    {
        SceneLinkedSMB<BossBehaviour>.Initialise(m_Animator, this);

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

    private void Update()
    {
        //if (!m_Switch && PlayerInput.Instance.Exit.Down)
        //{
        //    StartSwitch();
        //    m_Switch = true;
        //}
       
        //Test Tried
        if (isTired)
        {
            tick = false;
            m_Animator.SetBool("Tired", true);
        }
        else
        {
            tick = true;
            m_Animator.SetBool("Tired", false);
        }

        if (tick && !m_Dead)
            m_Ai.Tick();


        //Skill Timer;
        meleeAttack.Tick();
        rangeAttack.Tick();
        chase.Tick();
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
        if (m_TimeSinceLastTargetView > 0.0f && target == null)
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

    public void SetFacingData(int facing)
    {
        //Vector3 vector = shootingOrigin.transform.localPosition;
        Vector3 vector = rangeAttackOriginGroup.transform.localScale;
        if (facing == -1)
        {
            m_SpriteRenderer.flipX = !spriteFaceLeft;
            //vector.x = shootOriginLeft ^ spriteFaceLeft ? absX : -absX;
            m_SpriteForward = Vector2.left;
            vector.x = spriteFaceLeft ? 1 : -1;
            rangeAttackOriginGroup.transform.localScale = vector;
        }
        else if (facing == 1)
        {
            m_SpriteRenderer.flipX = spriteFaceLeft;
            m_SpriteForward = Vector2.right;
            vector.x = spriteFaceLeft ? -1 : 1;
            rangeAttackOriginGroup.transform.localScale = vector;
        }
    }

    public void OrientToTarget()
    {
        if (target == null)
            return;

        Vector3 toTarget = target.position - transform.position;

        if (Vector2.Dot(toTarget, m_SpriteForward) < 0)
        {
            SetFacingData(Mathf.RoundToInt(-m_SpriteForward.x));
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

        Debug.Log("dir.sqrMagnitude > viewDistance * viewDistance : " + (dir.sqrMagnitude > viewDistance * viewDistance));
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
        Debug.Log("DetectTarget");
        target = PlayerCharacter.PlayerInstance.transform;
        return true;
    }

    /// <summary>
    /// 当[目标]---处于视线范围内---处于导弹攻击范围外------追逐冷却完成---，开始追逐
    /// </summary>
    /// <returns></returns>
    public void CheckChase()
    {
        Debug.Log("Check Chase");
        Vector3 dir = PlayerCharacter.PlayerInstance.transform.position - transform.position;
        if (dir.sqrMagnitude > missileRange * missileRange)
        {
            if (target != null && DetectTarget(false) && chase.IsReady)
            {
                if (dir.sqrMagnitude > missileRange * missileRange && !chase.IsRunning)
                {
                    Debug.Log("Boss Chase");
                    m_Animator.SetBool(m_HashChasePara, true);
                    chase.StartSkill();
                }
            }
        }
        else
        {
            Debug.Log("Boss Stop Chase");
            chase.EndSkill();
            m_Animator.SetBool(m_HashChasePara, false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>true for track</returns>
    public bool CheckShootMode()
    {
        Vector2 shootPosition = shootOrigin.transform.position;
        RaycastHit2D[] m_HitBuffer = new RaycastHit2D[1];
        bool flag = Physics2D.Raycast(shootOrigin.position, m_SpriteForward, m_ContactFilter, m_HitBuffer, viewDistance) > 0 && target != null;
        return !flag;
    }

    #region Call By Event
    public virtual void DoShoot()
    {
        Vector2 shootPosition = shootOrigin.transform.localPosition;
        RaycastHit2D[] m_HitBuffer = new RaycastHit2D[1];
        Projectile.ProjectData projectData = new Projectile.ProjectData()
        {
            direction = m_SpriteForward,
            gravity = Vector2.zero,
            shootOrigin = shootOrigin.position,
            shootSpeed = launchSpeed,
            destroyWhenOutOfView = destroyWhenOutOfView,
            //----------//
            timeBeforeAutodestruct = 4f
        };
        //if (track)
        //{
        //    projectData.Track = true;
        //    projectData.direction = new Vector2(m_SpriteForward.x * Mathf.Cos(Mathf.Deg2Rad * shootAngle), Mathf.Sin(Mathf.Deg2Rad * shootAngle));
        //    projectData.Target = target;
        //    projectData.trackSensitivity = trackSensitivity;
        //    projectData.timeBeforeAutodestruct = trackTime;
        //}
        //else
        //{
        projectData.Track = false;
        projectData.Target = null;
        if (locate)
        {
            projectData.direction = (target.transform.position - shootOrigin.position).normalized;
        }
        //}
        if (!shootStrafe)
            m_ProjectilePool[0].Pop(projectData);
        else
            StartCoroutine(Strafe(projectData, 0));
        strafeAudio.PlayRandomSound();
    }

    public virtual void DoLaunch()
    {
        Vector2 shootPosition = shootOrigin.transform.localPosition;
        RaycastHit2D[] m_HitBuffer = new RaycastHit2D[1];
        Projectile.ProjectData projectData = new Projectile.ProjectData()
        {
            direction = m_SpriteForward,
            gravity = Vector2.zero,
            shootOrigin = launchOrigin.position,
            shootSpeed = launchSpeed,
            destroyWhenOutOfView = destroyWhenOutOfView
        };
        if (track)
        {
            projectData.Track = true;
            projectData.direction = new Vector2(m_SpriteForward.x * Mathf.Cos(Mathf.Deg2Rad * shootAngle), Mathf.Sin(Mathf.Deg2Rad * shootAngle));
            projectData.Target = target;
            projectData.trackSensitivity = trackSensitivity;
            projectData.timeBeforeAutodestruct = trackTime;
        }
        else
        {
            projectData.Track = false;
            projectData.Target = null;
            if (locate)
            {
                projectData.direction = (target.transform.position - launchOrigin.position).normalized;
            }
        }
        if (!launchStrafe)
        {
            m_ProjectilePool[1].Pop(projectData);
            launchAudio.PlayRandomSound();
        }
        else
            StartCoroutine(Strafe(projectData, 1));
    }
    protected virtual IEnumerator Strafe(Projectile.ProjectData projectData, int strafeMode)
    {
        if (strafeMode == 0)
        {
            while (m_ShootStrafeTimer < shootStrafeDuration)
            {
                m_ProjectilePool[strafeMode].Pop(projectData);
                m_ShootStrafeTimer += shootStrafeDuration / shootStrafeTimes;
                yield return new WaitForSeconds(shootStrafeDuration / shootStrafeTimes);
            }
            m_ShootStrafeTimer = 0.0f;
            yield break;
        }
        else
        {
            while (m_LaunchStrafeTimer < launchStrafeDuration)
            {
                launchAudio.PlayRandomSound();
                m_ProjectilePool[strafeMode].Pop(projectData);
                m_LaunchStrafeTimer += launchStrafeDuration / launchStrafeTimes;
                yield return new WaitForSeconds(launchStrafeDuration / launchStrafeTimes);
            }
            m_LaunchStrafeTimer = 0.0f;
            yield break;
        }
    }

    public void SetEnd(int i = 0)
    {
        end = i;
    }

    public void Die(Damager damager, Damageable damageable)
    {
        m_Animator.SetTrigger(m_HashDeathPara);
        dieAudio.PlayRandomSound();

        m_Dead = true;
        //Debug.Log(moveVector);
        //m_Collider.enabled = false;
        //gameObject.layer = 20;
        //CameraShaker.Shake(0.15f, 0.3f);
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
    public void CheckMeleeAttack()
    {
        if (target != null && (target.transform.position - transform.position).sqrMagnitude < (meleeRange * meleeRange) && meleeAttack.IsReady)
        {
            if (m_Switch)
            {
                m_Animator.SetTrigger(m_HashMeleeAttack2Para);
            }
            else
            {
                m_Animator.SetTrigger(m_HashMeleeAttack1Para);
            }
            meleeAttack.StartSkill();
        }
    }

    public void CheckRangeAttack()
    {
        if (target != null && (target.transform.position - transform.position).sqrMagnitude < (missileRange * missileRange) && rangeAttack.IsReady)
        {
            if (m_Switch)
            {
                m_Animator.SetTrigger(m_HashRangeAttack1Para);
            }
            else
            {
                m_Animator.SetTrigger(m_HashRangeAttack1Para);
            }
            OrientToTarget();
            rangeAttack.StartSkill();
        }
    }
    public void StartAttack()
    {
        if (m_SpriteRenderer.flipX)
            meleeDamager.transform.localPosition = Vector3.Scale(m_LocalDamagerPosition, new Vector3(-1, 1, 1));
        else
            meleeDamager.transform.localPosition = m_LocalDamagerPosition;

        meleeDamager.EnableDamage();
        meleeDamager.gameObject.SetActive(true);

        if (attackDash)
            m_MoveVector = new Vector2(m_SpriteForward.x * attackForce.x, attackForce.y);
    }
    public void EndAttack()
    {
        if (meleeDamager != null)
        {
            meleeDamager.gameObject.SetActive(false);
            meleeDamager.DisableDamage();
        }
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

    public void StartSwitch()
    {
        StartCoroutine(Switch());
    }

    public IEnumerator Switch()
    {
        tick = false;
        bossAudio.PlayAudio(end, 2);
        m_Animator.ResetTrigger(m_HashMeleeAttack1Para);
        m_Animator.ResetTrigger(m_HashRangeAttack1Para);
        m_Animator.ResetTrigger(m_HashWalk1Para);
        m_Animator.SetTrigger(m_HashSwitchPara);
        yield return new WaitForAnimatorState(m_Animator, "Idle 2");
        m_Ai = BT.Root();
        m_Ai.OpenBranch(
            BT.RandomSequence(new int[] { 2, 15, 6, 6 }).OpenBranch(
                BT.Root().OpenBranch(
                    BT.Log("mmp1-1"),
                    BT.SetBool(m_Animator, "Walk 2", true),
                    BT.Wait(Random.Range(1f, 2f)),
                    BT.SetBool(m_Animator, "Walk 2", false),
                    BT.Log("mmp1-2"),
                    BT.WaitForAnimatorState(m_Animator, "Idle 2")
                ),
                BT.Root().OpenBranch(
                    BT.Repeat(stage2MeleeTimes).OpenBranch(
                        //BT.Call(OrientToTarget),
                        BT.Log("mmp3-1"),
                        BT.Trigger(m_Animator, "Launch"),
                        BT.Wait(0.1f),
                        BT.Log("mmp3-2"),
                        BT.WaitForAnimatorState(m_Animator, "Idle 2")
                    )
                ),
                BT.Repeat(stage1StrafeTimes).OpenBranch(
                    BT.Log("mmp4-1"),
                    BT.Call(OrientToTarget),
                    BT.Trigger(m_Animator, "RangeAttack 2"),
                    BT.Wait(0.1f),
                    BT.Log("mmp4-2"),
                    BT.WaitForAnimatorState(m_Animator, "Idle 2"),
                    BT.Wait(strafeInterval)
                )
            )
        );
        tick = true;
    }

    #region Audio
    public void PlayMeleeSound()
    {
        meleeAttack1Audio.PlayRandomSound();
    }
    public void PlayMelee2_1Sound()
    {
        meleeAttack2_1Audio.PlayRandomSound();
    }
    public void PlayMelee2_2Sound()
    {
        meleeAttack2_2Audio.PlayRandomSound();
    }
    public void PlayFootStepSound()
    {
        footStepAudio.PlayRandomSound();
    }
    #endregion
#if UNITY_EDITOR
    public BTAI.Root GetAIRoot()
    {
        return m_Ai;
    }
    private void OnDrawGizmosSelected()
    {
        //draw the cone of view
        Vector3 forward = spriteFaceLeft ? Vector2.left : Vector2.right;
        forward = Quaternion.Euler(0, 0, spriteFaceLeft ? -viewDirection : viewDirection) * forward;

        if (GetComponent<SpriteRenderer>().flipX) forward.x = -forward.x;

        Vector3 endpoint = transform.position + (Quaternion.Euler(0, 0, viewFov * 0.5f) * forward);

        Handles.color = new Color(0, 1.0f, 0, 0.1f);
        Handles.DrawSolidArc(transform.position, -Vector3.forward, (endpoint - transform.position).normalized, viewFov, viewDistance);
        Handles.color = new Color(1.0f, 0, 0, 0.1f);
        Handles.DrawSolidDisc(transform.position, Vector3.back, meleeRange);
        Handles.color = new Color(1.0f, 1.0f, 0, 0.1f);
        Handles.DrawSolidDisc(transform.position, Vector3.back, missileRange);
    }
#endif
}