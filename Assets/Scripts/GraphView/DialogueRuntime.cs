using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialogueRuntime : MonoBehaviour
{
    public DSGraphSaveDataSO runtimeGraph;

    private DSNodeSaveData _currentNode;
    
    private Dictionary<string, DSNodeSaveData> _nodeLookup = new Dictionary<string, DSNodeSaveData>();

    [Header("UI Elements")] public GameObject dialoguePanel;
    public TextMeshProUGUI SpeakerNameText;
    public TextMeshProUGUI DialogueText;

    [Header("Choice Button UI")] public Button ChoiceButtonPrefab;
    public Transform ChoiceButtonContainer;

    public Speakers SpeakersScriptable;
    private SpeakerInfo _currentSpeaker;


    private void Start()
    {
        // Remplir le dictionnaire avec les ID des nodes
        foreach (var node in runtimeGraph.Nodes)
        {
            if (!string.IsNullOrEmpty(node.ID))
                _nodeLookup[node.ID] = node;
        }

        // Récupérer le node de départ
        var startNode = runtimeGraph.Nodes.FirstOrDefault();
        if (startNode != null && !string.IsNullOrEmpty(startNode.ID))
        {
            ShowNode(startNode.ID);
        }
        else
        {
            print("end");
            EndDialogue();
        }
    }

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame && _currentNode != null && _currentNode.ChoicesInNode.Count == 0)
        {
            if (!string.IsNullOrEmpty(_currentNode.NextDialogueNodeID))
            {
                ShowNode(_currentNode.NextDialogueNodeID);
            }
            else
            {
                EndDialogue();
            }
        }
    }

    private void ShowNode(string nodeID)
    {
        if (!_nodeLookup.ContainsKey(nodeID))
        {
            EndDialogue();
            return;
        }

        _currentNode = _nodeLookup[nodeID];

        dialoguePanel.SetActive(true);
        //SpeakerNameText.SetText(_currentNode.Speaker);
        print(_currentNode.Speaker);
        ChangeSpeaker(_currentNode.Speaker);

        DialogueText.SetText(_currentNode.Text);

        foreach (Transform child in ChoiceButtonContainer)
        {
            Destroy(child.gameObject);
        }

        if (_currentNode.ChoicesInNode.Count > 0)
        {
            foreach (DSChoiceSaveData choice in _currentNode.ChoicesInNode)
            {
                Button choiceButton = Instantiate(ChoiceButtonPrefab, ChoiceButtonContainer);

                TextMeshProUGUI choiceText = choiceButton.GetComponentInChildren<TextMeshProUGUI>();
                if (choiceText != null)
                {
                    choiceText.text = choice.DropDownKey;
                }

                DSChoiceSaveData cachedChoice = choice;

                choiceButton.onClick.AddListener(() =>
                {
                    if (!string.IsNullOrEmpty(cachedChoice.NodeID))
                    {
                        ShowNode(cachedChoice.NodeID);
                    }
                    else
                    {
                        EndDialogue();
                    }
                });
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
        //SpeakerNameText.Text
        print("je suis là " + _currentSpeaker.Name);
    }
}
