using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShowTextNormalTrigger : GameTrigger
{
  public TextMeshProUGUI text;
  public bool blocking = false;
  public bool stayOn = false;
  private void Start() {
    text.enabled = false;
  }
  public override IEnumerator Activate()
  {
    if (blocking) yield return StartCoroutine(Animation());
    else { GameManager.instance.StartCoroutine(Animation()); }
  }

  public IEnumerator Animation() {
    var color = text.color;
    color.a = 0;
    text.enabled = true;
    text.color = color; 
    while (color.a < 1f) {
        color.a += 0.01f;
        text.color = color;
        yield return new WaitForSeconds(0.01f);
    }
    yield return new WaitForSeconds(1);
    if (stayOn) {
        yield return null;
    } else {
        while (color.a > 0) {
            color.a -= 0.01f;
            text.color = color;
            yield return new WaitForSeconds(0.01f);
        }
    }
    yield return null;
  }
}
