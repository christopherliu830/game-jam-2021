using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Goal : MonoBehaviour
{
    public int nextLevel;

    public int goaled = 0;

    // Update is called once per frame
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.tag.Equals("Player")) {
            goaled++;
        }
        if (goaled == 2) {
            SceneManager.LoadSceneAsync(nextLevel, LoadSceneMode.Single);
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.tag.Equals("Player")) {
            goaled--;
        }
    }
}
