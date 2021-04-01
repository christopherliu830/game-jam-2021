using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RopeTieTrigger : GameTrigger
{
    public CharController big;
    public CharController little;
    public override IEnumerator Activate() {
        var c = Camera.main.GetComponent<FollowTargets>();

        little.inputsEnabled = false;
        big.inputsEnabled = false;
        c.Focus(little.transform); // Look at little
        yield return new WaitForSeconds(1);
        StartCoroutine(little.MoveTo(big.Position, 4));
        yield return new WaitForSeconds(4); // Wait for little to walk up to big
        HingeJointConnect.Connect(
            little.GetComponent<Rigidbody2D>(), 
            big.GetComponent<Rigidbody2D>(), 20, 5)
            .GetComponent<LineRenderer>().sharedMaterial = GameManager.instance.ropeMaterial;
        little.maxSeparation = GameManager.ROPE_LENGTH;
        little.followDistance = GameManager.ROPE_LENGTH;
        big.maxSeparation = GameManager.ROPE_LENGTH;
        big.followDistance = GameManager.ROPE_LENGTH;
        yield return new WaitForSeconds(2);
        c.Focus(big.transform);
        little.inputsEnabled = true;
        big.inputsEnabled = true;
    }
}
