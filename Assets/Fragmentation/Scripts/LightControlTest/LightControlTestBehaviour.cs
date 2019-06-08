using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class LightControlTestBehaviour : PlayableBehaviour
{
    public Light light = null;
    public Color color = Color.white;
    public float intensity = 1f;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (light != null)
        {
            light.color = color;
            light.intensity = intensity;
        }
    }
}
