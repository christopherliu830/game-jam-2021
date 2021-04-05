using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : GameTrigger
{
    public DialogueData data;
    public int sceneNo;
    public float delay = 0;
    public override IEnumerator Activate() {
        yield return new WaitForSeconds(delay);
        yield return StartCoroutine(DialogueManager.PlayDialogue(data, sceneNo));
    }
}
