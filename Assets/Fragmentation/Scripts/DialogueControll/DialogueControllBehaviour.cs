using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class DialogueControllBehaviour : PlayableBehaviour
{
    //public DialogueManager DialogueManager = null;
    public string sentence;
    public bool hasTriggered = false;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        DialogueManager dialogueManager = playerData as DialogueManager;
        if (dialogueManager != null && !hasTriggered)
        {
            dialogueManager.DisplaySentence(sentence);
            hasTriggered = true;
            Debug.Log("jack slow fuck");
        }
    }
}
