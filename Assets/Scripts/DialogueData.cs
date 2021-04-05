using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "DialogueData", menuName = "DialogueData", order = 0)]
public class DialogueData : ScriptableObject {
    public float triggerDelay;
    public NamePortrait portraits;
    public List<TextAsset> texts;
}

[System.Serializable]
public class NamePortrait : SerializableDictionary<string, Sprite> { }