using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportTrigger : GameTrigger
{
    public List<CharController> targets;
    public Vector2 teleportTo;

    public override IEnumerator Activate() {
        foreach(var target in targets) {
            target.Position = teleportTo;
        }
        yield return null;
    }

}
