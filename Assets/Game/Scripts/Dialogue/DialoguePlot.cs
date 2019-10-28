using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamekit2D;

public class DialoguePlot : MonoBehaviour, IPlot
{
    public Dialogue dialogue = new Dialogue();
    bool isFirstExecute;
    bool m_CanExecuteButtons;
    protected DialogueManager dialogueManager;

    public bool HasDone { get; set; }

    private void Awake()
    {
        isFirstExecute = true;
        HasDone = false;
    }

    private void Start()
    {
        dialogueManager = FindObjectOfType<DialogueManager>();
    }

    private void FixedUpdate()
    {
        if (m_CanExecuteButtons && PlayerInput.Instance.Interact.Down)
        {
            if (!isFirstExecute && dialogueManager.DisplayNextSentence())
            {
                isFirstExecute = true;
                HasDone = true;
            }
        }
    }

    void TriggerDialogue()
    {
        isFirstExecute = false;
        dialogueManager.StartDialogue(dialogue);
        m_CanExecuteButtons = true;
    }

    public IEnumerator DoPlot()
    {
        TriggerDialogue();
        while (!HasDone)
        {
            yield return null;
        }
        m_CanExecuteButtons = false;
        yield break;
    }
}
