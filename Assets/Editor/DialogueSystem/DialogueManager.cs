using System.Collections.Generic;
using System.Linq;
using DS.Data.Save;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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

    private void Start()
    {
        foreach (var node in runtimeGraph.Nodes)
        {
            _nodeLookup[node.GroupID] = node;
        }

        if (!string.IsNullOrEmpty(runtimeGraph.Nodes.FirstOrDefault()?.GroupID))
        {
            ShowNode(runtimeGraph.Nodes.FirstOrDefault()?.GroupID);
        }
        else
        {
            EndDialogue();
        }
    }

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame && _currentNode != null && _currentNode.Choices.Count == 0)
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
        print(_currentNode.speaker);
        ChangeSpeaker(_currentNode.speaker);

        DialogueText.SetText(_currentNode.Text);

        foreach (Transform child in ChoiceButtonContainer)
        {
            Destroy(child.gameObject);
        }

        if (_currentNode.Choices.Count > 0)
        {
            foreach (DSChoiceSaveData choice in _currentNode.Choices)
            {
                Button choiceButton = Instantiate(ChoiceButtonPrefab, ChoiceButtonContainer);

                TextMeshProUGUI choiceText = choiceButton.GetComponentInChildren<TextMeshProUGUI>();
                if (choiceText != null)
                {
                    choiceText.text = choice.Text;
                }

                // if (choiceButton != null)
                // {
                //     choiceButton.onClick.AddListener(() =>
                //     {
                //         if (!string.IsNullOrEmpty(choice.DestinationNodeID)) // SI LE STRING N'EST PAS VIDE
                //         {
                //             ShowNode(choice.DestinationNodeID);
                //         }
                //         else // SINON FIN DE DIALOGU
                //         {
                //             EndDialogue();
                //         }
                //     });
                // }
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