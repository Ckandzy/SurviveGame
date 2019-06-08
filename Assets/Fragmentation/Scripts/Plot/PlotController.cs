using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamekit2D;
using UnityEngine.Playables;

public class PlotController : MonoBehaviour
{
    static PlotController s_Instance;

    //public PlayableDirector playableDirector;

    public static PlotController Instance
    {
        get
        {
            if (s_Instance != null)
                return s_Instance;

            s_Instance = FindObjectOfType<PlotController>();

            if (s_Instance != null)
                return s_Instance;

            Create();

            return s_Instance;
        }
        set { s_Instance = value; }
    }

    [SerializeField]
    public List<MonoBehaviour> PlotList;
    protected Queue<MonoBehaviour> PlotQueue;
    static void Create()
    {
        GameObject plotController = new GameObject("PlotController");
        s_Instance = plotController.AddComponent<PlotController>();
    }

    private IEnumerator Start()
    {
        PlotQueue = new Queue<MonoBehaviour>(PlotList);
        PlayerInput.Instance.ReleaseControl(true);
        PlayerInput.Instance.Interact.GainControl();
        yield return new WaitForSeconds(2f);
        IPlot plot = null;
        if (PlotQueue.Count > 0)
            plot = PlotQueue.Dequeue() as IPlot;
        while (plot != null)
        {
            yield return StartCoroutine(plot.DoPlot());
            if (PlotQueue.Count > 0)
                plot = PlotQueue.Dequeue() as IPlot;
            else
            {
                plot = null;
                break;
            }
        }
        PlayerInput.Instance.GainControl();
        yield break;
    }
}