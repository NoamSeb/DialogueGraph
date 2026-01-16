using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;


[Serializable]
    public class DSNodeSaveData
    {
        //[field: SerializeField] public bool IsStart { get; set; }
        [field: SerializeField] public string ID { get; set; }
        [field: SerializeField] public string Name { get; set; }

        [field: SerializeField]
        public Espeaker Speaker
        {
            get; 
            set;
        }
        public void SaveSpeaker(Espeaker speaker)
        {
            Speaker = speaker;
            Debug.Log("Saved Speaker: " + Speaker);
        }
        
        public bool isMultipleChoice = false;
        
        public Dictionary<Port, List<VisualElement>> ConditionsMap = new Dictionary<Port, List<VisualElement>>();

        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public List<DSChoiceSaveData> ChoicesInNode { get; set; }
        [field: SerializeField] public string GroupID { get; set; }
        [field: SerializeField] public DSDialogueType DialogueType { get; set; }
        [field: SerializeField] public Vector2 Position { get; set; }
        [field: SerializeField] public string NextDialogueNodeID { get; set; }
    }
