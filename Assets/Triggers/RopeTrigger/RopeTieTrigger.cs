using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RopeTieTrigger : MonoBehaviour
{
    public CharController big;
    public CharController little;
    private GameTriggers _triggers;

    public void Start() {
        _triggers = GetComponent<GameTriggers>();
        _triggers.OnTriggerEnd += OnSceneChange;
    }

    void OnSceneChange(int i) {
        switch(i) {
            case 0:
                _triggers.blockingAnimations.Add(LittleMoveBig());
                break;
            case 1:
                _triggers.blockingAnimations.Add(RopeTie());
                break;

        }
    }

    IEnumerator LittleMoveBig() {
        var c = Camera.main.GetComponent<FollowTargets>();
        little.inputsEnabled = false;
        big.inputsEnabled = false;
        c.Focus(little.transform); // Look at little
        yield return new WaitForSeconds(1);
        StartCoroutine(little.MoveTo(big.Position, 2));
        yield return new WaitForSeconds(2); // Wait for little to walk up to big
    }

    IEnumerator RopeTie() {
        yield return new WaitForSeconds(2);
        var c = Camera.main.GetComponent<FollowTargets>();
        c.Focus(new List<Transform> { little.transform, big.transform }); // Look at little
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
        yield return null;
    }
}
