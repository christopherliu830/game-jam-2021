using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    public int toLoad;

    void Start() {
        StartCoroutine(Cutscene());
    }
    IEnumerator Cutscene() {
        for(int i = 0; i < transform.childCount; i++) {
            var t = transform.GetChild(i);
            t.gameObject.SetActive(false);
        }

        for(int i = 0; i < transform.childCount; i++) {
            var t = transform.GetChild(i);
            t.gameObject.SetActive(true);
            var text = t.GetComponentInChildren<TextMeshProUGUI>();
            var img = t.GetComponentInChildren<Image>();
            var imgColor = img.color;

            Color color = new Color(0, 0, 0, 0);
            if (text != null) {
                color = text.color;
            }

            imgColor.a = 0;
            color.a = 0;

            if (text != null) {
                text.color = color;
            }

            img.color = imgColor;

            // FADE IN IMAGE 1s
            while (imgColor.a < 1f) {
                imgColor.a += 0.01f;
                img.color = imgColor;
                yield return new WaitForSeconds(0.01f);
            }

            if (text != null)  {

                // FADE IN TEXT
                while (color.a < 1f) {
                    color.a += 0.01f;
                    text.color = color;
                    yield return new WaitForSeconds(0.01f);
                }

            }

            // HOLD!
            yield return new WaitForSeconds(5f);

            if (text != null ) {

                // FADE OUT TEXT
                while (color.a > 0f) {
                    color.a -= 0.01f;
                    text.color = color;
                    yield return new WaitForSeconds(0.01f);
                }

            }

            // FADE IN IMAGE 1s
            while (imgColor.a > 0f) {
                imgColor.a -= 0.01f;
                img.color = imgColor;
                yield return new WaitForSeconds(0.01f);
            }

            yield return new WaitForSeconds(2f);

        }

        yield return null;

        SceneManager.LoadScene(toLoad);
    }
}
