using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Gamekit2D
{
    [RequireComponent(typeof(AudioSource))]
    public class ContinuousAudioPlayer : MonoBehaviour
    {
        [System.Serializable]
        public struct TileOverride
        {
            public TileBase tile;
            public AudioClip[] clips;
        }

        public AudioClip[] clips;

        public TileOverride[] overrides;

        public bool randomizePitch = false;
        public float pitchRange = 0.2f;

        protected AudioSource m_Source;
        protected Dictionary<TileBase, AudioClip[]> m_LookupOverride;

        private void Awake()
        {
            m_Source = GetComponent<AudioSource>();
            m_LookupOverride = new Dictionary<TileBase, AudioClip[]>();

            for (int i = 0; i < overrides.Length; ++i)
            {
                if (overrides[i].tile == null)
                    continue;

                m_LookupOverride[overrides[i].tile] = overrides[i].clips;
            }
        }

        public void PlayRandomSound(TileBase surface = null)
        {
            AudioClip[] source = clips;

            AudioClip[] temp;
            if (surface != null && m_LookupOverride.TryGetValue(surface, out temp))
                source = temp;

            int choice = Random.Range(0, source.Length);

            if (randomizePitch)
                m_Source.pitch = Random.Range(1.0f - pitchRange, 1.0f + pitchRange);

            //m_Source.PlayOneShot(source[choice]);
            m_Source.loop = true;
            if (!m_Source.isPlaying)
            {
                m_Source.clip = source[choice];
                m_Source.Play();
            }
        }

        public void Stop()
        {
            m_Source.Stop();
        }
    }
}