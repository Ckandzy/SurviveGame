using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlot
{
    bool HasDone { get; set; }

    IEnumerator DoPlot();
}
