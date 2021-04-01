using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static readonly float GRAVITY = -3f;
    public static float ROPE_LENGTH = 12f;
    public static GameManager instance ;
    public Material ropeMaterial;
    public CharController[] characters;
    
    private int _currentCharacterIndex = 0;

    void Start()
    {

        if (instance == null) {
            instance = this;
        } else Debug.LogError("Multiple " + this);

        characters[_currentCharacterIndex].Focus();
        characters[0].maxSeparation = Mathf.Infinity;
        characters[1].maxSeparation = Mathf.Infinity;
        characters[1].followDistance = 4f;
        characters[0].partner = characters[1];
        characters[1].partner = characters[0];
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift)) {
            characters[_currentCharacterIndex].Unfocus();
            _currentCharacterIndex = (_currentCharacterIndex + 1) % characters.Length;
            characters[_currentCharacterIndex].Focus();
            var c = Camera.main.GetComponent<FollowTargets>();
            c.Focus(characters[_currentCharacterIndex].transform);
        }

        if (Input.GetKeyDown(KeyCode.R)) {
            var index = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(index);
        }
    }
}
