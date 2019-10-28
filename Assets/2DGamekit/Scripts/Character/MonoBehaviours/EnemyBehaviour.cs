using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gamekit2D
{
    public enum LaunchMode
    {
        Parabola = 0,
        Beeline,
    }
    public enum SightType
    {
        Rays = 0,
        Cone,
        Rectangle,
    }
    [RequireComponent(typeof(CharacterController2D))]
    [RequireComponent(typeof(Collider2D))]
    public class EnemyBehaviour : MonoBehaviour
    {
        static Collider2D[] s_ColliderCache = new Collider2D[16];

        public Vector3 moveVector { get { return m_MoveVector; } }
        public Transform Target { get { return m_Target; } }

        [Tooltip("If the sprite face left on the spritesheet, enable this. Otherwise, leave disabled")]
        public bool spriteFaceLeft = false;

        [Header("Movement")]
        public float speed;
        public float gravity = 10.0f;

        [Header("References")]
        [Tooltip("If the enemy will be using ranged attack, set a prefab of the projectile it should use")]
        public Bullet projectilePrefab;

        [Header("Scanning settings")]
        public SightType sightType = SightType.Cone;
        [Header("SightType Rectangle")]
        [Tooltip("视野矩形偏移量")]
        public Vector2 offset = new Vector2(1.5f, 1f);
        [Tooltip("视野矩形大小")]
        public Vector2 size = new Vector2(2.5f, 1f);
        [Header("SightType Cone")]
        [Tooltip("The angle of the forward of the view cone. 0 is forward of the sprite, 90 is up, 180 behind etc.")]
        [Range(0.0f,360.0f)]
        public float viewDirection = 0.0f;
        [Range(0.0f, 360.0f)]
        public float viewFov;
        public float viewDistance;
        [Header("SightType Rays")]
        public bool isDoubleSide;
        public float rayDistance;
        [Tooltip("Time in seconds without the target in the view cone before the target is considered lost from sight")]
        public float timeBeforeTargetLost = 3.0f;

        [Header("Melee Attack Data")]
        [EnemyMeleeRangeCheck]
        public float meleeRange = 3.0f;
        public Damager meleeDamager;
        public Damager contactDamager;
        [Tooltip("if true, the enemy will jump/dash forward when it melee attack")]
        public bool attackDash;
        [Tooltip("The force used by the dash")]
        public Vector2 attackForce;

        [Header("Range Attack Data")]
        [Tooltip("From where the projectile are spawned")]
        public Transform shootingOrigin;
        public LaunchMode launchMode = LaunchMode.Beeline;
        public float launchSpeed = 20f;
        public float shootAngle = 45.0f;
        public float shootForce = 100.0f;
        public float fireRate = 2.0f;

        [Header("Audio")]
        public RandomAudioPlayer shootingAudio;
        public RandomAudioPlayer meleeAttackAudio;
        public RandomAudioPlayer dieAudio;
        public RandomAudioPlayer footStepAudio;

        [Header("Misc")]
        [Tooltip("Time in seconds during which the enemy flicker after being hit")]
        public float flickeringDuration;

        protected SpriteRenderer m_SpriteRenderer;
        protected CharacterController2D m_CharacterController2D;
        protected Collider2D m_Collider;
        protected Animator m_Animator;

        protected Vector3 m_MoveVector;
        protected Transform m_Target;
        protected Vector3 m_TargetShootPosition;
        protected float m_TimeSinceLastTargetView;

        protected float m_FireTimer = 0.0f;

        //as we flip the sprite instead of rotating/scaling the object, this give the forward vector according to the sprite orientation
        /*protected*/public Vector2 m_SpriteForward;
        protected Bounds m_LocalBounds;
        protected Vector3 m_LocalDamagerPosition;

        protected RaycastHit2D[] m_RaycastHitCache = new RaycastHit2D[8];
        protected ContactFilter2D m_Filter;

        protected Coroutine m_FlickeringCoroutine = null;
        protected Color m_OriginalColor;

        protected BulletPool m_BulletPool;
        protected BoxCollider2D boxCollider2D;

        protected bool m_Dead = false;

        protected readonly int m_HashSpottedPara = Animator.StringToHash("Spotted");
        protected readonly int m_HashShootingPara = Animator.StringToHash("Shooting");
        protected readonly int m_HashTargetLostPara = Animator.StringToHash("TargetLost");
        protected readonly int m_HashMeleeAttackPara = Animator.StringToHash("MeleeAttack");
        protected readonly int m_HashHitPara = Animator.StringToHash("Hit");
        protected readonly int m_HashDeathPara = Animator.StringToHash("Death");
        protected readonly int m_HashGroundedPara = Animator.StringToHash("Grounded");

        private void Awake()
        {
            m_CharacterController2D = GetComponent<CharacterController2D>();
            m_Collider = GetComponent<Collider2D>();
            m_Animator = GetComponent<Animator>();
            m_SpriteRenderer = GetComponent<SpriteRenderer>();
            boxCollider2D = GetComponent<BoxCollider2D>();

            m_OriginalColor = m_SpriteRenderer.color;

            if(projectilePrefab != null)
                m_BulletPool = BulletPool.GetObjectPool(projectilePrefab.gameObject, 8);

            m_SpriteForward = spriteFaceLeft ? Vector2.left : Vector2.right;
            if (m_SpriteRenderer.flipX) m_SpriteForward = -m_SpriteForward;

            if(meleeDamager != null)
                EndAttack();
        }

        private void OnEnable()
        {
            if (meleeDamager != null)
                EndAttack();

            m_Dead = false;
            m_Collider.enabled = true;
        }

        private void Start()
        {
            SceneLinkedSMB<EnemyBehaviour>.Initialise(m_Animator, this);

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

            if (meleeDamager)
                m_LocalDamagerPosition = meleeDamager.transform.localPosition;
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
            if (m_TimeSinceLastTargetView > 0.0f)
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
            if (facing == -1)
            {
                m_SpriteRenderer.flipX = !spriteFaceLeft;
                m_SpriteForward = Vector2.left;
            }
            else if (facing == 1)
            {
                m_SpriteRenderer.flipX = spriteFaceLeft;
                m_SpriteForward = Vector2.right;
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
            //If the player don't have control, they can't react, so do not pursue them
            if (!PlayerInput.Instance.HaveControl)
                return;

            Vector3 dir = PlayerCharacter.PlayerInstance.transform.position - transform.position;

            switch (sightType)
            {
                case SightType.Cone:
                    {
                        if (dir.sqrMagnitude > viewDistance * viewDistance)
                        {
                            return;
                        }

                        Vector3 testForward = Quaternion.Euler(0, 0, spriteFaceLeft ? Mathf.Sign(m_SpriteForward.x) * -viewDirection : Mathf.Sign(m_SpriteForward.x) * viewDirection) * m_SpriteForward;

                        float angle = Vector3.Angle(testForward, dir);

                        if (angle > viewFov * 0.5f)
                        {
                            return;
                        }
                    }
                    break;
                case SightType.Rectangle:
                    {

                    }
                    break;
                case SightType.Rays:
                    {
                        //if (Mathf.Abs(dir.x) > rayDistance)
                        //{
                        //    return;
                        //}
                        //Vector3[] point = new Vector3[3];
                        //point[0] = boxCollider2D.bounds.center;
                        //point[1] = new Vector3(boxCollider2D.bounds.center.x,
                        //    boxCollider2D.bounds.center.y + boxCollider2D.bounds.extents.y,
                        //    boxCollider2D.bounds.center.z);
                        //point[2] = new Vector3(boxCollider2D.bounds.center.x,
                        //    boxCollider2D.bounds.center.y - boxCollider2D.bounds.extents.y,
                        //    boxCollider2D.bounds.center.z);
                    }
                    break;
            }

            m_Target = PlayerCharacter.PlayerInstance.transform;
            m_TimeSinceLastTargetView = timeBeforeTargetLost;

            m_Animator.SetTrigger(m_HashSpottedPara);
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
            if (m_Target == null)
                return;

            Vector3 toTarget = m_Target.position - transform.position;

            switch (sightType)
            {
                case SightType.Cone:
                    {
                        if (toTarget.sqrMagnitude < viewDistance * viewDistance)
                        {
                            //注：当viewDirection为0时, -viewDirection和viewDirection仍然有效, 在计算机有符号整数和浮点数中, +0和-0是两种不同的表示。
                            Vector3 testForward = Quaternion.Euler(0, 0, spriteFaceLeft ? -viewDirection : viewDirection) * m_SpriteForward;
                            if (m_SpriteRenderer.flipX) testForward.x = -testForward.x;

                            float angle = Vector3.Angle(testForward, toTarget);

                            if (angle <= viewFov * 0.5f)
                            {
                                //we reset the timer if the target is at viewing distance.
                                m_TimeSinceLastTargetView = timeBeforeTargetLost;
                            }
                        }
                    }
                    break;
                case SightType.Rectangle:
                    {

                    }
                    break;
                case SightType.Rays:
                    {

                    }
                    break;
            }

            if (m_TimeSinceLastTargetView <= 0.0f)
            {
                ForgetTarget();
            }
        }

        public void ForgetTarget()
        {
            m_Animator.SetTrigger(m_HashTargetLostPara);
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

        //Call every frame when the enemy is in pursuit to check for range & Trigger the attack if in range
        public void CheckMeleeAttack()
        {
            if (m_Target == null)
            {//we lost the target, shouldn't shoot
                return;
            }

            if((m_Target.transform.position - transform.position).sqrMagnitude < (meleeRange * meleeRange))
            {
                m_Animator.SetTrigger(m_HashMeleeAttackPara);
                meleeAttackAudio.PlayRandomSound();
            }
        }


        //This is called when the damager get enabled (so the enemy can damage the player). 
        //Likely be called by the animation throught animation event (see the attack animation of the Chomper)
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

        //This is call each update if the enemy is in a attack/shooting state, but the timer will early exit if too early to shoot.
        public void CheckShootingTimer()
        {
            if (m_FireTimer > 0.0f)
            {
                return;
            }

            if (m_Target == null)
            {//we lost the target, shouldn't shoot
                return;
            }

            m_Animator.SetTrigger(m_HashShootingPara);
            shootingAudio.PlayRandomSound();

            m_FireTimer = fireRate;
        }

        public void Shooting()
        {
            //Vector2 force = m_SpriteForward.x > 0 ? Vector2.right.Rotate(shootAngle) : Vector2.left.Rotate(-shootAngle);

            //force *= shootForce;
            Vector2 shootPosition = shootingOrigin.transform.localPosition;

            //if we are flipped compared to normal, we need to localy flip the shootposition too
            if ((spriteFaceLeft && m_SpriteForward.x > 0) || (!spriteFaceLeft && m_SpriteForward.x > 0))
                shootPosition.x *= -1;

            BulletObject obj = m_BulletPool.Pop(transform.TransformPoint(shootPosition));
            shootingAudio.PlayRandomSound();

            obj.rigidbody2D.velocity = (GetProjectilVelocity(m_TargetShootPosition, shootingOrigin.transform.position));
        }

        //This will give the velocity vector needed to give to the bullet rigidbody so it reach the given target from the origin.
        private Vector3 GetProjectilVelocity(Vector3 target, Vector3 origin)
        {
            float projectileSpeed = launchSpeed;

            Vector3 velocity = Vector3.zero;
            Vector3 toTarget = target - origin;

            switch (launchMode)
            {
                case LaunchMode.Beeline:
                    {
                        velocity = new Vector3(Mathf.Sign(toTarget.x), 0, 0) * projectileSpeed;
                    }
                    break;
                case LaunchMode.Parabola:
                    {
                        float gSquared = Physics.gravity.sqrMagnitude;
                        float b = projectileSpeed * projectileSpeed + Vector3.Dot(toTarget, Physics.gravity);
                        float discriminant = b * b - gSquared * toTarget.sqrMagnitude;
                        // Check whether the target is reachable at max speed or less.
                        if (discriminant < 0)
                        {
                            velocity = toTarget;
                            Debug.Log(toTarget);
                            velocity.y = 0;
                            velocity.Normalize();
                            velocity.y = 0.7f;

                            velocity *= projectileSpeed;
                            return velocity;
                        }

                        float discRoot = Mathf.Sqrt(discriminant);

                        // Highest
                        float T_max = Mathf.Sqrt((b + discRoot) * 2f / gSquared);

                        // Lowest speed arc
                        float T_lowEnergy = Mathf.Sqrt(Mathf.Sqrt(toTarget.sqrMagnitude * 4f / gSquared));

                        // Most direct with max speed
                        float T_min = Mathf.Sqrt((b - discRoot) * 2f / gSquared);

                        float T = 0;

                        // 0 = highest, 1 = lowest, 2 = most direct
                        int shotType = 1;

                        switch (shotType)
                        {
                            case 0:
                                T = T_max;
                                break;
                            case 1:
                                T = T_lowEnergy;
                                break;
                            case 2:
                                T = T_min;
                                break;
                            default:
                                break;
                        }

                        velocity = toTarget / T - Physics.gravity * T / 2f;
                    }
                    break;
            }

            return velocity;
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
                if(sinceLastChange > flickeringDuration)
                {
                    sinceLastChange -= flickeringDuration;
                    state = 1 - state;
                    m_SpriteRenderer.color = state == 1 ? transparent : m_OriginalColor;
                }
            }

            m_SpriteRenderer.color = m_OriginalColor;
        }

        public void DisableDamage ()
        {
            if(meleeDamager != null)
                meleeDamager.DisableDamage ();
            if(contactDamager != null)
                contactDamager.DisableDamage ();
        }

        public void PlayFootStep()
        {
            footStepAudio.PlayRandomSound();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            switch (sightType)
            {
                case SightType.Cone:
                    {
                        //draw the cone of view
                        Vector3 forward = spriteFaceLeft ? Vector2.left : Vector2.right;
                        forward = Quaternion.Euler(0, 0, spriteFaceLeft ? -viewDirection : viewDirection) * forward;

                        if (GetComponent<SpriteRenderer>().flipX) forward.x = -forward.x;

                        Vector3 endpoint = transform.position + (Quaternion.Euler(0, 0, viewFov * 0.5f) * forward);

                        Handles.color = new Color(0, 1.0f, 0, 0.2f);
                        Handles.DrawSolidArc(transform.position, -Vector3.forward, (endpoint - transform.position).normalized, viewFov, viewDistance);
                    } break;
                case SightType.Rectangle:
                    {
                        //Handles.color = new Color(0, 1.0f, 0, 0.2f);
                        Handles.DrawSolidRectangleWithOutline(new Rect((Vector2)transform.position + offset, size), new Color(0, 1.0f, 0, 0.2f), new Color(0, 1.0f, 0, 0.2f));
                    } break;
                //case SightType.Rays:
                //    {
                //        Vector3[] point = new Vector3[3];
                //        point[0] = boxCollider2D.bounds.center;
                //        point[1] = new Vector3(boxCollider2D.bounds.center.x,
                //            boxCollider2D.bounds.center.y + boxCollider2D.bounds.extents.y,
                //            boxCollider2D.bounds.center.z);
                //        point[2] = new Vector3(boxCollider2D.bounds.center.x,
                //            boxCollider2D.bounds.center.y - boxCollider2D.bounds.extents.y,
                //            boxCollider2D.bounds.center.z);
                //        if (isDoubleSide)
                //        {
                //            for (int i = 0; i < point.Length; i++)
                //            {
                //                Handles.DrawLine(point[i], new Vector3(point[i].x + rayDistance, point[i].y, point[i].z));
                //                Handles.DrawLine(point[i], new Vector3(point[i].x - rayDistance, point[i].y, point[i].z));
                //            }
                //        }
                //        else
                //        {
                //            Vector3 forward = spriteFaceLeft ? Vector2.left : Vector2.right;
                //            if (GetComponent<SpriteRenderer>().flipX) forward.x = -forward.x;
                //            for (int i = 0; i < point.Length; i++)
                //            {
                //                Handles.DrawLine(point[i], new Vector3(point[i].x + rayDistance * forward.x, point[i].y, point[i].z));
                //            }
                //        }
                //    } break;
            }
            //Draw attack range
            Handles.color = new Color(1.0f, 0, 0, 0.1f);
            Handles.DrawSolidDisc(transform.position, Vector3.back, meleeRange);
        }
#endif
    }

    //bit hackish, to avoid to have to redefine the whole inspector, we use an attirbute and associated property drawer to 
    //display a warning above the melee range when it get over the view distance.
    public class EnemyMeleeRangeCheckAttribute : PropertyAttribute
    {

    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(EnemyMeleeRangeCheckAttribute))]
    public class EnemyMeleeRangePropertyDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty viewRangeProp = property.serializedObject.FindProperty("viewDistance");
            if (viewRangeProp.floatValue < property.floatValue)
            {
                Rect pos = position;
                pos.height = 30;
                EditorGUI.HelpBox(pos, "Melee range is bigger than View distance. Note enemies only attack if target is in their view range first", MessageType.Warning);
                position.y += 30;
            }

            EditorGUI.PropertyField(position, property, label);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty viewRangeProp = property.serializedObject.FindProperty("viewDistance");
            if (viewRangeProp.floatValue < property.floatValue)
            {
                return base.GetPropertyHeight(property, label) + 30;
            }
            else
                return base.GetPropertyHeight(property, label);
        }
    }
#endif
}