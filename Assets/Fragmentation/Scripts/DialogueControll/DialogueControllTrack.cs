using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackClipType(typeof(DialogueControllClip))]
[TrackBindingType(typeof(DialogueManager))]
public class DialogueControllTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return ScriptPlayable<DialogueControllMixerBehaviour>.Create (graph, inputCount);
    }
}
