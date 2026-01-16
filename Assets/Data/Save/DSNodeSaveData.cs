using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;


[Serializable]
public class DSNodeSaveData
{
    [field: SerializeField] public string ID { get; set; }
    [field: SerializeField] public string Name { get; set; }
    [field: SerializeField] public Espeaker Speaker { get; set; }

    public void SaveSpeaker(Espeaker speaker)
    {
        Speaker = speaker;
    }

    public bool isMultipleChoice = false;

    public Dictionary<Port, List<VisualElement>> ConditionsMapElement = new Dictionary<Port, List<VisualElement>>();
    public Dictionary<Port, List<ConditionsSC>> ConditionsMapSc = new Dictionary<Port, List<ConditionsSC>>();

    private string _dropDownKeyDialogue;

    public void SaveDropDownKeyDialogue(string key)
    {
        _dropDownKeyDialogue = key;
    }

    public string GetDropDownKeyDialogue()
    {
        return _dropDownKeyDialogue;
    }

    [field: SerializeField] public string Text { get; set; }
    
    public void SetChoices(List<DSChoiceSaveData> choicesSaveData)
    {
        choicesInNode = choicesSaveData;
        Debug.Log("Choices SET in Node Save Data: " + choicesInNode.Count);
    }
    public void AddChoice(DSChoiceSaveData choiceSaveData)
    {
        choicesInNode.Add(choiceSaveData);
        Debug.Log("Choice ADDED in Node Save Data: " + choicesInNode.Count);
    }
    [field: SerializeField] public List<DSChoiceSaveData> choicesInNode { get; private set; }
    
    
    [field: SerializeField] public string GroupID { get; set; }
    [field: SerializeField] public DSDialogueType DialogueType { get; set; }
    [field: SerializeField] public Vector2 Position { get; set; }
    [field: SerializeField] public string NextDialogueNodeID { get; set; }
}
