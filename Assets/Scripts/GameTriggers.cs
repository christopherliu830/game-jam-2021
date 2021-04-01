using UnityEngine;
using System.Collections.Generic;
using System.Collections;


public class GameTriggers : MonoBehaviour {
  public List<GameTrigger> triggers;
  private bool activated = false;

  private void OnTriggerEnter2D(Collider2D other) {
    if (other.tag.Equals("Player")) {
      if (!activated) {
        StartCoroutine(Animation());
      }
      activated = true;
    }
  }

  IEnumerator Animation() {
    foreach(var trigger in triggers) {
      yield return StartCoroutine(trigger.Activate());
    }
    Destroy(this);
    yield return null;
  }
}

public abstract class GameTrigger : MonoBehaviour { 
  public abstract IEnumerator Activate();
}