using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreationTrigger: GameTrigger 
{
    public List<GameObject> targets;
    bool activated = false;
    public float cameraZoomFactor = 1;
    public AudioClip audioClip;

    void Start() {
      foreach(var target in targets) {
        target.SetActive(false);
      }
    }

    public override IEnumerator Activate() {
      var c = Camera.main.GetComponent<FollowTargets>();
      var source = GetComponent<AudioSource>();
      var oldSize = Camera.main.orthographicSize;
      c.SetSize(oldSize * cameraZoomFactor);
      GameManager.DisableCharacterInputs();
      var old = c.targets;
      foreach(var target in targets) {
        c.Focus(target.transform);
        yield return new WaitForSeconds(2);
        target.SetActive(true);
        target.transform.parent = null;
        if (source != null) { source.PlayOneShot(audioClip, 0.5f); }
        yield return new WaitForSeconds(2);
      }
      c.targets = old;
      c.SetSize(oldSize);
      GameManager.EnableCharacterInputs();
      yield return null;
    }
}
