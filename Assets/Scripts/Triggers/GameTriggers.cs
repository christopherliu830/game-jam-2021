using UnityEngine;
using System.Collections.Generic;
using System.Collections;


[SelectionBase]
public class GameTriggers : MonoBehaviour {
  public bool repeatable = false;
  public List<GameTrigger> triggers;
  private bool activated = false;
  public delegate void TriggerHandler(int triggerNo);
  public TriggerHandler OnTriggerEnd;
  public TriggerHandler OnTriggerStart;

  public List<IEnumerator> blockingAnimations = new List<IEnumerator>();

  private void OnTriggerEnter2D(Collider2D other) {
    if (other.tag.Equals("Player")) {
      if (!activated || repeatable) {

        var audio = GetComponent<AudioSource>();
        if (audio != null) audio.Play();

        StartCoroutine(Animation());

      }
      activated = true;
    }
  }

  IEnumerator Animation() {
    int num = 0;
    foreach(var trigger in triggers) {

      OnTriggerStart?.Invoke(num);
      yield return WaitForAnimations();

      yield return StartCoroutine(trigger.Activate());
      OnTriggerEnd?.Invoke(num);

      yield return WaitForAnimations();
      num++;
    }
    if (!repeatable) {
      gameObject.SetActive(false);
    }
    yield return null;
  }

  IEnumerator WaitForAnimations() {
    foreach(var a in blockingAnimations) {
      yield return StartCoroutine(a);
    }
  }
}

public abstract class GameTrigger : MonoBehaviour { 
  public abstract IEnumerator Activate();
}