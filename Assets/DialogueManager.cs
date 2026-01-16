using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using NaughtyAttributes;
using UnityEngine.SocialPlatforms;

public class DialogueManager : MonoBehaviour
{
    public DSGraphSaveDataSO runtimeGraph;

    [Header("UI Elements")] public GameObject dialoguePanel;
    public TextMeshProUGUI SpeakerNameText;
    public TextMeshProUGUI DialogueText;

    [Header("Choice Button UI")] public Button ChoiceButtonPrefab;
    public Transform ChoiceButtonContainer;

    public Speakers SpeakersScriptable;
    private SpeakerInfo _currentSpeaker;

    private Dictionary<string, DSNodeSaveData> _nodeLookup = new Dictionary<string, DSNodeSaveData>();
    private DSNodeSaveData _currentNode;

    [Button]
    public void LoadCsv()
    {
        FantasyDialogueTable.Load();
    }

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
        if (Mouse.current.leftButton.wasPressedThisFrame && _currentNode != null && _currentNode.choicesInNode.Count == 0)
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
        //SpeakerNameText.SetText(_currentNode.SpeakerName);
        print(_currentNode.Speaker);
        ChangeSpeaker(_currentNode.Speaker);

        DialogueText.SetText(_currentNode.Text);

        foreach (Transform child in ChoiceButtonContainer)
        {
            Destroy(child.gameObject);
        }

        if (_currentNode.choicesInNode.Count > 0)
        {
            foreach (DSChoiceSaveData choice in _currentNode.choicesInNode)
            {
                Button choiceButton = Instantiate(ChoiceButtonPrefab, ChoiceButtonContainer);

                TextMeshProUGUI choiceText = choiceButton.GetComponentInChildren<TextMeshProUGUI>();
                if (choiceText != null)
                {
                    choiceText.text = "TEST";
                    choiceText.text = choice.DropDownKey; // Assure-toi que Text est rempli dans DSChoiceSaveData
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
        print(_currentSpeaker.Name);
    }
}