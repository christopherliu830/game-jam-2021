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

    public bool RopeOnStart = true;
    
    private int _currentCharacterIndex = 0;

    void Start()
    {

        if (instance == null) {
            instance = this;
        } else Debug.LogError("Multiple " + this);

        characters[_currentCharacterIndex].Focus();
        var l = characters[0];
        var b = characters[1];
        if (RopeOnStart) {
            HingeJointConnect.Connect(
                l.GetComponent<Rigidbody2D>(), 
                b.GetComponent<Rigidbody2D>(), 20, 5)
                .GetComponent<LineRenderer>().sharedMaterial = GameManager.instance.ropeMaterial;
            l.maxSeparation = GameManager.ROPE_LENGTH;
            l.followDistance = GameManager.ROPE_LENGTH;
            b.maxSeparation = GameManager.ROPE_LENGTH;
            b.followDistance = GameManager.ROPE_LENGTH;
        }
        else {
            characters[0].maxSeparation = Mathf.Infinity;
            characters[1].maxSeparation = Mathf.Infinity;
            characters[1].followDistance = 4f;
            characters[0].partner = characters[1];
            characters[1].partner = characters[0];
        }
    }

    public static void DisableCharacterInputs() {
        foreach(var character in instance.characters) {
            character.inputsEnabled = false;
        }
    }

    public static void EnableCharacterInputs() {
        foreach(var character in instance.characters) {
            character.inputsEnabled = true;
        }
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
