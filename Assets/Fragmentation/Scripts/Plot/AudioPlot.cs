using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Gamekit2D;

public class AudioPlot : MonoBehaviour/*, IDataPersister*/, IPlot
{
    public RandomAudioPlayer randomAudio;

    public bool HasDone { get; set; }

    private void Awake()
    {
        HasDone = false;
    }

    public IEnumerator DoPlot()
    {
        TriggerAudio();
        while (!HasDone)
        {
            yield return null;
        }
        Debug.Log("nmsl");
        yield break;
    }

    public void TriggerAudio()
    {
        randomAudio.PlayRandomSound();
        HasDone = true;
    }
}