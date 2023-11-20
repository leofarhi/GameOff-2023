using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueValue : ScriptableObject
{
    public static DialogueValue currentDialogue;
    public static IEnumerator LaunchDialogue(DialogueValue dialogueValue, DialogueValue last = null)
    {
        if (dialogueValue == null)
            yield break;
        GameState gameState = GameStateManager.Instance.CurrentGameState;
        currentDialogue = dialogueValue;
        dialogueValue.StartDialogue(last);
        yield return null;
        while (!dialogueValue.isFinished && currentDialogue == dialogueValue)
        {
            dialogueValue.UpdateDialogue();
            yield return null;
        }
        if (dialogueValue.isFinished) 
            dialogueValue.EndDialogue();
        if (currentDialogue != dialogueValue)
            yield break;
        currentDialogue = null;
        if (dialogueValue.next != null)
        {
            yield return LaunchDialogue(dialogueValue.next, dialogueValue);
        }
    }
    
    public static void StopDialogue()
    {
        if (currentDialogue == null)
            return;
        currentDialogue.isFinished = true;
        currentDialogue = null;
    }

    public DialogueValue next;
    
    public virtual bool isFinished { get; set; }
    protected virtual void StartDialogue(DialogueValue last = null)
    {
        
    }
    
    protected virtual void UpdateDialogue()
    {
        
    }
    
    protected virtual void EndDialogue()
    {
        
    }
}
