using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class LightControlTestClip : PlayableAsset
{
    public ExposedReference<Light> light;
    public Color color = Color.white;
    public float intensity = 1.0f;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<LightControlTestBehaviour>.Create(graph);

        var lightControlBehaviour = playable.GetBehaviour();
        lightControlBehaviour.light = light.Resolve(graph.GetResolver());
        lightControlBehaviour.color = color;
        lightControlBehaviour.intensity = intensity;

        return playable;
    }
}
