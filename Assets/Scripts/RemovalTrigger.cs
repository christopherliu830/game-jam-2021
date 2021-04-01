using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemovalTrigger : GameTrigger
{
    public List<GameObject> targets;
    bool activated = false;

    public override IEnumerator Activate() {
        var c = Camera.main.GetComponent<FollowTargets>();
        var old = c.targets;
        foreach(var target in targets) {
            c.Focus(target.transform);
            yield return new WaitForSeconds(2);
            c.Focus(old);
            Destroy(target);
        }
        yield return null;
    }
}
