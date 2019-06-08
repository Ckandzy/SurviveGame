using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

public class DirectorPlot : MonoBehaviour/*, IDataPersister*/, IPlot
{
    public PlayableAsset playableAsset;
    protected PlayableDirector director;

    public bool HasDone { get; set; }

    private void Awake()
    {
        HasDone = false;
        director = GetComponent<PlayableDirector>();
    }

    public IEnumerator DoPlot()
    {
        TriggerDirecotr();
        while (!HasDone)
        {
            yield return null;
        }
        Debug.Log("nmsl");
        yield break;
    }

    public void TriggerDirecotr()
    {
        director.playableAsset = playableAsset;
        director.Play();
        Invoke("FinishInvoke", (float)director.duration);
    }

    void FinishInvoke()
    {
        HasDone = true;
    }
}