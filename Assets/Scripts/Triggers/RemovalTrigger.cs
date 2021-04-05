using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemovalTrigger : GameTrigger
{
    public List<GameObject> targets;
    public AudioClip audioClip;
    bool activated = false;

    public override IEnumerator Activate() {
        var c = Camera.main.GetComponent<FollowTargets>();
        var source = GetComponent<AudioSource>();
        GameManager.DisableCharacterInputs();
        var old = c.targets;
        foreach(var target in targets) {
            c.Focus(target.transform);
            yield return new WaitForSeconds(2);
            target.gameObject.SetActive(false);
            if (source != null) { source.PlayOneShot(audioClip, 0.5f); }
            yield return new WaitForSeconds(2);
            Destroy(target);
            c.Focus(old);
        }
        GameManager.EnableCharacterInputs();
        yield return null;
    }
}
