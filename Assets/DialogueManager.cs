using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using NaughtyAttributes;
using UnityEngine.Serialization;
using UnityEngine.SocialPlatforms;
public enum language
{
    FR,
    EN,
}
public class DialogueManager : MonoBehaviour
{
    public DSGraphSaveDataSO runtimeGraph;
    
    [Header("PLAYER SETTINGS")]
    
    [SerializeField] private language languageSetting = language.FR;

    [Header("UI Elements")] public GameObject dialoguePanel;
    public TextMeshProUGUI SpeakerNameText;
    public TextMeshProUGUI DialogueText;

    [Header("Choice Button UI")] public Button ChoiceButtonPrefab;
    public Transform ChoiceButtonContainer;

    public Speakers SpeakersScriptable;
    private SpeakerInfo _currentSpeaker;

    private Dictionary<string, DSNodeSaveData> _nodeLookup = new Dictionary<string, DSNodeSaveData>();
    private DSNodeSaveData _currentNode;
    
    private bool _isWaitingForChoice = false;

    [Button]
    public void LoadCsv()
    {
        FantasyDialogueTable.Load();
    }

    private void Start()
    {
        
        FantasyDialogueTable.Load();
        foreach (var node in runtimeGraph.Nodes)
        {
            Debug.Log("Adding Node ID to Lookup: " + node.ID);
            _nodeLookup[node.ID] = node;
        }
        
        _currentNode = GetNextNode(GetNodeStart().choicesInNode[0].NodeID);
        
        if (_currentNode == null)
        {
            EndDialogue();
            return;
        }
        else
        {
            Debug.Log("Starting Dialogue at Node ID: " + _currentNode.ID);;
            ShowNode(_currentNode.ID);
        }
    }
    
    private DSNodeSaveData GetNextNode(string nextID)
    {
        Debug.Log("Looking for Next Node ID: " + nextID);
        if (!string.IsNullOrEmpty(nextID) && _nodeLookup.TryGetValue(nextID, out var node))
        {
            Debug.Log("Next Node ID found: " + nextID);
            return node;
        }

        Debug.Log("Next Node ID not found or is empty.");

        return null;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !_isWaitingForChoice)
        {
            if (_currentNode == null)
            {
                Debug.Log("Current Node is null on click.");
                return;
            }
            
            Debug.Log("Advancing dialogue from Node ID: " + _currentNode.ID + "TO " + _currentNode.choicesInNode[0].NodeID);
            if (!string.IsNullOrEmpty(_currentNode.choicesInNode[0].NodeID))
            {
                ShowNode(_currentNode.choicesInNode[0].NodeID);
            }
            else
            {
                EndDialogue();
            }
        }
    }

    private void ShowNode(string nodeID)
    {
        if (!_nodeLookup.TryGetValue(nodeID, out var value))
        {
            Debug.Log("Node ID not found: " + nodeID);
            EndDialogue();
            return;
        }

        _currentNode = value;
        if (_currentNode == null)
        {
            Debug.Log("Current Node is null for ID: " + nodeID);
            EndDialogue();
            return;
        }

        dialoguePanel.SetActive(true);
        ChangeSpeaker(_currentNode.Speaker);
        

        DialogueText.SetText(FantasyDialogueTable.LocalManager.FindDialogue(_currentNode.GetDropDownKeyDialogue(), Enum.GetName(typeof(language), languageSetting)));

        foreach (Transform child in ChoiceButtonContainer)
        {
            Destroy(child.gameObject);
        }

        if (_currentNode.choicesInNode.Count > 1)
        {
            _isWaitingForChoice = true;
            foreach (DSChoiceSaveData choice in _currentNode.choicesInNode)
            {
                Button choiceButton = Instantiate(ChoiceButtonPrefab, ChoiceButtonContainer);
                
                choiceButton.onClick.AddListener(() =>
                {
                    _isWaitingForChoice = false;
                    Debug.Log("Player selected choice leading to Node ID: " + choice.NodeID);
                    ShowNode(choice.NodeID);
                    
                });

                TextMeshProUGUI choiceText = choiceButton.GetComponentInChildren<TextMeshProUGUI>();
                if (choiceText != null)
                {
                    choiceText.SetText(FantasyDialogueTable.LocalManager.FindDialogue(choice.GetDropDownKeyChoice(), Enum.GetName(typeof(language), languageSetting)));
                }
            }
        }
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        _currentNode = null;

        foreach (Transform child in ChoiceButtonContainer)
        {
            Destroy(child.gameObject);
        }
    }

    public void ChangeSpeaker(Espeaker speak)
    {
        foreach (var speaker in SpeakersScriptable.speakers)
        {
            if (speaker.speakEnum == speak)
            {
                SetNewSpeaker(speaker);
            }
        }
    }

    private void SetNewSpeaker(SpeakerInfo speaker)
    {
        _currentSpeaker = speaker;
        SpeakerNameText.SetText(_currentSpeaker.Name);
    }

    private DSNodeSaveData GetNodeStart()
    {
        foreach (var node in runtimeGraph.Nodes)
        {
            if (node.DialogueType == DSDialogueType.Start)
            {
                return node;
            }
        }
        return null;
    }
}