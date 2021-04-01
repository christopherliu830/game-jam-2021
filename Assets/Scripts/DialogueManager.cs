using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Tooltip("Folder that stores the dialogue files -- should be located under Resources")][SerializeField] private string path_to_dialogue_files;
    [Tooltip("Where the character's name should be displayed on the UI")][SerializeField] private TextMeshProUGUI character_name_box = null;
    [Tooltip("Where the character's speech should be displayed on the UI")][SerializeField] private TextMeshProUGUI character_speech_box = null;
    
    private List<TextAsset> dialogue_files;
    
    // Singleton pattern
    private static DialogueManager _instance;
    public static DialogueManager Instance {get {return _instance; }}
    
    private void Awake()
    {
        if (_instance != null & _instance != this) Destroy(this.gameObject);
        else _instance = this;
        dialogue_files = GetDialogueFiles(path_to_dialogue_files);
        StartCoroutine(PlayDialogue(0));
    }
    
    // Get files from a subfolder under Resources
    private List<TextAsset> GetDialogueFiles (string path)
    {
        List<TextAsset> results = new List<TextAsset>();
        try
        {
            foreach (TextAsset t in Resources.LoadAll(path, typeof(TextAsset)))
            {
                results.Add(t);
            }
            return results;
        }
        catch
        {
            Debug.LogError("DialogueManager: Failed to find " + path);
            return results;
        }
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
    
    public IEnumerator PlayDialogue(int index)
    {
        if (index > dialogue_files.Count) 
        {
            Debug.LogError("DialogueManager: Index exceeds the length of dialogue_files");
            yield return null;
        }
        string[] lines = GetLines(dialogue_files[index]);
        foreach (string line in lines)
        {
            string[] tokens = TokenizeLine(line, '\t');
            yield return StartCoroutine(UpdateDialogueBox(tokens));
        }
        ClearDialogueBox();
    }
    
    private IEnumerator UpdateDialogueBox(string[] tokens)
    {
        Debug.Log(tokens[0]);
        Debug.Log(tokens[1]);
        do
        {
            character_name_box.text = tokens[0];
            character_speech_box.text = tokens[1];
            yield return null;
        }
        while (!Input.GetKeyDown(KeyCode.Mouse0) && !Input.GetKey(KeyCode.Mouse1)); 
    }
    
    public void ClearDialogueBox()
    {
        character_name_box.text = "";
        character_speech_box.text = "";
    }
}
