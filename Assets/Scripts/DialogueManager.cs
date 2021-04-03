using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    private static DialogueManager _instance;
    public static DialogueManager Instance => _instance;

    [SerializeField] 
    private GameObject dialogueBox = null;

    [Tooltip("Where the character's name should be displayed on the UI")]
    [SerializeField] 
    private Image nameBox;
    
    [Tooltip("Where the character's speech should be displayed on the UI")]
    [SerializeField] 
    private TextMeshProUGUI speechBox;
    
    
    private void Awake()
    {
        if (_instance != null & _instance != this) Destroy(this.gameObject);
        else _instance = this;
        dialogueBox.SetActive(false);
    }

    // Returns the lines contained in a file
    // Chose this method over System.IO in case we want to do a WebGL build
    private string[] GetLines (TextAsset text)
    {
        try
        {
            string[] results = text.text.Split('\n');
            return results;
        }
        catch
        {
            Debug.LogError("DialogueManager: Could not read the lines stored in " + text);
            return new string[0];
        }
    }
    
    // Returns the contents of a line
    private string[] TokenizeLine (string line, char delimiter)
    {
        return line.Split(delimiter);
    }

    public static void Play(DialogueData data) {
        Instance.dialogueBox.SetActive(true);
        Instance.nameBox.enabled = true;
        Instance.StartCoroutine(PlayDialogue(data));
    }
    
    public static IEnumerator PlayDialogue(DialogueData data)
    {
        yield return new WaitForSeconds(data.triggerDelay);
        string[] lines = Instance.GetLines(data.text);
        GameManager.DisableCharacterInputs();

        foreach (string line in lines)
        {
            string[] tokens = Instance.TokenizeLine(line, '\t');
            string name = tokens[0];
            Sprite portrait = data.portraits[name];
            string speech = tokens[1];
            yield return Instance.StartCoroutine(Instance.UpdateDialogueBox(name, portrait, speech));
        }
        Instance.ClearDialogueBox();
        Instance.dialogueBox.SetActive(false);
        GameManager.EnableCharacterInputs();
    }
    
    private IEnumerator UpdateDialogueBox(string name, Sprite portrait, string speech)
    {
        nameBox.sprite = portrait;
        speechBox.text = speech;
        yield return new WaitForSeconds(0.5f);
        yield return new WaitWhile(() => !Input.GetKeyDown(KeyCode.Mouse0));
    }
    
    public void ClearDialogueBox()
    {
        nameBox.sprite = null;
        speechBox.text = "";
    }
    
}
