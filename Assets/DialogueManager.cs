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
public enum bubleType
{
    NORMAL,
    THINK,
    SHOUT,
}


public class DialogueManager : MonoBehaviour
{
    public DSGraphSaveDataSO runtimeGraph;
    
    [Header("PLAYER SETTINGS")]
    
    [SerializeField] private language languageSetting = language.FR;

    [Header("UI Elements")]
    
    private Dictionary<bubleType, dialogueContainer> _bubleContainers = new Dictionary<bubleType, dialogueContainer>();
    [SerializeField] private List<dialogueContainer> _bubleContainerList = new List<dialogueContainer>();

    [Header("Choice Button UI")] public Button ChoiceButtonPrefab;
    public Transform ChoiceButtonContainer;

    public Speakers SpeakersScriptable;
    private SpeakerInfo _currentSpeaker;

    private Dictionary<string, DSNodeSaveData> _nodeLookup = new Dictionary<string, DSNodeSaveData>();
    private DSNodeSaveData _currentNode;
    
    private bool _isWaitingForChoice = false;
    
    private dialogueContainer _currentDialogueContainer;
    private dialogueContainer _oldDialogueContainer;

    [Button]
    public void LoadCsv()
    {
        FantasyDialogueTable.Load();
    }

    private void Awake()
    {
        // ON FAIT CA EN BRUT PRCQ NSM
        
        _bubleContainers.Add(bubleType.NORMAL, _bubleContainerList[0]);
        _bubleContainers.Add(bubleType.THINK, _bubleContainerList[1]);
        _bubleContainers.Add(bubleType.SHOUT, _bubleContainerList[2]);
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
        
        ChangeSpeaker(_currentNode.Speaker);
        string target = FantasyDialogueTable.LocalManager.FindDialogue(_currentNode.GetDropDownKeyDialogue(), Enum.GetName(typeof(language), languageSetting));
        _currentDialogueContainer.InitializeDialogueContainer(target, _currentSpeaker.Name);

        foreach (Transform child in ChoiceButtonContainer)
        {
            Destroy(child.gameObject);
        }
        
        CreateButtonsChoice();
    }

    private void CreateButtonsChoice()
    {
        if (_currentNode.choicesInNode.Count > 1)
        {
            _isWaitingForChoice = true;
            foreach (DSChoiceSaveData choice in _currentNode.choicesInNode)
            {
                Button choiceButton = Instantiate(ChoiceButtonPrefab, ChoiceButtonContainer);
                buttonChoiceController buttonController = choiceButton.GetComponent<buttonChoiceController>();
                if (buttonController != null)
                {
                    bool fillCondition = true;
                    foreach (var condition in choice.Conditions)
                    {
                        Debug.Log("Evaluating condition for choice: " + condition.conditionItem);
                        fillCondition = DoesFillCondtions(condition);
                        if (!fillCondition)
                        {
                            Debug.Log("Choice locked due to unmet condition: " + condition.conditionItem);
                            break;
                        }
                    }
                    string textButton = FantasyDialogueTable.LocalManager.FindDialogue(choice.GetDropDownKeyChoice(), Enum.GetName(typeof(language), languageSetting));
                    buttonController.InitializeButtonChoiceController(fillCondition, textButton);
                }
                
                choiceButton.onClick.AddListener(() =>
                {
                    _isWaitingForChoice = false;
                    Debug.Log("Player selected choice leading to Node ID: " + choice.NodeID);
                    ShowNode(choice.NodeID);
                });
            }
        }
    }
    
    private bool DoesFillCondtions(ConditionsSC choice)
    {
        Debug.Log("Checking if player fills condition: " + choice.conditionItem);
        return PlayerInventoryManager.instance.DoesPlayerFillCondition(choice);
    }

    private void EndDialogue()
    {
        _currentDialogueContainer.HideContainer();
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

        foreach (var humeur in _currentSpeaker.SpritesHumeur)
        {
            if (_currentNode.Humeur == humeur.humeur)
            {
                SpriteSpeakerHumeur.sprite = humeur.sprite;
            }
        }
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