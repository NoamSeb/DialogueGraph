using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;


public class DSStartNode : DSNode
{
        public override void Initialize(string nodeName, DSGraphView dsGraphView, Vector2 position)
        {
            base.Initialize("START", dsGraphView, position);
            DialogueType = DSDialogueType.Start;
        }

public override void Draw()
{
    TextField dialogueNameTextField = DSElementUtility.CreateTextField(DialogueName, null, (ChangeEvent<string> evt) =>
    {
        var target = (TextField)evt.target;
        string newValue = evt.newValue;
        try
        {
            newValue = newValue.RemoveWhitespaces().RemoveSpecialCharacters();
        }
        catch { }

        bool wasEmpty = string.IsNullOrEmpty(DialogueName);
        bool nowEmpty = string.IsNullOrEmpty(newValue);
        if (wasEmpty && !nowEmpty)
            graphView.NameErrorsAmount = Math.Max(0, graphView.NameErrorsAmount - 1);
        else if (!wasEmpty && nowEmpty)
            graphView.NameErrorsAmount++;

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
    
    if (Saves.choicesInNode.Count > 0)
    {
        foreach (DSChoiceSaveData choiceData in Saves.choicesInNode)
        {
           var output = CreateSingleChoicePortForExisting(choiceData);
           outputContainer.Add(output);

        }
    }
    else
    {
        var output =CreateSingleChoicePortNew();
        outputContainer.Add(output);

    }


    RefreshExpandedState();
}

   
private Port CreateSingleChoicePortForExisting(DSChoiceSaveData choiceData)
{
    Port choicePort = this.CreatePort();
    choicePort.userData = choiceData;

    outputContainer.Add(choicePort);

    choicePort.MarkDirtyRepaint();
    RefreshExpandedState();
    MarkDirtyRepaint();

    return choicePort;
}


private Port CreateSingleChoicePortNew(string dropDownKey = "")
{
    DSChoiceSaveData newChoice = new DSChoiceSaveData();
    newChoice.SaveDropDownKeyChoice(dropDownKey);

    Saves.choicesInNode.Add(newChoice);

    return CreateSingleChoicePortForExisting(newChoice);
}

}
