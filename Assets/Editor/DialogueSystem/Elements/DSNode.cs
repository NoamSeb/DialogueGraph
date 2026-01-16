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
    public string Text { get; set; }
    public Espeaker Speaker { get; set; }
    public DSDialogueType DialogueType { get; set; }
    public DSGroup Group { get; set; }
    public DSNodeSaveData Saves { get; set; }

    protected DSGraphView graphView;
    private Color defaultBackgroundColor;

    private TextField _previewField;

    #region INITIALIZATION

    public virtual void Initialize(string nodeName, DSGraphView dsGraphView, Vector2 position)
    {
        ID = Guid.NewGuid().ToString();
        DialogueName = DSIOUtility.CheckNameWithOthers(nodeName);
        Text = "Dialogue text.";
        Speaker = Espeaker.None;

        Saves = new DSNodeSaveData
        {
            ChoicesInNode = new List<DSChoiceSaveData>()
        };

        graphView = dsGraphView;
        defaultBackgroundColor = new Color(29f / 255f, 29f / 255f, 30f / 255f);

        SetPosition(new Rect(position, Vector2.zero));

        mainContainer.AddToClassList("ds-node__main-container");
        extensionContainer.AddToClassList("ds-node__extension-container");
    }

    #endregion

    #region DRAW

    public virtual void Draw()
    {
        DrawTitle();
        DrawInput();
        DrawCustomData();
        RefreshExpandedState();
    }

    private void DrawTitle()
    {
        TextField nameField = DSElementUtility.CreateTextField(DialogueName, null, evt =>
        {
            string newValue = evt.newValue.RemoveWhitespaces().RemoveSpecialCharacters();

            if (string.IsNullOrEmpty(DialogueName) && !string.IsNullOrEmpty(newValue))
                graphView.NameErrorsAmount--;

            if (!string.IsNullOrEmpty(DialogueName) && string.IsNullOrEmpty(newValue))
                graphView.NameErrorsAmount++;

            DialogueName = newValue;
        });

        nameField.AddClasses(
            "ds-node__text-field",
            "ds-node__filename-text-field"
        );

        titleContainer.Insert(0, nameField);

        EnumField speakerField = new EnumField(Speaker);
        speakerField.RegisterValueChangedCallback(evt =>
        {
            Speaker = (Espeaker)evt.newValue;
        });

        titleContainer.Add(speakerField);
    }

    private void DrawInput()
    {
        Port inputPort = this.CreatePort(
            "Dialogue Connection",
            Orientation.Horizontal,
            Direction.Input,
            Port.Capacity.Multi
        );

        inputContainer.Add(inputPort);
    }

    private void DrawCustomData()
    {
        VisualElement container = new VisualElement();
        container.AddToClassList("ds-node__custom-data-container");

        Foldout foldout = DSElementUtility.CreateFoldout("Dialogue Text");

        TextField textField = DSElementUtility.CreateTextArea(Text, null, evt =>
        {
            Text = evt.newValue;
        });

        textField.AddClasses(
            "ds-node__text-field",
            "ds-node__quote-text-field"
        );

        foldout.Add(textField);

        DropdownField dropdown = DSElementUtility.CreateDropdownArea("Dialogue Key", "Choose key");
        FillCsvDropdown(dropdown);

        dropdown.RegisterValueChangedCallback(_ => UpdatePreview(dropdown));

        //_previewField = DSElementUtility.CreateTextField("", true);
        _previewField = DSElementUtility.CreateTextField("", null, null);
        _previewField.isReadOnly = true;

        
        _previewField.isReadOnly = true;

        foldout.Add(dropdown);
        foldout.Add(_previewField);

        container.Add(foldout);
        extensionContainer.Add(container);
    }

    #endregion

    #region CSV / PREVIEW

    protected void FillCsvDropdown(DropdownField dropdown)
    {
        dropdown.choices.Clear();
        dropdown.choices.AddRange(FantasyDialogueTable.FindAll_Keys());
    }

    private void UpdatePreview(DropdownField dropdown)
    {
        if (string.IsNullOrEmpty(dropdown.value))
        {
            _previewField.value = "";
            return;
        }

        List<string> values = FantasyDialogueTable.LocalManager
            .FindAllDialogueForKey(dropdown.value);

        List<string> langs = FantasyDialogueTable.LocalManager
            .FindAllDialogueForKey("idLng");

        _previewField.value = "";

        for (int i = 0; i < Mathf.Min(values.Count, langs.Count); i++)
        {
            _previewField.value += $"{langs[i]} : {values[i]}";
            if (i < values.Count - 1)
                _previewField.value += "\n";
        }
    }

    #endregion

    #region PORT MANAGEMENT

    public void DisconnectAllPorts()
    {
        DisconnectPorts(inputContainer);
        DisconnectPorts(outputContainer);
    }

    private void DisconnectPorts(VisualElement container)
    {
        if (container == null) return;

        foreach (Port port in container.Children().OfType<Port>())
        {
            if (port.connected)
                graphView.DeleteElements(port.connections);
        }
    }

    public bool IsStartingNode()
    {
        Port inputPort = inputContainer.Children().OfType<Port>().FirstOrDefault();
        return inputPort != null && !inputPort.connected;
    }

    #endregion

    #region STYLE

    public void SetErrorStyle(Color color)
    {
        mainContainer.style.backgroundColor = color;
    }

    public void ResetStyle()
    {
        mainContainer.style.backgroundColor = defaultBackgroundColor;
    }

    #endregion
    
    public void SetSpeaker(Espeaker speaker)
    {
        Speaker = speaker;
    }

}
