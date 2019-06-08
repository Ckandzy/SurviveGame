using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class DialogueControllClip : PlayableAsset
{
    public string sentence;

    public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<DialogueControllBehaviour>.Create (graph);
        var dialogueControllBehaviour = playable.GetBehaviour ();
        dialogueControllBehaviour.sentence = sentence;
        return playable;
    }
}
