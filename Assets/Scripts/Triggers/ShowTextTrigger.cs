using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShowTextTrigger : GameTrigger
{
  public TextMeshProUGUI text;
  public override IEnumerator Activate()
  {
    var color = text.color;
    color.a = 0;
    text.enabled = true;
    text.color = color; 
    Debug.Log("?");
    if (GameManager.GlobalState.TryGetValue("activated-blocks", out object active)) {
        yield return null;
    } else {
        GameManager.GlobalState["activated-blocks"] = 1;
        while (color.a < 1f) {
            color.a += 0.01f;
            text.color = color;
            yield return new WaitForSeconds(0.01f);
        }
        yield return new WaitForSeconds(1);
        while (color.a > 0) {
            color.a -= 0.01f;
            text.color = color;
            yield return new WaitForSeconds(0.01f);
        }
    }
    Debug.Log("end");
    yield return null;
  }
}
