using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtTrigger : GameTrigger
{
    public Transform target;
    public float cameraZoomFactor = 1;
  public override IEnumerator Activate()
  {
    if (!target.gameObject.activeInHierarchy) yield break;
    var c = Camera.main.GetComponent<FollowTargets>();
    var oldSize = c.Size;
    c.SetSize(oldSize * cameraZoomFactor);
    var old = c.targets;
    c.Focus(target.transform);
    yield return new WaitForSeconds(3);
    c.SetSize(oldSize);
    c.Focus(old);
    yield return null;
  }
}
