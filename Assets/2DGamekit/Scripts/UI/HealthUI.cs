using System.Collections;
using UnityEngine;

namespace Gamekit2D
{
    public class HealthUI : MonoBehaviour
    {
        [Header("Reference")]
        public GameObject healthIconPrefab;
        public Damageable representedDamageable;

        [Header("Setting")]
        public int maxIconNum = 20;

        protected Animator[] m_HealthIconAnimators;

        protected readonly int m_HashActivePara = Animator.StringToHash ("Active");
        protected readonly int m_HashInactiveState = Animator.StringToHash ("Inactive");
        //protected const float k_HeartIconAnchorWidth = 0.041f;

        IEnumerator Start ()
        {
            if(representedDamageable == null)
                yield break;

            yield return null;
            
            m_HealthIconAnimators = new Animator[maxIconNum];

            for (int i = 0; i < maxIconNum; i++)
            {
                GameObject healthIcon = Instantiate (healthIconPrefab);
                healthIcon.transform.SetParent (transform);
                //RectTransform healthIconRect = healthIcon.transform as RectTransform;
                //healthIconRect.anchoredPosition = Vector2.zero;
                //healthIconRect.sizeDelta = Vector2.zero;
                //healthIconRect.anchorMin += new Vector2(k_HeartIconAnchorWidth, 0f) * i;
                //healthIconRect.anchorMax += new Vector2(k_HeartIconAnchorWidth, 0f) * i;
                m_HealthIconAnimators[i] = healthIcon.GetComponent<Animator> ();

                if (Mathf.CeilToInt(Mathf.Clamp(representedDamageable.CurrentHealth / representedDamageable.MaxHealth, 0, maxIconNum)) < i + 1)
                {
                    m_HealthIconAnimators[i].Play(m_HashInactiveState);
                    m_HealthIconAnimators[i].SetBool(m_HashActivePara, false);
                }
            }
        }

        private void Update()
        {
            ChangeHitPointUI(representedDamageable);
        }

        public void ChangeHitPointUI (Damageable damageable)
        {
            if(m_HealthIconAnimators == null)
                return;
            
            for (int i = 0; i < m_HealthIconAnimators.Length; i++)
            {
               m_HealthIconAnimators[i].SetBool(m_HashActivePara, Mathf.CeilToInt(representedDamageable.CurrentHealth / representedDamageable.MaxHealth * maxIconNum) >= i + 1);
            }
        }
    }
}