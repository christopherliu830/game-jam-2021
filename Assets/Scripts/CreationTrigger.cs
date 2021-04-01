using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreationTrigger: GameTrigger 
{
    public List<GameObject> targets;
    bool activated = false;

    void Start() {
      foreach(var target in targets) {
        target.SetActive(false);
      }
    }

    public override IEnumerator Activate() {
      var c = Camera.main.GetComponent<FollowTargets>();
      var old = c.targets;
      foreach(var target in targets) {
        c.Focus(target.transform);
        yield return new WaitForSeconds(2);
        target.SetActive(true);
        yield return new WaitForSeconds(2);
      }
      c.targets = old;
      yield return null;
    }
}
