using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class DSNode : Node
{
    public string ID { get; set; }
    public string DialogueName { get; set; }
    public Espeaker Speaker { get; set; }
    public bubleType BubleType { get; set; }
    public HumeurSpeaker Humeur { get; set; }
    public DSNodeSaveData Saves { get; set; }
    public string Text { get; set; }

    public DropdownField DialogueTypeField { get; set; }
    public Label LanguageLabel { get; set; }
    public DSDialogueType DialogueType { get; set; }
    public DSGroup Group { get; set; }

    protected DSGraphView graphView;

    private Color defaultBackgroundColor;

    private TextField _fieldLabel;
    
    private DropdownField _dropdownFieldDialogue;

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        evt.menu.AppendAction("Disconnect Input Ports", actionEvent => DisconnectInputPorts());
        evt.menu.AppendAction("Disconnect Output Ports", actionEvent => DisconnectOutputPorts());

        base.BuildContextualMenu(evt);
    }

    public virtual void Initialize(string nodeName, DSGraphView dsGraphView, Vector2 position)
    {
        ID = Guid.NewGuid().ToString();

        DialogueName = nodeName;

        Saves = new DSNodeSaveData();
        Saves.SetChoices(new List<DSChoiceSaveData>());

        Text = "Dialogue text.";
        SetPosition(new Rect(position, Vector2.zero));

        graphView = dsGraphView;
        defaultBackgroundColor = new Color(29f / 255f, 29f / 255f, 30f / 255f);

        mainContainer.AddToClassList("ds-node__main-container");
        extensionContainer.AddToClassList("ds-node__extension-container");
    }

    public virtual void Draw()
    {
        /* TITLE CONTAINER */

        TextField dialogueNameTextField = DSElementUtility.CreateTextField(DialogueName, null,
            (ChangeEvent<string> evt) =>
            {
                var target = (TextField)evt.target;
                string newValue = evt.newValue;
                try
                {
                    newValue = newValue.RemoveWhitespaces().RemoveSpecialCharacters();
                }
                catch
                {
                }

                bool wasEmpty = string.IsNullOrEmpty(DialogueName);
                bool nowEmpty = string.IsNullOrEmpty(newValue);
                if (wasEmpty && !nowEmpty)
                {
                    graphView.NameErrorsAmount = Math.Max(0, graphView.NameErrorsAmount - 1);
                }
                else if (!wasEmpty && nowEmpty)
                {
                    graphView.NameErrorsAmount++;
                }

                if (Group == null)
                {
                    graphView.RemoveUngroupedNode(this);
                    DialogueName = newValue;
                    graphView.AddUngroupedNode(this);
                }
                else
                {
                    var currentGroup = Group;
                    graphView.RemoveGroupedNode(this, currentGroup);
                    DialogueName = newValue;
                    graphView.AddGroupedNode(this, currentGroup);
                }

                target.value = newValue;
            });

        dialogueNameTextField.AddToClassList("ds-node__text-field");
        dialogueNameTextField.AddToClassList("ds-node__text-field__hidden");
        dialogueNameTextField.AddToClassList("ds-node__filename-text-field");

        titleContainer.Insert(0, dialogueNameTextField);

        EnumField speakerEnumField = new EnumField("Character : ", Speaker);
        speakerEnumField.RegisterValueChangedCallback(callback => SetSpeaker((Espeaker)callback.newValue));
        titleContainer.Add(speakerEnumField);
        
        
        EnumField humeurEnumField = new EnumField("Mood : ", Humeur);
        humeurEnumField.RegisterValueChangedCallback(callback => SetHumeur((HumeurSpeaker)callback.newValue));
        titleContainer.Add(humeurEnumField);
        
        EnumField UISpeakerEnumField = new EnumField("Buble type : ", BubleType);
        UISpeakerEnumField.RegisterValueChangedCallback(callback => BubleType = ((bubleType)callback.newValue));
        titleContainer.Add(UISpeakerEnumField);
        

        /* INPUT CONTAINER */
        Port inputPort = this.CreatePort("Dialogue Connection", Orientation.Horizontal, Direction.Input,
            Port.Capacity.Multi);
        inputContainer.Add(inputPort);

        /* EXTENSION CONTAINER */
        VisualElement customDataContainer = new VisualElement();
        customDataContainer.AddToClassList("ds-node__custom-data-container");

        Foldout textFoldout = DSElementUtility.CreateFoldout("Dialogue Text");

        _dropdownFieldDialogue = DSElementUtility.CreateDropdownArea("Dialogue Key", "Choose a key ->");
        FillCsvDropdown(_dropdownFieldDialogue);
        

        _dropdownFieldDialogue.RegisterValueChangedCallback((ChangeEvent<string> evt) => OnDropdownEvent(_dropdownFieldDialogue));

        textFoldout.Add(_dropdownFieldDialogue);

        _fieldLabel = DSElementUtility.CreateTextField("waiting for key...");
        textFoldout.Add(_fieldLabel);

        if (Saves.GetDropDownKeyDialogue() != "")
        {
            _dropdownFieldDialogue.value = Saves.GetDropDownKeyDialogue();
            OnDropdownEvent(_dropdownFieldDialogue);
        }

        customDataContainer.Add(textFoldout);
        extensionContainer.Add(customDataContainer);
    }

    public void DisconnectAllPorts()
    {
        DisconnectInputPorts();
        DisconnectOutputPorts();
    }

    private void OnDropdownEvent(DropdownField dropdownField)
    {
        if (_fieldLabel == null)
            return;

        _fieldLabel.value = FantasyDialogueTable.LocalManager.GetAllDialogueFromValue(dropdownField.value);
        Saves.SaveDropDownKeyDialogue(dropdownField.value);
        

    }

    public void FillCsvDropdown(DropdownField dropdownField)
    {
        dropdownField.choices.Clear();
        
        string speakerName = null;
        if (Speaker != 0)
        {
            speakerName = Enum.GetName(typeof(Espeaker), Speaker);
        }
        List<string> keys = FantasyDialogueTable.FindAll_Keys(speakerName);
        if (keys == null) return;
        foreach (string key in keys)
        {
            dropdownField.choices.Add(key);
        }
    }

    private void DisconnectInputPorts()
    {
        DisconnectPorts(inputContainer);
    }

    private void DisconnectOutputPorts()
    {
        DisconnectPorts(outputContainer);
    }

    private void DisconnectPorts(VisualElement container)
    {
        if (container == null)
        {
            return;
        }

        foreach (var visualElement in container.Children().ToList())
        {
            var port = visualElement as Port;
            if (port == null)
            {
                continue;
            }

            if (!port.connected)
            {
                continue;
            }

            // Suppression des connexions existantes
            graphView.DeleteElements(port.connections);
        }
    }

    public bool IsStartingNode()
    {
        if (!inputContainer.Children().Any())
            return true;

        Port inputPort = (Port)inputContainer.Children().First();
        return !inputPort.connected;
    }

    public void SetErrorStyle(Color color)
    {
        mainContainer.style.backgroundColor = color;
    }

    public void ResetStyle()
    {
        mainContainer.style.backgroundColor = defaultBackgroundColor;
    }

    public void SetSpeaker(Espeaker speaker)
    {
        Speaker = speaker;
        string speakerName = null;
        if (Speaker != 0)
        {
            speakerName = Enum.GetName(typeof(Espeaker), Speaker);
        }

        List<string> keys = FantasyDialogueTable.FindAll_Keys(speakerName);
        Debug.Log(keys.Count);
        if(_dropdownFieldDialogue == null) return;
        _dropdownFieldDialogue.choices.Clear();
        foreach (string key in keys)
        {
            _dropdownFieldDialogue.choices.Add(key);
        }
    }
    
    public void SetHumeur(HumeurSpeaker humeur)
    {
        Humeur = humeur;
    }

}
