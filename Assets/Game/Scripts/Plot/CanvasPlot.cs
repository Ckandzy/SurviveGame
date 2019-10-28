using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamekit2D;

public class CanvasPlot : MonoBehaviour, IPlot
{
    public ScreenFader screenFader;
    public bool HasDone { get; set; }

    private void Awake()
    {
        HasDone = false;
    }

    public IEnumerator DoPlot()
    {
        yield return StartCoroutine(ScreenFader.FadeSceneOut());
        HasDone = true;
    }
}
