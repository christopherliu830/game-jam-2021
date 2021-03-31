using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static readonly float GRAVITY = -3f;
    public float ropeDistance = 12f;
    public Material ropeMaterial;
    public CharController[] characters;
    
    private int _currentCharacterIndex = 0;

    void Start()
    {
        characters[_currentCharacterIndex].Focus();
        characters[0].maxSeparation = ropeDistance;
        characters[1].maxSeparation = ropeDistance;
        characters[0].partner = characters[1];
        characters[1].partner = characters[0];
        HingeJointConnect.Connect(
            characters[0].GetComponent<Rigidbody2D>(), 
            characters[1].GetComponent<Rigidbody2D>(), 20, 5)
            .GetComponent<LineRenderer>().sharedMaterial = ropeMaterial;

    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift)) {
            characters[_currentCharacterIndex].Unfocus();
            _currentCharacterIndex = (_currentCharacterIndex + 1) % characters.Length;
            characters[_currentCharacterIndex].Focus();
            Camera.main.GetComponent<FollowTargets>().Focus(characters[_currentCharacterIndex].transform);
        }

        if (Input.GetKeyDown(KeyCode.R)) {
            var index = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(index);
        }
    }
}
