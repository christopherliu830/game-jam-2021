using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public DialogueData data;
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.tag.Equals("Player")) {
            DialogueManager.Play(data);
            Destroy(gameObject);
        }
    }
}
