using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreationTrigger: GameTrigger 
{
    public List<GameObject> targets;
    bool activated = false;
    public float cameraZoomFactor = 1;

    void Start() {
      foreach(var target in targets) {
        target.SetActive(false);
      }
    }

    public override IEnumerator Activate() {
      var c = Camera.main.GetComponent<FollowTargets>();
      var oldSize = Camera.main.orthographicSize;
      c.SetSize(oldSize * cameraZoomFactor);
      var old = c.targets;
      foreach(var target in targets) {
        c.Focus(target.transform);
        yield return new WaitForSeconds(2);
        target.SetActive(true);
        target.transform.parent = null;
        yield return new WaitForSeconds(2);
      }
      c.targets = old;
      c.SetSize(oldSize);
      yield return null;
    }
}
