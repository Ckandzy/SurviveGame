using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamekit2D;
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance
    {
        get
        {
            if (instance != null)
                return instance;

            instance = FindObjectOfType<DialogueManager>();

            if (instance != null)
                return instance;

            return CreateDefault();
        }
    }
    protected static DialogueManager instance;

    public static DialogueManager CreateDefault()
    {
        GameObject sceneCDialogueManager = new GameObject("DialogueManager");
        instance = sceneCDialogueManager.AddComponent<DialogueManager>();
        return instance;
    }

    public Queue<string> sentences = new Queue<string>();

    public DialogueCanvasController dialogueCanvasController;

    public bool isActive = false;

    public void StartDialogue(Dialogue dialogue)
    {
        sentences.Clear();

        foreach (string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }

        DisplayNextSentence();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>is the last dialogue</returns>
    public bool DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return true;
        }
        isActive = true;
        string sentence = sentences.Dequeue();
        dialogueCanvasController.ActivateCanvasWithText(sentence);
        Debug.Log(sentence);
        return false;
    }

    public void EndDialogue()
    {
        if (isActive)
        {
            dialogueCanvasController.DeactivateCanvasWithDelay(0);
            isActive = false;
        }
    }

    public void DisplaySentence(string sentence)
    {
        sentences.Clear();
        dialogueCanvasController.ActivateCanvasWithText(sentence);
    }
}
