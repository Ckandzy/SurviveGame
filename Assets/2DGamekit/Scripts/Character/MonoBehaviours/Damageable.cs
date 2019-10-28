using System;
using UnityEngine;
using UnityEngine.Events;

namespace Gamekit2D
{
    public class Damageable : MonoBehaviour, IDataPersister
    {
        [Serializable]
        public class HealthEvent : UnityEvent<Damageable>
        { }

        [Serializable]
        public class DamageEvent : UnityEvent<Damager, Damageable>
        { }

        [Serializable]
        public class HealEvent : UnityEvent<float, Damageable>
        { }

        [Tooltip("当收到伤害时无敌")]
        public bool invulnerableAfterDamage = true;
        [Tooltip("无敌效果持续时间")]
        public float invulnerabilityDuration = 3f;
        public bool disableOnDeath = false;
        [Tooltip("An offset from the object position used to set from where the distance to the damager is computed")]
        public Vector2 centreOffset = new Vector2(0f, 1f);
        public HealthEvent OnHealthSet;
        public DamageEvent OnTakeDamage;
        public DamageEvent OnDie;
        public HealEvent OnGainHealth;
        [HideInInspector]
        public DataSettings dataSettings;

        protected bool _invulnerable;
        protected float _inulnerabilityTimer;
        protected Vector2 _damageDirection;
        protected bool _resetHealthOnSceneReload;

        protected Character _character;

#if UNITY_EDITOR
        [SerializeField]
#endif
        protected float _maxHealth;
#if UNITY_EDITOR
        [SerializeField]
#endif
        protected float _currentHealth;

        public float CurrentHealth
        {
            get
            {
                if (_character != null)
                    _currentHealth = _character.health.CurValue;
                return _currentHealth;
            }
        }

        public float MaxHealth
        {
            get
            {
                if (_character != null)
                    _maxHealth =  _character.health.MaxValue;
                return _maxHealth;
            }
        }

        private void OnValidate()
        {
            if (_character == null)
                _character = GetComponent<Character>();
        }

        private void Awake()
        {
            _character = GetComponent<Character>();
        }

        void OnEnable()
        {
            PersistentDataManager.RegisterPersister(this);
            _currentHealth = MaxHealth;

            OnHealthSet.Invoke(this);

            DisableInvulnerability();
        }

        void OnDisable()
        {
            PersistentDataManager.UnregisterPersister(this);
        }

        void Update()
        {
            if (_invulnerable)
            {
                _inulnerabilityTimer -= Time.deltaTime;

                if (_inulnerabilityTimer <= 0f)
                {
                    _invulnerable = false;
                }
            }
        }

        /// <summary>
        /// 开启无敌
        /// </summary>
        /// <param 是否忽略无敌时间="ignoreTimer"></param>
        public void EnableInvulnerability(bool ignoreTimer = false)
        {
            _invulnerable = true;
            //technically don't ignore timer, just set it to an insanly big number. Allow to avoid to add more test & special case.
            _inulnerabilityTimer = ignoreTimer ? float.MaxValue : invulnerabilityDuration;
        }

        /// <summary>
        /// 关闭无敌
        /// </summary>
        public void DisableInvulnerability()
        {
            _invulnerable = false;
        }

        public Vector2 GetDamageDirection()
        {
            return _damageDirection;
        }

        /// <summary>
        /// 造成伤害
        /// </summary>
        /// <param name="damager"></param>
        /// <param name="ignoreInvincible"></param>
        public void TakeDamage(Damager damager, bool ignoreInvincible = false)
        {
            if ((_invulnerable && !ignoreInvincible) || _currentHealth <= 0)
                return;

            //we can reach that point if the damager was one that was ignoring invincible state.
            //We still want the callback that we were hit, but not the damage to be removed from health.
            if (!_invulnerable)
            {
                _currentHealth -= damager.Damage;
                OnHealthSet.Invoke(this);
            }

            _damageDirection = transform.position + (Vector3)centreOffset - damager.transform.position;

            OnTakeDamage.Invoke(damager, this);

            if (_currentHealth <= 0)
            {
                OnDie.Invoke(damager, this);
                _resetHealthOnSceneReload = true;
                EnableInvulnerability();
                if (disableOnDeath) gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 增加生命
        /// </summary>
        /// <param name="amount"></param>
        public void GainHealth(float amount)
        {
            _currentHealth += amount;

            if (_currentHealth > MaxHealth)
                _currentHealth = MaxHealth;

            OnHealthSet.Invoke(this);

            OnGainHealth.Invoke(amount, this);
        }

        /// <summary>
        /// 设置生命
        /// </summary>
        /// <param name="amount"></param>
        public void SetHealth(float amount)
        {
            _currentHealth = amount;

            OnHealthSet.Invoke(this);
        }

        public DataSettings GetDataSettings()
        {
            return dataSettings;
        }

        public void SetDataSettings(string dataTag, DataSettings.PersistenceType persistenceType)
        {
            dataSettings.dataTag = dataTag;
            dataSettings.persistenceType = persistenceType;
        }

        public Data SaveData()
        {
            return new Data<float, bool>(CurrentHealth, _resetHealthOnSceneReload);
        }

        public void LoadData(Data data)
        {
            Data<float, bool> healthData = (Data<float, bool>)data;
            //如果m_ResetHealthOnSceneReload为真则重置生命为初始值，否则读取存储值
            _currentHealth = healthData.value1 ? MaxHealth : healthData.value0;
            OnHealthSet.Invoke(this);
        }


    }
}
