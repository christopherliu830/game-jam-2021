using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Goal : MonoBehaviour
{
    public MeshRenderer mesh;
    public int nextLevel;

    public int goaled = 0;

    // Update is called once per frame
    void Update()
    {
        float incr = Time.deltaTime * 10;
        mesh.transform.Rotate(new Vector3(incr, incr * 1.5f, incr * .333f), Space.Self);
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.tag.Equals("Player")) {
            goaled++;
        }
        if (goaled == 2) {
            SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.tag.Equals("Player")) {
            goaled--;
        }
    }
}
