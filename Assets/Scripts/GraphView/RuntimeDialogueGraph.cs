using System;
using System.Collections.Generic;
using UnityEngine;


    [CreateAssetMenu(fileName = "FILENAME", menuName = "MENUNAME", order = 0)]
    public class RuntimeDialogueGraph : ScriptableObject
    {
        public string EntryNodeID;
        public List<RuntimeDialogueNode> AllNodes = new List<RuntimeDialogueNode>();
    }
    
    [Serializable]
    public class RuntimeDialogueNode
    {
        public string DialogueNodeID;
        public Espeaker speaker;
        public string DialogueText;
        public List<ChoiceData> Choices = new List<ChoiceData>();
        public string NextDialogueNodeID;
    }

    [Serializable]
    public class ChoiceData
    {
        public string ChoiceText;
        public ConditionsSC Condition;
        public string DestinationNodeID;
    }
