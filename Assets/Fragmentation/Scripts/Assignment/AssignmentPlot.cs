using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamekit2D;
using UnityEngine.Playables;

public class AssignmentPlot : MonoBehaviour, IPlot
{
    public int assID;
    protected Assignment assignment;
    public enum AssignType
    {
        Awake = 0,
        Event,
    }
    public AssignType assignType = AssignType.Awake;
    bool isFirstExecute = true;
    bool m_CanExecuteButtons = false;
    protected DialogueManager dialogueManager;

    public bool HasDone { get; set; }

    private void Awake()
    {
        HasDone = false;
    }

    private void Start()
    {

        dialogueManager = FindObjectOfType<DialogueManager>();
        for (int i = 0; i < AssignManager.Instance.assList.Count; i++)
        {
            if (AssignManager.Instance.assList[i].assID == assID)
                assignment = AssignManager.Instance.assList[i];
        }
        if (assignment != null && assignType == AssignType.Awake)
        {
            TriggerAssign();
        }
        //else
        //{
        //    Debug.LogError("Assignment not exit");
        //}
    }

    private void FixedUpdate()
    {
        if (m_CanExecuteButtons && PlayerInput.Instance.Interact.Down)
        {
            if (isFirstExecute && !HasDone)
            {
                isFirstExecute = false;
                dialogueManager.StartDialogue(assignment.dialogue);
            }
            else
            {
                if (dialogueManager.DisplayNextSentence())
                {
                    isFirstExecute = true;
                    m_CanExecuteButtons = false;
                    HasDone = true;
                }
            }
        }
    }

    public void TriggerAssign()
    {
        m_CanExecuteButtons = true;
        isFirstExecute = false;
        //PlayerInput.Instance.ReleaseControl();
        //director.Play();
        //m_AlreadyTriggered = true;
        //Invoke("FinishInvoke", (float)director.duration);
        dialogueManager.StartDialogue(assignment.dialogue);
    }

    void FinishInvoke()
    {
        //dialogueManager.StartDialogue(assignment.dialogue);
        //PlayerInput.Instance.GainControl();
    }

    public IEnumerator DoPlot()
    {
        TriggerAssign();
        while (!HasDone)
        {
            yield return null;
        }
        yield break;
    }
}
