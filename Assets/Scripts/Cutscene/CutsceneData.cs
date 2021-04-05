using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "CutsceneData", menuName = "CutsceneData", order = 0)]
public class CutsceneData : ScriptableObject {
    public List<ImageDialogue> scenes;
}

[System.Serializable]
public struct ImageDialogue {
    public Sprite image;
    public List<string> texts;
}
