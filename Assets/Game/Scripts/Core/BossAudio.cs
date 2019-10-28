using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAudio : MonoBehaviour
{
    public AudioClip[] boss_1;
    public AudioClip[] boss_2;
    public AudioSource audioSource;

    public void PlayAudio(int i, int stage)
    {
        //if (stage == 1)
        //{
        //    audioSource.clip = boss_1[i - 1];
        //    audioSource.loop = true;
        //    audioSource.Play();
        //}
        //else
        //{
        //    audioSource.clip = boss_2[i - 1];
        //    audioSource.loop = true;
        //    audioSource.Play();
        //}
    }

    public void Stop()
    {
        //audioSource.Stop();
    }
}
